namespace SwaggerDocCreator;

interface ISwaggerTableRender
{
    ISwaggerTableRender AddHeader(string header);

    ISwaggerTableRender AddCell(string text);

    ISwaggerTableRender EndRow();

    ISwaggerDocRender Complete();
    ISwaggerTableRender EndHeader();
}