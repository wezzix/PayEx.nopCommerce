using System.Xml.Linq;

namespace SD.Payex2.Entities
{
    /// <summary>
    /// BaseResult is common for all result.
    /// </summary>
    public class BaseResult
    {
        /// <summary>
        /// Indicates the result of the request. Obsolete parameter, check errorCode.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A more informative error code which indicates the result of the request.
        /// Returns OK if request is successful. Note: This does NOT indicate wether the transaction requested
        /// was successful, only wether the Initialize request was carried out successfully.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// A literal description explaining the result. Returns OK if request is successful.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns the name of the parameter that contains invalid data.
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Returns the error code received from third party (if returned).
        /// </summary>
        public string ThirdPartyError { get; set; }

        /// <summary>
        /// Gets the raw xml used to create the object.
        /// </summary>
        public string RawXml { get; set; }

        /// <summary>
        /// Returns a value indicating if the request (not the transaction) was carried out successfully (errorCode = OK).
        /// Be sure to check the result of the transaction separately.
        /// </summary>
        public bool IsRequestSuccessful => ErrorCode == Enumerations.OK;

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        /// <returns></returns>
        public virtual string GetErrorDescription()
        {
            return $"{Description} [ {ErrorCode} {ParamName} {ThirdPartyError} ]";
        }

        public XElement GetRootElement()
        {
            var doc = XDocument.Parse(RawXml);
            var root = doc.Element("payex");
            return root;
        }
    }
}