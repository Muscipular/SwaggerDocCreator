using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace SwaggerDocCreator
{
    class Options
    {
        [Option('i', "input", Required = true, HelpText = "swagger json definition file")]
        public string Input { get; set; }

        [Option('F', "font-path", Required = true, HelpText = "Font file path, support truetype font")]
        public string FontPath { get; set; }

        [Option('f', "font-family", Required = true, HelpText = "Font family of Font")]
        public string FontFamily { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output pdf file")]
        public string Output { get; set; }

        [Option('t', "type", Required = false, HelpText = "Output format: pdf/md", Default = "pdf")]
        public string Type { get; set; }
        
        [Option('T', "tag", Required = false, HelpText = "Output tag")]
        public IEnumerable<string> Tags { get; set; }
    }
}