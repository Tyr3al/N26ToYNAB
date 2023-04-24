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
using Serilog;

namespace N26ToYNAB
{
    internal static class Program
    {
        /// <summary>
        /// General CSV parser configuration
        /// </summary>
        private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            Quote = '"',
            HasHeaderRecord = true,
        };

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ConversionHandler)
                .WithNotParsed(ErrorHandler);
        }

        private static void ErrorHandler(IEnumerable<Error> errors)
        {
            Log.Logger.Error("Could not run conversion due to missing input");
        }

        /// <summary>
        /// Conversion workflow handler
        /// </summary>
        /// <param name="options">Parsed commandline options</param>
        private static void ConversionHandler(Options options)
        {
            Log.Logger.Information("Started Conversion");
            var n26Transactions = ParseData(options.InputCSV);
            var ynabTransaction = ConvertModels(n26Transactions);
            CreateYnabCsv(ynabTransaction, options.OutputCSV, options.InputCSV);
            Log.Logger.Information("Finished Conversion");
        }

        private static void CreateYnabCsv(IEnumerable<YNABTransaction> ynabTransaction, string outputCsvPath,
            string inputCsvPath)
        {
            if (string.IsNullOrWhiteSpace(outputCsvPath))
            {
                outputCsvPath = CreateOutputFilePath(inputCsvPath, Config.YNABSuffix);
                Log.Logger.Information("No output file specified, using: {FileName}", outputCsvPath);
            }

            using var writer = new StreamWriter(outputCsvPath);
            using var csv = new CsvWriter(writer, CsvConfig);
            csv.WriteRecords(ynabTransaction);
        }

        /// <summary>
        /// Creates <see cref="YNABTransaction"/> objects from <see cref="N26Transaction"/> objects
        /// </summary>
        /// <param name="n26Transactions">Parsed List of <see cref="N26Transaction"/>s</param>
        /// <returns>List of <see cref="YNABTransaction"/></returns>
        private static IEnumerable<YNABTransaction> ConvertModels(IEnumerable<N26Transaction> n26Transactions)
        {
            var ynabRecords = n26Transactions.Select(transaction => new YNABTransaction
            {
                Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture), Payee = transaction.Recipient,
                Date = transaction.Date.ToString("yyyy-MM-dd"), Memo = CreateYnabMemo(transaction)
            }).ToList();

            Log.Logger.Information("Converting to YNAB transactions");
            return ynabRecords;
        }

        /// <summary>
        /// Parses n26 csv to model
        /// </summary>
        /// <param name="inputCsv">Path to n26 csv file</param>
        /// <returns>List of <see cref="N26Transaction"/> objects</returns>
        private static IEnumerable<N26Transaction> ParseData(string inputCsv)
        {
            if (!File.Exists(inputCsv))
            {
                Console.Error.WriteLine("Could not find input file");
                Environment.Exit(-1);
            }

            List<N26Transaction> records = null;

            Log.Logger.Information("Trying to parse input file {File}", inputCsv);
            try
            {
                using var reader = new StreamReader(inputCsv);
                using var csv = new CsvReader(reader, CsvConfig);
                records = csv.GetRecords<N26Transaction>().ToList();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Could read / parse input file!");
                Environment.Exit(-1);
            }

            Log.Logger.Information("Found {RecordCount} N26 transactions", records.Count);
            return records;
        }

        /// <summary>
        /// Creates output file name by using input file name and appending suffix
        /// </summary>
        /// <param name="inputCsvPath">input file name</param>
        /// <param name="suffix">suffix for output file</param>
        /// <returns>Output filename "INPUT_SUFFIX.csv</returns>
        /// <exception cref="ArgumentException">Thrown if input file name is null or whitespace</exception>
        private static string CreateOutputFilePath(string inputCsvPath, string suffix)
        {
            if (string.IsNullOrWhiteSpace(inputCsvPath))
                throw new ArgumentException("Path cannot be null or whitespace!");

            var folderPath = Path.GetDirectoryName(inputCsvPath);
            var fileName = Path.GetFileNameWithoutExtension(inputCsvPath);
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

            memoBuilder.Append($"{transaction.TransactionType} - {transaction.Reference}");
            if (transaction.IsFxTransaction())
            {
                memoBuilder.Append(
                    $" | FX: {transaction.AmountForeignCurrency}{transaction.ForeignCurrency} @ {transaction.ExchangeRate}");
            }

            return memoBuilder.ToString();
        }
    }
}