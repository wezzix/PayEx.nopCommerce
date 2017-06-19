namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to CreateAgreement
    /// </summary>
    public class CreateAgreementResult : BaseResult
    {
        /// <summary>
        /// Reference to the created agreement.
        /// </summary>
        public string AgreementRef { get; set; }

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx CreateAgreement failed: {base.GetErrorDescription()}";
        }
    }
}