namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Initialize()
    /// </summary>
    public class InitializeResult : BaseResult
    {
        /// <summary>
        /// This parameter is only returned if the parameter is successful, and returns a 32bit, hexadecimal value (Guid)
        /// identifying the orderRef.
        /// Example: 8e96e163291c45f7bc3ee998d3a89c39
        /// </summary>
        public string OrderRef { get; set; }

        /// <summary>
        /// This parameter is only returned if the parameter is successful, and returns a 32bit, hexadecimal value (Guid)
        /// identifying the session for this order. This id is later used as a secret key when making a hash in the callback
        /// functionality.
        /// Example: 8e96e163291c45f7bc3ee998d3a89c39
        /// Note:This field is only used if the merchant account is specifically configured to use the callback functionality.
        /// Otherwise there is no need to store the sessionRef.
        /// </summary>
        public string SessionRef { get; set; }

        /// <summary>
        /// Dynamic URL to send the end user to, when using redirect model. Note: This URL may change, do not store in any config
        /// file.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx Initialize failed: {base.GetErrorDescription()}";
        }
    }
}