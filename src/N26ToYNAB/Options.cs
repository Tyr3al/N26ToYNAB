using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace N26ToYNAB
{
    /// <summary>
    /// Available command line options
    /// </summary>
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Path to N26 CSV file")]
        public string InputCSV { get; set; }
        
        [Option('o', "output", Required = false, HelpText = "Path to generated YNAB csv (optional)")]
        public string OutputCSV { get; set; }
        
        [Usage(ApplicationAlias = "N26ToYNAB ")]
        public static IEnumerable<Example> Example
        {
            get
            {
                return new[]
                {
                    new Example("Generate YNAB CSV next to input CSV", new Options()
                    {
                        InputCSV = @"C:\Users\JohnDoe\n26.csv"
                    }),
                    new Example("Generate YNAB CSV at target path", new Options()
                    {
                        InputCSV = @"C:\Users\JohnDoe\n26.csv",
                        OutputCSV = @"C:\Users\JohnDoe\ynab.csv",
                    })
                };
            }
        }
    }
}