using System.Collections.Generic;
using System.Linq;

namespace SwaggerDocCreator;

class MarkdownTableRender : ISwaggerTableRender
{
    private MarkdownSwaggerDocRender _docRender;
    private List<string> headers = new List<string>();

    public MarkdownTableRender(MarkdownSwaggerDocRender docRender)
    {
        _docRender = docRender;
    }

    public ISwaggerTableRender AddHeader(string header)
    {
        headers.Add(header);
        return this;
    }

    public ISwaggerTableRender AddCell(string text)
    {
        this._docRender.PdfDocument.Write("| " + (string.IsNullOrWhiteSpace(text) ? "-" : text) + " ");
        return this;
    }

    public ISwaggerTableRender EndRow()
    {
        this._docRender.PdfDocument.WriteLine("| ");
        return this;
    }

    public ISwaggerDocRender Complete()
    {
        this._docRender.PdfDocument.WriteLine();
        return _docRender;
    }

    public ISwaggerTableRender EndHeader()
    {
        this._docRender.PdfDocument.WriteLine("| " + string.Join(" | ", headers) + " |");
        this._docRender.PdfDocument.WriteLine("| " + string.Join(" | ", headers.Select(e => "---")) + " |");
        return this;
    }
}