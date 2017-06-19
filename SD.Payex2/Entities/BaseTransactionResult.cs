namespace SD.Payex2.Entities
{
    /// <summary>
    /// Base class for transaction results, containing TransactionStatus, TransactionNumber and in applicable cases
    /// OriginalTransactionNumber.
    /// </summary>
    public class BaseTransactionResult : BaseResult
    {
        /// <summary>
        /// 0=Sale, 1=Initialize, 2=Credit, 3=Authorize, 4=Cancel, 5=Failure, 6=Capture
        /// (This field needs to be validated by the merchant to verify wether the transaction was successful or not).
        /// </summary>
        public Enumerations.TransactionStatusCode? TransactionStatus { get; set; }

        /// <summary>
        /// Returns the transaction number This is useful for support reference as this is the number
        /// available in the merchant admin view and also the transaction number presented to the end user.
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Returns the transaction number of any original (Authorize/Sale) transaction.
        /// </summary>
        public string OriginalTransactionNumber { get; set; }

        /// <summary>
        /// Gets a value indicating if both the result and the transaction was successful.
        /// </summary>
        public virtual bool IsTransactionSuccessful { get; protected set; }
    }
}