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
                        new SwaggerProcessor().Process(x.Input,x.FontPath,x.FontFamily,x.Output);
                        return 0;
                    },
                    x => -1);
        }
    }
}