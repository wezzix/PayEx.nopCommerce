namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to DeleteAgreement
    /// </summary>
    public class DeleteAgreementResult : BaseResult
    {
        /// <summary>
        /// Reference to the deleted agreement.
        /// </summary>
        public string AgreementRef { get; set; }

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx DeleteAgreement failed: {base.GetErrorDescription()}";
        }
    }
}