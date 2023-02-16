using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace SwaggerDocCreator
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(x =>
                    {
                        if (x.Type.ToLower() == "pdf")
                        {
                            new SwaggerProcessor<PdfSwaggerDocRender>().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        if (x.Type.ToLower() == "md")
                        {
                            new SwaggerProcessor<MarkdownSwaggerDocRender>().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        if (x.Type.ToLower() == "docx")
                        {
                            new SwaggerProcessor<DocxSwaggerDocRender>().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        return 0;
                    },
                    x => -1);
        }
    }
}