using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace SwaggerDocCreator
{
    class Options
    {
        [Option('i', "input", Required = true)]
        public string Input { get; set; }
    }
}