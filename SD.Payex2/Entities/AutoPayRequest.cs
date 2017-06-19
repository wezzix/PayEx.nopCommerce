namespace SD.Payex2.Entities
{
    /// <summary>
    /// Parameters for a call to AutoPay.
    /// </summary>
    public class AutoPayRequest
    {
        /// <summary>
        /// Required. Ref to an agreement that will be charged.
        /// </summary>
        public string AgreementRef { get; set; }

        /// <summary>
        /// Required. Amount the merchant is charging.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Required. Product number of the product the client is purchasing.
        /// </summary>
        public string ProductNumber { get; set; }

        /// <summary>
        /// Required. Product description of the product the client is purchasing.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Required. The merchant should use this parameter to send in a local ID identifying this particular order.
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// Optional. SALE | AUTHORIZATION.
        /// If AUTHORIZATION is submitted, this indicates that the order will be a 2-phased transaction if the payment method
        /// supports it.
        /// If empty, uses the value when the agreement was created.
        /// </summary>
        public PayexInterface.PurchaseOperation? PurchaseOperation { get; set; }

        /// <summary>
        /// Optional. Sets the currency for the transaction to be debited the customer.
        /// </summary>
        public string CurrencyCode { get; set; }
    }
}