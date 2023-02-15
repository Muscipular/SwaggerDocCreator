using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NJsonSchema;
using NSwag;

namespace SwaggerDocCreator;

class SwaggerProcessorMd
{
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
        using (var pdfDocument = new StreamWriter(File.Open(output, FileMode.Create)))
        {
            pdfDocument.WriteLine("# " + swaggerDocument.Info.Title);
            pdfDocument.WriteLine("" + swaggerDocument.Info.Version);
            pdfDocument.WriteLine();
            pdfDocument.WriteLine("" + swaggerDocument.Info.Description);

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
                pdfDocument.WriteLine($"## {indexTag}. {tag}");
                foreach (var (path, operations) in pairs)
                {
                    foreach (var (method, operation) in operations)
                    {
                        indexOper++;
                        pdfDocument.WriteLine($"### {indexTag}.{indexOper}. {operation.Summary}");
                        pdfDocument.WriteLine("Method: " + method);
                        pdfDocument.WriteLine("Url: " + path);

                        var parameters = operation.ActualParameters ?? Array.Empty<OpenApiParameter>();
                        if (parameters.Any())
                        {
                            RenderParmeter(pdfDocument, parameters);
                        }

                        var responses = operation.ActualResponses.Where(x => x.Key == "200").Select(x => x.Value)
                            .FirstOrDefault();
                        if (responses != null)
                        {
                            RenderResponse(pdfDocument, responses);
                        }

                        pdfDocument.WriteLine();
                        pdfDocument.WriteLine();
                        pdfDocument.WriteLine();
                    }
                }
            }
        }
    }

    private void RenderResponse(TextWriter document, OpenApiResponse response)
    {
        if (response.Schema == null)
        {
            return;
        }
        var childs = new Dictionary<string, JsonSchema>();
        document.WriteLine("返回值");
        document.WriteLine("");
        document.WriteLine("|字段|类型|是否可空|说明|");
        document.WriteLine("|--|--|--|--|");
        foreach (var (field, prop) in response.ActualResponse.Schema.ActualProperties)
        {
            FillTable(field, prop, document, childs);
        }

        document.WriteLine();
        document.WriteLine();
        document.WriteLine();
        RenderChildren(document, childs);
    }

    private void RenderParmeter(TextWriter document, IEnumerable<OpenApiParameter> parameters)
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
                    document.WriteLine($"{ps.Key:G}参数:");
                    break;
                case OpenApiParameterKind.Header:
                    document.WriteLine("Header:");
                    break;
            }

            var table = document;
            document.WriteLine("");
            document.WriteLine("|字段|类型|是否可空|说明|");
            document.WriteLine("|--|--|--|--|");
            foreach (var parameter in ps)
            {
                if (parameter.Kind == OpenApiParameterKind.Body)
                {
                    var schema = parameter.ActualSchema;

                    foreach (var (field, property) in schema.ActualProperties)
                    {
                        FillTable(field, property, table, childs);
                    }
                }
                else
                {
                    FillTable(parameter.Name, parameter, table, childs);
                }
            }

            table.WriteLine();
            table.WriteLine();
            table.WriteLine();
        }

        RenderChildren(document, childs);
    }

    private void RenderChildren(TextWriter document, Dictionary<string, JsonSchema> childs)
    {
        while (childs.Any())
        {
            var key = childs.Keys.FirstOrDefault();
            var jsonSchema4 = childs[key];
            childs.Remove(key);
            document.WriteLine(key);
            var table2 = document;
            document.WriteLine();
            document.WriteLine("|字段|类型|是否可空|说明|");
            document.WriteLine("|--|--|--|--|");
            foreach (var (field, property) in jsonSchema4.ActualProperties)
            {
                FillTable(field, property, table2, childs);
            }

            table2.WriteLine();
            table2.WriteLine();
            table2.WriteLine();
        }
    }

    private void FillTable(string field, JsonSchema property, TextWriter table, Dictionary<string, JsonSchema> childs)
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

                if (property.Type == JsonObjectType.Number)
                {
                    typeName = "Double";
                    break;
                }

                //table.AddCell(property.Type.ToString());
                break;
        }

        table.AddCell(typeName);
        table.AddCell(isAllowNull ? "是" : "否");

        if (property.ExtensionData != null && property.ExtensionData.ContainsKey("enumDesc"))
        {
            table.AddCell(desc + "  " + property.ExtensionData["enumDesc"]);
        }
        else
        {
            table.AddCell(desc);
        }

        table.EndCell();
    }
}