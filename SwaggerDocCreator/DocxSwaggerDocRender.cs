using System;
using System.IO;
using Aspose.Words;
using Aspose.Words.Fonts;
using NSwag;

namespace SwaggerDocCreator;

class DocxSwaggerDocRender : ISwaggerDocRender
{
    static DocxSwaggerDocRender()
    {
        Nd.Update();
    }
    
    private Aspose.Words.Document doc;
    private DocumentBuilder document;

    private Stream _path;

    public DocumentBuilder Document => document;

    public void Dispose()
    {
        _path?.Dispose();
    }

    public void Save()
    {
        doc.Save(_path, SaveFormat.Docx);
    }

    public ISwaggerDocRender RenderDocInfo(OpenApiInfo info)
    {
        // document.InsertParagraph();
        using (PushFont())
        {
            document.Font.Size = 20;
            document.Font.Bold = true;
            document.Writeln(info.Title);
        }

        document.Writeln(info.Version);
        document.Writeln();
        document.Writeln(info.Description);
        document.Writeln();
        document.Writeln();
        return this;
    }

    public ISwaggerDocRender RenderGroup(int groupSeq, string groupName)
    {
        // document.InsertParagraph();
        using (PushFont())
        {
            document.Font.Size = 18;
            document.Writeln($"{groupSeq}. {groupName}");
        }

        return this;
    }

    public ISwaggerDocRender RenderMethod(int groupSeq, int methodSeq, string path, string method,
        OpenApiOperation operation)
    {
        // document.InsertParagraph();
        using (PushFont())
        {
            document.Font.Size = 18;
            document.Writeln($"{groupSeq}.{methodSeq}. {operation.Summary}");
        }

        document.Writeln($"Method: {method}");
        document.Writeln($"Url: {path}");

        return this;
    }

    public ISwaggerTableRender StartTable()
    {
        return new DocxTableRender(this);
    }

    public ISwaggerDocRender RenderParameterGroup(string groupName)
    {
        // var paragraph = document.InsertParagraph();
        using (PushFont())
        {
            document.Writeln(groupName);
        }

        return this;
    }

    public ISwaggerDocRender RenderSubTypeName(string name)
    {
        // document.InsertParagraph();
        using (PushFont())
        {
            document.Writeln(name);
        }

        return this;
    }

    public ISwaggerDocRender LineBreak()
    {
        document.InsertBreak(BreakType.LineBreak);
        return this;
    }

    public ISwaggerDocRender Init(Stream path, string font, string fontFamily)
    {
        this._path = path;
        doc = new Document();
        document = new DocumentBuilder(doc);
        // doc.FontSettings.SetFontsSources(new FontSourceBase[] {new FileFontSource(font)});
        doc.Styles.DefaultFont.Name = fontFamily;
        return this;
    }

    public ISwaggerDocRender Init(string path, string font, string fontFamily)
    {
        return this.Init(File.Open(path, FileMode.CreateNew), font, fontFamily);
    }

    public IDisposable PushFont()
    {
        document.PushFont();
        return new DisposeCallback(() => document.PopFont());
    }
}