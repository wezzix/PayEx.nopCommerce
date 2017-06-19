namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Cancel()
    /// </summary>
    public class CancelResult : BaseTransactionResult
    {
        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx Cancel failed: {base.GetErrorDescription()}";
        }
    }
}