using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using NJsonSchema;
using NSwag;

namespace SwaggerDocCreator
{
    class SwaggerProcessor
    {
        float[] columnWidths = new[] {22f, 20, 12, 46};

        public void Process(string input, string fontPath, string fontFamily, string output)
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
            using (var pdfDocument =
                   new iText.Kernel.Pdf.PdfDocument(new PdfWriter(File.Open(output, FileMode.Create))))
            {
                var document = new Document(pdfDocument);
                PdfFontFactory.Register(fontPath); //"simsun.ttc"
                //var array = PdfFontFactory.GetRegisteredFonts().Where(x => x.Contains("sum")).ToArray();
                var msyh = PdfFontFactory.CreateRegisteredFont(fontFamily, PdfEncodings.IDENTITY_H); //新宋体
                document.SetFont(msyh);
                document.SetFontSize(14);
                //                document.SetFont(PdfFontFactory.CreateRegisteredFont("helvetica"));
                document.Add(new Paragraph(new Text(swaggerDocument.Info.Title).SetBold().SetFontSize(36)
                    .SetTextAlignment(TextAlignment.CENTER)));
                document.Add(new Paragraph(new Text("Version: " + swaggerDocument.Info.Version).SetBold()));
                document.Add(new Paragraph(new Text(swaggerDocument.Info.Description ?? "")));

                int indexTag = 0, indexOper = 0;
                foreach (var (tag, pairs) in swaggerDocument.Paths.GroupBy(x =>
                             x.Value.Values.FirstOrDefault().Tags.FirstOrDefault()))
                {
                    if (tag != "CustomsExtSys")
                    {
                        continue;
                    }

                    indexTag++;
                    indexOper = 0;
                    document.Add(new Div().SetHeight(16));
                    document.Add(new Paragraph($"{indexTag}. {tag}").NoMarginPadding().SetMarginTop(10).SetFontSize(18)
                        .SetBold());
                    foreach (var (path, operations) in pairs)
                    {
                        foreach (var (method, operation) in operations)
                        {
                            indexOper++;
                            document.Add(new Paragraph(new Text($"{indexTag}.{indexOper}. {operation.Summary}"))
                                .NoMarginPadding().SetBold());
                            document.Add(new Paragraph(new Text("Method: " + method)).NoMarginPadding()
                                .SetFontSize(12));
                            document.Add(new Paragraph(new Text("Url: " + path)).NoMarginPadding().SetFontSize(12));

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

                            document.Add(new AreaBreak());
                        }
                    }
                }
            }
        }

        private void RenderResponse(Document document, OpenApiResponse response)
        {
            var childs = new Dictionary<string, JsonSchema>();
            document.Add(new Paragraph("返回值").NoMarginPadding().SetMarginTop(10));
            var table = new Table(columnWidths, true).SetFontSize(12);
            document.Add(table);
            table.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明");
            foreach (var (field, prop) in response.ActualResponse.Schema.ActualProperties)
            {
                FillTable(field, prop, table, childs);
            }

            table.Complete();
            RenderChildren(document, childs);
        }

        private void RenderParmeter(Document document, IEnumerable<OpenApiParameter> parameters)
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
                        document.Add(new Paragraph($"{ps.Key:G}参数:").NoMarginPadding().SetMarginTop(10));
                        break;
                    case OpenApiParameterKind.Header:
                        document.Add(new Paragraph("Header:").NoMarginPadding().SetMarginTop(10));
                        break;
                }

                var table = new Table(columnWidths, true).SetFontSize(12);
                document.Add(table);
                table.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明");
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

        private void RenderChildren(Document document, Dictionary<string, JsonSchema> childs)
        {
            while (childs.Any())
            {
                var key = childs.Keys.FirstOrDefault();
                var jsonSchema4 = childs[key];
                childs.Remove(key);
                document.Add(new Paragraph(key));
                var table2 = new Table(columnWidths, true).SetFontSize(12);
                document.Add(table2);
                table2.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明");
                foreach (var (field, property) in jsonSchema4.ActualProperties)
                {
                    FillTable(field, property, table2, childs);
                }

                table2.Complete();
            }
        }

        private void FillTable(string field, JsonSchema property, Table table, Dictionary<string, JsonSchema> childs)
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
            table.Flush();
        }
    }
}