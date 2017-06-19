namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Complete()
    /// </summary>
    public class TransactionDetailsResult : BaseTransactionResult
    {
        /// <summary>
        /// Returns the amount credited the merchant on Sale, negative on Credit.
        /// </summary>
        public decimal? Amount { get; set; }

        public string CurrencyCode { get; set; }

        public int? AccountNumber { get; set; }

        /// <summary>
        /// This returns the orderID supplied by the merchant when the order was created,
        /// enabling the merchant to link the return data from PayEx with their local orderID.
        /// </summary>
        public string OrderId { get; set; }

        public string OrderDescription { get; set; }

        /// <summary>
        /// Returns the payment method used to pay for this transaction (Payex, VISA, MC, etc).
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets a value indicating if both the result and the complete transaction was successful.
        /// </summary>
        public override bool IsTransactionSuccessful => IsRequestSuccessful && TransactionStatus.HasValue &&
                                                        (TransactionStatus == Enumerations.TransactionStatusCode.Sale ||
                                                         TransactionStatus
                                                         == Enumerations.TransactionStatusCode.Authorize ||
                                                         TransactionStatus == Enumerations.TransactionStatusCode.Capture
                                                         ||
                                                         TransactionStatus == Enumerations.TransactionStatusCode.Credit)
        ;

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx GetTransactionDetails2 failed: {base.GetErrorDescription()}";
        }
    }
}