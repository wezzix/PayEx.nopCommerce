using System;

namespace SD.Payex2.Entities
{
    /// <summary>
    /// Parameters for a call to CreateAgreement.
    /// </summary>
    public class CreateAgreementRequest
    {
        /// <summary>
        /// Required. A reference that links this agreement to something the merchant takes money for.
        /// </summary>
        public string MerchantRef { get; set; }

        /// <summary>
        /// Required. A short description about this agreement. This will show up on the client admin page so that the client gets
        /// info about the agreement. It will also show on the web page where the client verifies the agreement.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// SALE | AUTHORIZATION.
        /// If AUTHORIZATION is submitted, this indicates that the order will be a 2-phased transaction if the payment method
        /// supports it.
        /// This is the value that will be used in AutoPay if the purchaseOperation parameter is left empty in the AutoPay call
        /// </summary>
        public PayexInterface.PurchaseOperation PurchaseOperation { get; set; }

        /// <summary>
        /// Required. One single transaction can never be greater than this amount.
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// If this parameter is set there is a start date on this agreement and the agreement don’t start wotking before this
        /// date/time.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// If this parameter is set there is a stop date on this agreement and the agreement will not work after this date/time.
        /// If there are a recurring autopay using this agreement this will have to be deleted when the stop date occurs.
        /// </summary>
        public DateTime? StopDate { get; set; }
    }
}