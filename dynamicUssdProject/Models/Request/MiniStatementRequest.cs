namespace dynamicUssdProject.Models.Request
{
    public class MiniStatementRequest
    {
        public string PhoneNumber { get; set; }
        public string Pin { get; set; }
        /// <summary>
        /// Optional: Gets or sets the number of transactions to be included in the mini statement.
        /// </summary>
        public int? TransactionCount { get; set; }
    }

}
