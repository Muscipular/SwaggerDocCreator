using Aspose.Words;
using Aspose.Words.Tables;

namespace SwaggerDocCreator;

class DocxTableRender : ISwaggerTableRender
{
    private readonly DocxSwaggerDocRender _render;
    private readonly DocumentBuilder _document;
    private Table table;

    public DocxTableRender(DocxSwaggerDocRender render)
    {
        _render = render;
        _document = render.Document;
        table = _document.StartTable();
    }

    public ISwaggerTableRender AddHeader(string header)
    {
        _document.InsertCell();
        using (_render.PushFont())
        {
            _document.Font.Bold = true;
            _document.Write(header);
        }

        return this;
    }

    public ISwaggerTableRender AddCell(string text)
    {
        _document.InsertCell();
        _document.Write(text);
        return this;
    }

    public ISwaggerTableRender EndRow()
    {
        _document.EndRow();
        return this;
    }

    public ISwaggerDocRender Complete()
    {
        table.AllowAutoFit = false;
        table.PreferredWidth = PreferredWidth.FromPercent(100);
        _document.EndTable();
        return _render;
    }

    public ISwaggerTableRender EndHeader()
    {
        _document.EndRow();
        return this;
    }
}