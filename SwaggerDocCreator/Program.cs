using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Font;
using iText.Layout.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Linq;
using iText.IO;
using iText.Kernel.Pdf.Canvas.Draw;

namespace SwaggerDocCreator
{
    class Options
    {
        [Option('i', "input", Required = true)]
        public string Input { get; set; }
    }

    public static class KeyValuePairExtension
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey Key, out TValue Value)
        {
            Key = source.Key;
            Value = source.Value;
        }

        public static void Deconstruct<TKey, TValue>(this IGrouping<TKey, TValue> source, out TKey Key, out IEnumerable<TValue> Value)
        {
            Key = source.Key;
            Value = source;
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(x =>
                    {
                        new SwaggerProcessor().Process(x.Input);
                        return 0;
                    },
                    x => -1);
        }
    }


    class SwaggerProcessor
    {
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

                foreach (var (tag, pairs) in swaggerDocument.Paths.GroupBy(x => x.Value.Values.FirstOrDefault().Tags.FirstOrDefault()))
                {
                    document.Add(new Div().SetHeight(16));
                    document.Add(new Paragraph(tag).SetPaddingBottom(0).SetMarginBottom(0).SetFontSize(18).SetBold());
                    foreach (var (path, operations) in pairs)
                    {
                        foreach (var (key, operation) in operations)
                        {
                            document.Add(new Paragraph(new Text(operation.Summary)).SetFontSize(16).SetMargin(1));
                            document.Add(new Paragraph(new Text(key.ToString())).Add(" ").Add(new Text(path)).SetMargin(1));
                            var table = new Table(new[]
                            {
                                UnitValue.CreatePointValue(50),
                                UnitValue.CreatePercentValue(50),
                                UnitValue.CreatePercentValue(0),
                            }).SetWidth(UnitValue.CreatePercentValue(100));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("1")));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("1")));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("1")));
                            document.Add(table);
                            var div = new Div().SetHeight(10);
                            document.Add(div);
                        }
                    }
                }
            }
        }
    }
}