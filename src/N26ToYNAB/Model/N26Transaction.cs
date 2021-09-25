using System;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration.Attributes;

namespace N26ToYNAB.Model
{
    /// <summary>
    /// YNAB CSV model 
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class N26Transaction
    {
        [Index(0)] public DateTime Date { get; set; }
        [Index(1)] public string Recipient { get; set; }
        [Index(2)] public string AccountNumber { get; set; }
        [Index(3)] public string TransactionType { get; set; }
        [Index(4)] public string Reference { get; set; }
        [Index(5)] public string Category { get; set; }
        [Index(6)] public decimal Amount { get; set; }
        [Index(7)] public decimal? AmountForeignCurrency { get; set; }
        [Index(8)] public string ForeignCurrency { get; set; }
        [Index(9)] public decimal? ExchangeRate { get; set; }


        /// <summary>
        /// Check if transaction involved foreign currency
        /// </summary>
        /// <returns>True if foreign currency was used, else false</returns>
        public bool IsFxTransaction()
        {
            return !string.IsNullOrWhiteSpace(ForeignCurrency) &&
                   !ForeignCurrency.Equals("EUR", StringComparison.CurrentCultureIgnoreCase);
        }
        
    }
}