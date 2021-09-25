using CsvHelper.Configuration.Attributes;

namespace N26ToYNAB.Model
{
    /// <summary>
    /// YNAB transaction model
    /// <remarks>See YNAB documentation: https://docs.youneedabudget.com/article/921-formatting-csv-file
    /// </remarks>
    /// </summary>
    public class YNABTransaction
    {
        [Index(0)]
        public string Date { get; set; }
        [Index(1)]
        public string Payee { get; set; }
        [Index(2)]
        public string Memo { get; set; }
        [Index(3)]
        public string Amount { get; set; }
    }
}