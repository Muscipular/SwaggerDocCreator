using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf.Canvas;
using NJsonSchema;
using NSwag;

namespace SwaggerDocCreator;

class SwaggerProcessor<TRender> where TRender : ISwaggerDocRender, new()
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
        using ISwaggerDocRender document = new TRender();
        document.Init(File.Open(output, FileMode.Create), fontPath, fontFamily);

        document.RenderDocInfo(swaggerDocument.Info);

        int indexTag = 0, indexOper = 0;
        foreach (var (tag, pairs) in swaggerDocument.Paths.GroupBy(x =>
                     x.Value.Values.FirstOrDefault()?.Tags.FirstOrDefault()))
        {
            if (argTags.Length > 0 && argTags.All(e => e != tag))
            {
                continue;
            }

            indexTag++;
            indexOper = 0;
            document.RenderGroup(indexTag, tag);
            foreach (var (path, operations) in pairs)
            {
                foreach (var (method, operation) in operations)
                {
                    indexOper++;
                    document.RenderMethod(indexTag, indexOper, path, method, operation);
                    var parameters = operation.ActualParameters ?? Array.Empty<OpenApiParameter>();
                    if (parameters.Any())
                    {
                        RenderParmeter(document, parameters);
                    }

                    var responses = operation.ActualResponses
                        .Where(x => x.Key == "200")
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    if (responses != null)
                    {
                        RenderResponse(document, responses);
                    }

                    document.LineBreak();
                }
            }
        }

        document.Save();
    }

    private void RenderResponse(ISwaggerDocRender document, OpenApiResponse response)
    {
        var actualSchema = response.ActualResponse?.Schema?.ActualSchema;
        if (actualSchema == null || actualSchema?.ActualProperties.Count == 0)
        {
            return;
        }

        var childs = new Dictionary<string, JsonSchema>();
        document.RenderParameterGroup("返回值");
        var table = document.StartTable();
        table.AddHeader("字段").AddHeader("类型").AddHeader("是否可空").AddHeader("说明").EndHeader();
        foreach (var (field, prop) in actualSchema.ActualProperties)
        {
            FillTable(field, prop, table, childs);
        }

        table.Complete();
        RenderChildren(document, childs);
    }

    private void RenderParmeter(ISwaggerDocRender document, IEnumerable<OpenApiParameter> parameters)
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
                    document.RenderParameterGroup($"{ps.Key:G}参数:");
                    break;
                case OpenApiParameterKind.Header:
                    document.RenderParameterGroup("Header:");
                    break;
            }

            var table = document.StartTable();
            table.AddHeader("字段").AddHeader("类型").AddHeader("是否可空").AddHeader("说明").EndHeader();
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

            table.Complete();
        }

        RenderChildren(document, childs);
    }

    private void RenderChildren(ISwaggerDocRender document, Dictionary<string, JsonSchema> childs)
    {
        while (childs.Any())
        {
            var key = childs.Keys.FirstOrDefault();
            var jsonSchema4 = childs[key];
            childs.Remove(key);
            if (jsonSchema4.ActualProperties.Count == 0)
            {
                continue;
            }

            document.RenderSubTypeName(key);
            var table2 = document.StartTable();
            table2.AddHeader("字段").AddHeader("类型").AddHeader("是否可空").AddHeader("说明").EndHeader();
            foreach (var (field, property) in jsonSchema4.ActualProperties)
            {
                FillTable(field, property, table2, childs);
            }

            table2.Complete();
        }
    }

    private string ResolveTypeName(JsonSchema property)
    {
        var schema = property.ActualTypeSchema;
        switch (schema.Type)
        {
            case JsonObjectType.Array:
                return ResolveTypeName(schema.Item) + "[]";
            case JsonObjectType.Boolean:
                return "Bool";
            case JsonObjectType.Integer:
                return "Int";
            case JsonObjectType.Number:
                return "Double";
            case JsonObjectType.Object:
                if (schema.ExtensionData?.TryGetValue("typeInfo", out var typeName) == true)
                {
                    return typeName?.ToString();
                }

                return "Object";
            case JsonObjectType.String:
                return property.Format switch
                {
                    "date-time" => "DateTime",
                    "date" => "Date",
                    _ => "String"
                };
            case JsonObjectType.File:
            case JsonObjectType.Null:
            case JsonObjectType.None:
            default:
                return "Unsupported";
        }
    }

    private void FillTable(string field, JsonSchema property, ISwaggerTableRender table,
        Dictionary<string, JsonSchema> childs)
    {
        var isAllowNull = !((dynamic) property).IsRequired;
        var desc = property.Description ?? "";
        property = property.ActualTypeSchema;
        table.AddCell(field);
        string typeName = ResolveTypeName(property);
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