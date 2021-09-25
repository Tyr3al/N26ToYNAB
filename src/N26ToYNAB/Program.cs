using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using N26ToYNAB.Model;

namespace N26ToYNAB
{
    internal static class Program
    {
        /// <summary>
        /// General CSV parser configuration
        /// </summary>
        private static readonly CsvConfiguration _csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            Quote = '"',
            HasHeaderRecord = true,
        };

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ConversionHandler)
                .WithNotParsed(ErrorHandler);
        }

        private static void ErrorHandler(IEnumerable<Error> errors)
        {
            Console.Error.WriteLine("Could not run conversion due to missing input");
        }

        /// <summary>
        /// Conversion workflow handler
        /// </summary>
        /// <param name="options">Parsed commandline options</param>
        private static void ConversionHandler(Options options)
        {
            Console.WriteLine("Started Conversion");
            var n26Transactions = ParseData(options.InputCSV);
            var ynabTransaction = ConvertModels(n26Transactions);
            CreateYnabCSV(ynabTransaction, options.OutputCSV, options.InputCSV);
            Console.WriteLine("Finished Conversion");
        }

        private static void CreateYnabCSV(IEnumerable<YNABTransaction> ynabTransaction, string outputCsvPath,
            string inputCSVPath)
        {
            if (string.IsNullOrWhiteSpace(outputCsvPath))
            {
                outputCsvPath = CreateOutputFilePath(inputCSVPath, Config.YNABSuffix);
                Console.WriteLine($"No output file specified, using: {outputCsvPath}");
            }
            
            using var writer = new StreamWriter(outputCsvPath);
            using var csv = new CsvWriter(writer, _csvConfig);
            csv.WriteRecords(ynabTransaction);
        }

        /// <summary>
        /// Creates <see cref="YNABTransaction"/> objects from <see cref="N26Transaction"/> objects
        /// </summary>
        /// <param name="n26Transactions">Parsed List of <see cref="N26Transaction"/>s</param>
        /// <returns>List of <see cref="YNABTransaction"/></returns>
        private static IEnumerable<YNABTransaction> ConvertModels(IEnumerable<N26Transaction> n26Transactions)
        {
            var ynabRecords = new List<YNABTransaction>();

            foreach (var transaction in n26Transactions)
            {
                ynabRecords.Add(new YNABTransaction()
                {
                    Amount = transaction.Amount.ToString(),
                    Payee = transaction.Recipient,
                    Date = transaction.Date.ToString("yyyy-MM-dd"),
                    Memo = CreateYnabMemo(transaction)
                });
            }

            Console.WriteLine($"Converting to YNAB transactions");
            return ynabRecords;
        }

        /// <summary>
        /// Parses n26 csv to model
        /// </summary>
        /// <param name="inputCSV">Path to n26 csv file</param>
        /// <returns>List of <see cref="N26Transaction"/> objectss</returns>
        private static IEnumerable<N26Transaction> ParseData(string inputCSV)
        {
            if (!File.Exists(inputCSV))
            {
                Console.Error.WriteLine("Could not find input file");
                Environment.Exit(-1);
            }

            List<N26Transaction> records = null;
            
            Console.WriteLine($"Trying to parse input file {inputCSV}");
            try
            {
                using var reader = new StreamReader(inputCSV);
                using var csv = new CsvReader(reader, _csvConfig);
                records = csv.GetRecords<N26Transaction>().ToList();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Could read / parse input file!{Environment.NewLine}Exception: {e.Message}");
                Environment.Exit(-1);
            }

            Console.WriteLine($"Found {records.Count} N26 transactions");
            return records;
        }

        /// <summary>
        /// Creates output file name by using input file name and appending suffix
        /// </summary>
        /// <param name="inputCSVPath">input file name</param>
        /// <param name="suffix">suffix for output file</param>
        /// <returns>Output filename "INPUT_SUFFIX.csv</returns>
        /// <exception cref="ArgumentException">Thrown if input file name is null or whitespace</exception>
        private static string CreateOutputFilePath(string inputCSVPath, string suffix)
        {
            if (string.IsNullOrWhiteSpace(inputCSVPath))
                throw new ArgumentException("Path cannot be null or whitespace!");

            var folderPath = Path.GetDirectoryName(inputCSVPath);
            var fileName = Path.GetFileNameWithoutExtension(inputCSVPath);
            fileName += $"_{suffix}.csv";

            return Path.Combine(folderPath, fileName);
        }

        /// <summary>
        /// Constructs YNAB memo from N26 transactions details
        /// </summary>
        /// <param name="transaction">N26 Transaction</param>
        /// <returns>Memo string for transaction</returns>
        private static string CreateYnabMemo(N26Transaction transaction)
        {
            var memoBuilder = new StringBuilder();

            memoBuilder.Append($"{transaction.TransactionType} - {transaction.Category}");
            if (transaction.IsFxTransaction())
            {
                memoBuilder.Append(
                    $" | FX: {transaction.AmountForeignCurrency}{transaction.ForeignCurrency} @ {transaction.ExchangeRate}");
            }

            return memoBuilder.ToString();
        }
    }
}