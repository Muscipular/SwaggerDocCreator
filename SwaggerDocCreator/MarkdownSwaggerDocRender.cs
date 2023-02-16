using System.IO;
using NSwag;

namespace SwaggerDocCreator;

class MarkdownSwaggerDocRender : ISwaggerDocRender
{
    private StreamWriter pdfDocument;
    public StreamWriter PdfDocument => pdfDocument;

    public void Dispose()
    {
        pdfDocument?.Close();
        pdfDocument?.Dispose();
    }

    public void Save()
    {
        pdfDocument.Flush();
    }

    public ISwaggerDocRender RenderDocInfo(OpenApiInfo info)
    {
        pdfDocument.WriteLine("# " + info.Title);
        pdfDocument.WriteLine("" + info.Version);
        pdfDocument.WriteLine();
        pdfDocument.WriteLine();
        pdfDocument.WriteLine();
        pdfDocument.WriteLine("" + info.Description);
        return this;
    }

    public ISwaggerDocRender RenderGroup(int groupSeq, string groupName)
    {
        pdfDocument.WriteLine($"## {groupSeq}. {groupName}");
        return this;
    }

    public ISwaggerDocRender RenderMethod(int groupSeq, int methodSeq, string path, string method,
        OpenApiOperation operation)
    {
        pdfDocument.WriteLine($"### {groupSeq}.{methodSeq}. {operation.Summary}");
        pdfDocument.WriteLine("Method: " + method);
        pdfDocument.WriteLine();
        pdfDocument.WriteLine("Url: " + path);
        pdfDocument.WriteLine();
        return this;
    }

    public ISwaggerTableRender StartTable()
    {
        return new MarkdownTableRender(this);
    }

    public ISwaggerDocRender RenderParameterGroup(string groupName)
    {
        pdfDocument.WriteLine(groupName);
        pdfDocument.WriteLine();
        return this;
    }

    public ISwaggerDocRender RenderSubTypeName(string name)
    {
        pdfDocument.WriteLine(name);
        pdfDocument.WriteLine();
        return this;
    }

    public ISwaggerDocRender LineBreak()
    {
        pdfDocument.WriteLine();
        pdfDocument.WriteLine();
        return this;
    }

    public ISwaggerDocRender Init(Stream path, string font, string fontFamily)
    {
        this.pdfDocument = new StreamWriter(path);
        return this;
    }

    public ISwaggerDocRender Init(string path, string font, string fontFamily)
    {
        return Init(File.Open(path, FileMode.CreateNew), font, fontFamily);
    }
}