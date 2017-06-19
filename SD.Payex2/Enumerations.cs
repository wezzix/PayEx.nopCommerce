namespace SD.Payex2
{
    public static class Enumerations
    {
        /// <summary>
        /// Transaction status codes
        /// (as returned from PXOrder.Complete or PXOrder.Check)
        /// http://pim.payex.com/Section3/section3_5.htm
        /// </summary>
        public enum TransactionStatusCode
        {
            /// <summary>
            /// This is a successful, one phased transaction.
            /// </summary>
            Sale = 0,

            /// <summary>
            /// We have initialized communication towards a third party payment provider (like VISA), but no result has been received.
            /// </summary>
            Initialize = 1,

            /// <summary>
            /// This transaction has been credited.
            /// </summary>
            Credit = 2,

            /// <summary>
            /// This is a successful authorization(reservation), which is the first step in a two phase transaction. The transaction
            /// amount has been reserved from the customers account.
            /// </summary>
            Authorize = 3,

            /// <summary>
            /// This is a cancelled authorization.
            /// </summary>
            Cancel = 4,

            /// <summary>
            /// This is a failed transaction. An example would be if we have attempted to perform a transaction towards a third party
            /// payment provder (like VISA), but received a negative result.
            /// </summary>
            Failure = 5,

            /// <summary>
            /// This is a successful capture, the second and last step of a two phased transaction. The customer has now been debited.
            /// </summary>
            Capture = 6
        }

        /// <summary>
        /// This indicates that no error has occurred, and the operation completed successfully.
        /// </summary>
        public static readonly string OK = "OK";

        /// <summary>
        /// This indicates a generic/unspecified error processing the request. The description field may or may not supply
        /// additional information.
        /// </summary>
        public static readonly string ValidationError_Generic = "ValidationError_Generic";

        /// <summary>
        /// This indicates that the supplied merchantAccountNumber or encryption key was incorrect, or you are not properly
        /// generating the MD5 hash for the hash parameter.
        /// </summary>
        public static readonly string ValidationError_HashNotValid = "ValidationError_HashNotValid";

        /// <summary>
        /// This indicates that the supplied merchantAccountNumber or encryption key was incorrect.
        /// </summary>
        public static readonly string ValidationError_LoginFailed = "ValidationError_LoginFailed";

        /// <summary>
        /// This indicates that one or more parameters supplied was incorrect. The description field will indicate the first
        /// invalid field encountered.
        /// </summary>
        public static readonly string ValidationError_InvalidParameter = "ValidationError_InvalidParameter";

        /// <summary>
        /// This indicates that even though basic verification of the parameters was successful, one or more parameters was
        /// logically invalid (like i.e a non-existing reference to a user or order). The description field will supply more
        /// detailed information.
        /// </summary>
        public static readonly string ValidationError_InvalidData = "ValidationError_InvalidData";

        /// <summary>
        /// This may be returned from PxOrder.Complete or PxOrder.Check if you request a non-existing transaction. This may be
        /// returned even though you enter a valid orderRef when invoking PxOrder.Complete, as even though you have created an
        /// order, the customer may not have successfully carried out the transaction.
        /// </summary>
        public static readonly string Error_NoRecordFound = "Error_NoRecordFound";

        /// <summary>
        /// This may be returned from any of the interfaces, ex. PxOrder.Initialize or PxAgreement.CreateAgreement. If you post a
        /// request without having registered your server's source IP address at first, you'll get this error code. Please login
        /// into PayEx Admin og register it at Merchant Profile. Make sure you update this information if your server gets a new IP
        /// address.
        /// </summary>
        public static readonly string Merchant_InvalidIpAddress = "Merchant_InvalidIpAddress";

        public static readonly string Order_ErrorSet = "Order_ErrorSet";

        public static readonly string Check_OrderInProcess = "Check_OrderInProcess";

        public static readonly string Check_OrderNotExists = "Check_OrderNotExists";
    }
}