namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to AutoPay
    /// </summary>
    public class AutoPayResult : BaseTransactionResult
    {
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
                                                         == Enumerations.TransactionStatusCode.Authorize);

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx AutoPay failed: {base.GetErrorDescription()}";
        }
    }
}