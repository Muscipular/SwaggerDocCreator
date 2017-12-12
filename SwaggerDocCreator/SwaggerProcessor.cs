﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    static class ElementExtensions
    {
        public static T SetPadding<T>(this T e, float? top, float? right, float? bottom, float? left) where T : BlockElement<T>
        {
            if (left.HasValue) e.SetPaddingLeft(left.Value);
            if (right.HasValue) e.SetPaddingRight(right.Value);
            if (top.HasValue) e.SetPaddingLeft(top.Value);
            if (bottom.HasValue) e.SetPaddingBottom(bottom.Value);
            return e;
        }

        public static T SetPadding<T>(this T e, float top, float right, float bottom, float left) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, bottom, left);
        }

        public static T SetPadding<T>(this T e, float top, float right, float bottom) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, bottom, right);
        }

        public static T SetPadding<T>(this T e, float top, float right) where T : BlockElement<T>
        {
            return e.SetPadding((float?)top, right, top, right);
        }

        public static T SetMargin<T>(this T e, float? top, float? right, float? bottom, float? left) where T : BlockElement<T>
        {
            if (left.HasValue) e.SetMarginLeft(left.Value);
            if (right.HasValue) e.SetMarginRight(right.Value);
            if (top.HasValue) e.SetMarginLeft(top.Value);
            if (bottom.HasValue) e.SetMarginBottom(bottom.Value);
            return e;
        }

        public static T SetMargin<T>(this T e, float top, float right, float bottom, float left) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, bottom, left);
        }

        public static T SetMargin<T>(this T e, float top, float right, float bottom) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, bottom, right);
        }

        public static T SetMargin<T>(this T e, float top, float right) where T : BlockElement<T>
        {
            return e.SetMargin((float?)top, right, top, right);
        }

        public static T NoMarginPadding<T>(this T e) where T : BlockElement<T>
        {
            return e.SetPadding(0).SetMargin(0);
        }
    }

    class SwaggerProcessor
    {
        float[] columnWidths = new[] { 22f, 20, 12, 46 };

        public void Process(string input)
        {
            var swaggerDocument = NSwag.SwaggerDocument.FromFileAsync(input).Result;
            var filename = Path.Combine(Path.GetPathRoot(input), Path.GetFileNameWithoutExtension(input) + ".pdf");
            using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(new PdfWriter(File.Open(filename, FileMode.Create))))
            {
                var document = new Document(pdfDocument);
                PdfFontFactory.Register("simsun.ttc");
                //var array = PdfFontFactory.GetRegisteredFonts().Where(x => x.Contains("sum")).ToArray();
                var msyh = PdfFontFactory.CreateRegisteredFont("新宋体", PdfEncodings.IDENTITY_H);
                document.SetFont(msyh);
                document.SetFontSize(14);
                //                document.SetFont(PdfFontFactory.CreateRegisteredFont("helvetica"));
                document.Add(new Paragraph(new Text(swaggerDocument.Info.Title).SetBold().SetFontSize(36).SetTextAlignment(TextAlignment.CENTER)));
                document.Add(new Paragraph(new Text("Version: " + swaggerDocument.Info.Version).SetBold()));
                document.Add(new Paragraph(new Text(swaggerDocument.Info.Description)));

                int indexTag = 0, indexOper = 0;
                foreach (var (tag, pairs) in swaggerDocument.Paths.GroupBy(x => x.Value.Values.FirstOrDefault().Tags.FirstOrDefault()))
                {
                    indexTag++;
                    indexOper = 0;
                    document.Add(new Div().SetHeight(16));
                    document.Add(new Paragraph($"{indexTag}. {tag}").NoMarginPadding().SetMarginTop(10).SetFontSize(18).SetBold());
                    foreach (var (path, operations) in pairs)
                    {
                        foreach (var (method, operation) in operations)
                        {
                            indexOper++;
                            document.Add(new Paragraph(new Text($"{indexTag}.{indexOper}. {operation.Summary}")).NoMarginPadding().SetBold());
                            document.Add(new Paragraph(new Text("Method: " + method)).NoMarginPadding().SetFontSize(12));
                            document.Add(new Paragraph(new Text("Url: " + path)).NoMarginPadding().SetFontSize(12));

                            var parameters = operation.ActualParameters ?? Array.Empty<SwaggerParameter>();
                            if (parameters.Any())
                            {
                                RenderParmeter(document, parameters);
                            }
                            var responses = operation.ActualResponses.Where(x => x.Key == "200").Select(x => x.Value).FirstOrDefault();
                            if (responses != null)
                            {
                                RenderResponse(document, responses);
                            }
                            document.Add(new AreaBreak());
                        }
                    }
                }
            }

            void FillTable(string field, JsonSchema4 property, Table table, Dictionary<string, JsonSchema4> childs)
            {
                var isRequired = !((dynamic)property).IsRequired;
                var desc = property.Description ?? "";
                property = property.ActualTypeSchema;
                table.AddCell(field);
                string typeName = property.Type.ToString();
                if (property.ExtensionData.TryGetValue("typeInfo", out var typeNameO))
                {
                    typeName = typeNameO?.ToString() ?? property.Type.ToString();
                }
                switch (property.Type)
                {
                    case JsonObjectType.Array:
                        var tname = Regex.Replace(typeName, @"^List\[(.+)\]$", "$1");
                        table.AddCell(tname + "[]");
                        childs.Add(tname, property.Item.ActualTypeSchema);
                        break;
                    case JsonObjectType.Object:
                        table.AddCell(typeName);
                        childs.Add(typeName, property.ActualTypeSchema);
                        break;
                    case JsonObjectType.None:
                        throw new InvalidOperationException();
                        break;
                    default:
                        table.AddCell(property.Type.ToString());
                        break;
                }
                table.AddCell(isRequired ? "是" : "否");
                table.AddCell(desc);
                table.Flush();
            }


            void RenderResponse(Document document, SwaggerResponse response)
            {
                var childs = new Dictionary<string, JsonSchema4>();
                document.Add(new Paragraph("返回值").NoMarginPadding().SetMarginTop(10));
                var table = new Table(columnWidths, true).SetFontSize(12);
                document.Add(table);
                table.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明");
                foreach (var (field, prop) in response.ActualResponseSchema.ActualProperties)
                {
                    FillTable(field, prop, table, childs);
                }
                table.Complete();
                RenderChildren(document, childs);
            }

            void RenderParmeter(Document document, IEnumerable<SwaggerParameter> parameters)
            {
                document.Add(new Paragraph("参数:").NoMarginPadding().SetMarginTop(10));
                var table = new Table(columnWidths, true).SetFontSize(12);
                document.Add(table);
                table.AddHeaderCell("字段").AddHeaderCell("类型").AddHeaderCell("是否可空").AddHeaderCell("说明");
                var childs = new Dictionary<string, JsonSchema4>();
                foreach (var parameter in parameters)
                {
                    if (parameter.Kind == SwaggerParameterKind.Body)
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
                RenderChildren(document, childs);
            }

            void RenderChildren(Document document, Dictionary<string, JsonSchema4> childs)
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
        }
    }
}