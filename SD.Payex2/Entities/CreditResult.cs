namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Credit()
    /// </summary>
    public class CreditResult : BaseTransactionResult
    {
        /// <summary>
        /// Gets a value indicating if both the result and the credit was successful.
        /// </summary>
        public override bool IsTransactionSuccessful => IsRequestSuccessful && TransactionStatus.HasValue &&
                                                        TransactionStatus == Enumerations.TransactionStatusCode.Credit;

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx Credit failed: {base.GetErrorDescription()}";
        }
    }
}