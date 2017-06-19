namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Capture()
    /// </summary>
    public class CaptureResult : BaseTransactionResult
    {
        /// <summary>
        /// Gets a value indicating if both the result and the capture was successful.
        /// </summary>
        public override bool IsTransactionSuccessful => IsRequestSuccessful && TransactionStatus.HasValue &&
                                                        TransactionStatus == Enumerations.TransactionStatusCode.Capture;

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx Capture failed: {base.GetErrorDescription()}";
        }
    }
}