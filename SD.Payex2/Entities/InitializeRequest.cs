namespace SD.Payex2.Entities
{
    /// <summary>
    /// Parameters for a call to Initialize.
    /// </summary>
    public class InitializeRequest
    {
        /// <summary>
        /// SALE | AUTHORIZATION. If AUTHORIZATION is submitted, this indicates that the order will be a 2-phased transaction if
        /// the payment method supports it.
        /// </summary>
        public PayexInterface.PurchaseOperation PurchaseOperation { get; set; }

        /// <summary>
        /// This parameter determines the amount you would like to charge incl. VAT.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// VAT percent, for example 25%. Optional.
        /// </summary>
        public decimal VatPercent { get; set; }

        /// <summary>
        /// Set to your desired currency.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Use this to send in your local ID identifying this particular order. Using an unique orderID is strongly recommended.
        /// If you use invoice as payment method this string is restricted to these characters [a-zA-Z0-9]. This value have to be
        /// numeric if merchant have chosen to send orderId to the aquiring institution./
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// Merchant product number/reference for this specific product. We recommend that only the characters A-Z and 0-9 are used
        /// in this parameter.
        /// </summary>
        public string ProductNumber { get; set; }

        /// <summary>
        /// Merchant’s description of the product.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Here you send in the customers (clients) IP address.
        /// </summary>
        public string ClientIPAddress { get; set; }

        /// <summary>
        /// The information in this field is only used if you are implementing Credit Card in the direct model. It is used for
        /// 3D-secure verification. Send in your customers user agent.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// A string identifying the full URL for the page the user will be redirected to after a successful purchase. We will add
        /// orderRef to the existing query, and if no query is supplied to the URL, then the query will be added.
        /// </summary>
        public string ReturnURL { get; set; }

        /// <summary>
        /// A string identifying the full URL for the page the user will be redirected to when the Cancel Purchase button is
        /// pressed by the user. We do not add data to the end of this string. Set to blank if you don’t want this functionality.
        /// (Note: This is the PayEx cancel button, and must not be associated with cancel buttons in the customers bank.)
        /// </summary>
        public string CancelUrl { get; set; }

        /// <summary>
        /// Default payment method. Available string constants: PX | CREDITCARD | DIRECTDEBIT | CPA | IVR | EVC | INVOICE | LOAN |
        /// GC | CA | PAYPAL | FINANCING.
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// Specify the agreementRef (from PxAgreement.CreateAgreement) to open for recurring payments. The following payments
        /// should be performed by using PxAgreement.Autopay. Set to blank if you don’t want this functionality. Note: The customer
        /// must be informed of recurring payments.
        /// </summary>
        public string AgreementRef { get; set; }

        /// <summary>
        /// The language used in the redirect purchase dialog with the client. Available languages depend on the merchant
        /// configuration. Supported languages: nb-NO,da-DK,en-US,sv-SE,es-ES,de-DE,fi-FI,fr-FR,pl-PL,cs-CZ,hu-HU. If no language
        /// is specified, the default language for client UI is used.
        /// </summary>
        public string ClientLanguage { get; set; }
    }
}