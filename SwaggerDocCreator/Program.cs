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
                            new SwaggerProcessor().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        if (x.Type.ToLower() == "md")
                        {
                            new SwaggerProcessorMd().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        if (x.Type.ToLower() == "docx")
                        {
                            new SwaggerProcessorDocx().Process(x.Input, x.FontPath, x.FontFamily, x.Output, x.Tags.ToArray());
                        }

                        return 0;
                    },
                    x => -1);
        }
    }
}