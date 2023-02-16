using iText.Layout.Element;

namespace SwaggerDocCreator;

class PdfTableRender : ISwaggerTableRender
{
    private static readonly float[] ColumnWidths = new[] {22f, 20, 12, 46};
    private Table _table;

    public PdfTableRender(PdfSwaggerDocRender pdfSwaggerDocRender)
    {
        PdfSwaggerDocRender = pdfSwaggerDocRender;
        _table = new Table(ColumnWidths, true).SetFontSize(12);
        pdfSwaggerDocRender.Document.Add(_table);
    }

    public PdfSwaggerDocRender PdfSwaggerDocRender { get; }

    public ISwaggerTableRender AddHeader(string header)
    {
        _table.AddHeaderCell(header);
        return this;
    }

    public ISwaggerTableRender AddCell(string text)
    {
        _table.AddCell(text);
        return this;
    }

    public ISwaggerTableRender EndRow()
    {
        _table.StartNewRow();
        return this;
    }

    public ISwaggerDocRender Complete()
    {
        _table.Complete();
        return PdfSwaggerDocRender;
    }

    public ISwaggerTableRender EndHeader()
    {
        return this;
    }
}