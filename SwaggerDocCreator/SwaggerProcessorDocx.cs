using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.Words;
using Aspose.Words.Tables;
using NJsonSchema;
using NSwag;

namespace SwaggerDocCreator;

class SwaggerProcessorDocx
{
    static SwaggerProcessorDocx()
    {
        Nd.Update();
    }

    public void Process(string input, string fontPath, string fontFamily, string output, string[] argTags)
    {
        OpenApiDocument swaggerDocument;
        if (input.StartsWith("http"))
        {
            swaggerDocument = OpenApiDocument.FromUrlAsync(input).Result;
        }
        else
        {
            swaggerDocument = NSwag.OpenApiDocument.FromFileAsync(input).Result;
        }

        //var filename = Path.Combine(Path.GetPathRoot(input), Path.GetFileNameWithoutExtension(input) + ".pdf");
        var doc = new Document();
        var document = new DocumentBuilder(doc);
        //                document.SetFont(PdfFontFactory.CreateRegisteredFont("helvetica"));
        document.Writeln(swaggerDocument.Info.Title);
        document.Writeln("Version: " + swaggerDocument.Info.Version);
        document.Writeln(swaggerDocument.Info.Description ?? "");

        int indexTag = 0, indexOper = 0;
        foreach (var (tag, pairs) in swaggerDocument.Paths.GroupBy(x =>
                     x.Value.Values.FirstOrDefault().Tags.FirstOrDefault()))
        {
            if (argTags.Length > 0 && argTags.All(e => e != tag))
            {
                continue;
            }

            indexTag++;
            indexOper = 0;
            document.InsertBreak(BreakType.LineBreak);
            document.Bold = true;
            document.Writeln($"{indexTag}. {tag}");
            document.Bold = false;
            foreach (var (path, operations) in pairs)
            {
                foreach (var (method, operation) in operations)
                {
                    indexOper++;
                    document.Writeln($"{indexTag}.{indexOper}. {operation.Summary}");
                    document.Writeln("Method: " + method);
                    document.Writeln("Url: " + path);

                    var parameters = operation.ActualParameters ?? Array.Empty<OpenApiParameter>();
                    if (parameters.Any())
                    {
                        RenderParmeter(document, parameters);
                    }

                    var responses = operation.ActualResponses.Where(x => x.Key == "200").Select(x => x.Value)
                        .FirstOrDefault();
                    if (responses != null)
                    {
                        RenderResponse(document, responses);
                    }

                    document.InsertBreak(BreakType.PageBreak);
                }
            }

            doc.Save(output);
        }
    }

    private void RenderResponse(DocumentBuilder document, OpenApiResponse response)
    {
        if (response.Schema == null)
        {
            return;
        }

        var childs = new Dictionary<string, JsonSchema>();
        document.Writeln("返回值");
        var table = document.StartTable();
        document.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明").EndRow();
        // table.PreferredWidth = PreferredWidth.FromPercent(100);
        foreach (var (field, prop) in response.Schema.Properties)
        {
            FillTable(field, prop, document, childs);
        }

        table.SetAllCellWidth();

        document.EndTable();
        RenderChildren(document, childs);
    }

    private void RenderParmeter(DocumentBuilder document, IEnumerable<OpenApiParameter> parameters)
    {
        var childs = new Dictionary<string, JsonSchema>();

        foreach (var ps in parameters.GroupBy(x => x.Kind).OrderBy(x => x.Key != OpenApiParameterKind.Header))
        {
            switch (ps.Key)
            {
                case OpenApiParameterKind.Undefined:
                    continue;
                case OpenApiParameterKind.Body:
                case OpenApiParameterKind.Query:
                case OpenApiParameterKind.Path:
                case OpenApiParameterKind.FormData:
                case OpenApiParameterKind.ModelBinding:
                    document.Writeln($"{ps.Key:G}参数:");
                    break;
                case OpenApiParameterKind.Header:
                    document.Writeln("Header:");
                    break;
            }

            var table = document.StartTable();
            document.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明").EndRow();
            // table.PreferredWidth = PreferredWidth.FromPercent(100);
            foreach (var parameter in ps)
            {
                if (parameter.Kind == OpenApiParameterKind.Body)
                {
                    var schema = parameter.ActualSchema;

                    foreach (var (field, property) in schema.ActualProperties)
                    {
                        FillTable(field, property, document, childs);
                    }
                }
                else
                {
                    FillTable(parameter.Name, parameter, document, childs);
                }
            }

            table.SetAllCellWidth();

            document.EndTable();
        }

        RenderChildren(document, childs);
    }

    private void RenderChildren(DocumentBuilder document, Dictionary<string, JsonSchema> childs)
    {
        while (childs.Any())
        {
            var key = childs.Keys.FirstOrDefault();
            var jsonSchema4 = childs[key];
            childs.Remove(key);
            document.Writeln(key);
            var table = document.StartTable();
            document.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明").EndRow();
            foreach (var (field, property) in jsonSchema4.ActualProperties)
            {
                FillTable(field, property, document, childs);
            }

            table.SetAllCellWidth();
            document.EndTable();
        }
    }

    private void FillTable(string field, JsonSchema property, DocumentBuilder table,
        Dictionary<string, JsonSchema> childs)
    {
        var isAllowNull = !((dynamic) property).IsRequired;
        var desc = property.Description ?? "";
        property = property.ActualTypeSchema;
        table.AddCell(field);
        string typeName = field;
        if (property.ExtensionData != null)
        {
            if (property.ExtensionData.TryGetValue("typeInfo", out var typeNameO))
            {
                typeName = typeNameO?.ToString() ?? (field + "Object");
            }
        }

        switch (property.Type)
        {
            case JsonObjectType.Array:
                typeName = Regex.Replace(typeName, @"^List\[(.+)\]$", "$1[]");
                childs.Add(typeName.Replace("[]", ""), property.Item.ActualTypeSchema);
                break;
            case JsonObjectType.Object:
                //table.AddCell(typeName);
                childs.Add(typeName, property.ActualTypeSchema);
                break;
            case JsonObjectType.None:
                break;
            default:
                typeName = property.Type.ToString();
                if (property.Type == JsonObjectType.String)
                {
                    switch (property.Format)
                    {
                        case "date-time":
                            typeName = "DateTime";
                            break;
                        case "date":
                            typeName = "Date";
                            break;
                    }
                }

                //table.AddCell(property.Type.ToString());
                break;
        }

        table.AddCell(typeName);
        table.AddCell(isAllowNull ? "是" : "否");
        table.AddCell(desc);
        table.EndRow();
    }
}

internal static class DocumentBuilderExt
{
    public static DocumentBuilder AddHeaderCell(this DocumentBuilder b, string n)
    {
        b.InsertCell();
        b.Write(n);
        return b;
    }

    public static DocumentBuilder AddCell(this DocumentBuilder b, string n)
    {
        b.InsertCell();
        b.Write(n);
        return b;
    }

    public static Table SetAllCellWidth(this Table table)
    {
        foreach (Row row in table.Rows)
        {
            foreach (Cell cell in row.Cells)
            {
                cell.CellFormat.WrapText = true;
                cell.CellFormat.PreferredWidth = PreferredWidth.FromPercent(100.0 / table.FirstRow.Cells.Count);
                // cell.CellFormat.Width = 4.5;
            }
        }

        table.PreferredWidth = PreferredWidth.FromPercent(100);
        table.AllowAutoFit = false;
        // table.AutoFit(AutoFitBehavior.AutoFitToWindow);
        return table;
    }
}