using System;
using System.Drawing;
using System.IO;
using NSwag;

namespace SwaggerDocCreator;

interface ISwaggerDocRender : IDisposable
{
    void Save();

    ISwaggerDocRender RenderDocInfo(OpenApiInfo info);

    ISwaggerDocRender RenderGroup(int groupSeq, string groupName);

    ISwaggerDocRender RenderMethod(int groupSeq, int methodSeq, string path, string method,
        OpenApiOperation operation);

    ISwaggerTableRender StartTable();
    ISwaggerDocRender RenderParameterGroup(string groupName);
    ISwaggerDocRender RenderSubTypeName(string name);
    ISwaggerDocRender LineBreak();
    ISwaggerDocRender Init(Stream path, string font, string fontFamily);
    ISwaggerDocRender Init(string path, string font, string fontFamily);
}