using CommandLine;

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
    }
}