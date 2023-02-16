using System.IO;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using NSwag;

namespace SwaggerDocCreator;

class PdfSwaggerDocRender : ISwaggerDocRender
{
    private Document document;

    public Document Document
    {
        get => document;
        private set => document = value;
    }

    public PdfDocument PdfDocument { get; private set; }

    public ISwaggerDocRender Init(Stream path, string font, string fontFamily)
    {
        this.PdfDocument = new PdfDocument(new PdfWriter(path));
        Document = new Document(PdfDocument);
        PdfFontFactory.Register(font); //"simsun.ttc"
        //var array = PdfFontFactory.GetRegisteredFonts().Where(x => x.Contains("sum")).ToArray();
        var msyh = PdfFontFactory.CreateRegisteredFont(fontFamily, PdfEncodings.IDENTITY_H); //新宋体
        Document.SetFont(msyh);
        Document.SetFontSize(14);
        return this;
    }

    public ISwaggerDocRender Init(string path, string font, string fontFamily)
    {
        return this.Init(File.Open(path, FileMode.CreateNew), font, fontFamily);
    }
    
    public void Save()
    {
        Document.Flush();
    }

    public ISwaggerDocRender RenderDocInfo(OpenApiInfo info)
    {
        //                document.SetFont(PdfFontFactory.CreateRegisteredFont("helvetica"));
        Document.Add(new Paragraph(new Text(info.Title).SetBold().SetFontSize(36)
            .SetTextAlignment(TextAlignment.CENTER)));
        Document.Add(new Paragraph(new Text("Version: " + info.Version).SetBold()));
        Document.Add(new Paragraph(new Text(info.Description ?? "")));
        return this;
    }

    public ISwaggerDocRender RenderGroup(int groupSeq, string groupName)
    {
        document.Add(new Div().SetHeight(16));
        document.Add(new Paragraph($"{groupSeq}. {groupName}").NoMarginPadding().SetMarginTop(10).SetFontSize(18)
            .SetBold());
        return this;
    }

    public ISwaggerDocRender RenderMethod(int groupSeq, int methodSeq, string path, string method,
        OpenApiOperation operation)
    {
        document.Add(new Paragraph(new Text($"{groupSeq}.{methodSeq}. {operation.Summary}"))
            .NoMarginPadding().SetBold());
        document.Add(new Paragraph(new Text("Method: " + method)).NoMarginPadding()
            .SetFontSize(12));
        document.Add(new Paragraph(new Text("Url: " + path)).NoMarginPadding().SetFontSize(12));
        return this;
    }

    public ISwaggerTableRender StartTable()
    {
        var tableRender = new PdfTableRender(this);
        return tableRender;
    }

    public ISwaggerDocRender RenderParameterGroup(string groupName)
    {
        document.Add(new Paragraph(groupName).NoMarginPadding().SetMarginTop(10));
        return this;
    }

    public ISwaggerDocRender RenderSubTypeName(string name)
    {
        document.Add(new Paragraph(name));
        return this;
    }

    public ISwaggerDocRender LineBreak()
    {
        document.Add(new Paragraph());
        return this;
    }

    public void Dispose()
    {
        PdfDocument.Close();
    }
}