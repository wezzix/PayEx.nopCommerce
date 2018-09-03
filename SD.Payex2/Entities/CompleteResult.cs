﻿using System;

namespace SD.Payex2.Entities
{
    /// <summary>
    /// Result of a call to Complete()
    /// </summary>
    public class CompleteResult : BaseTransactionResult
    {
        /// <summary>
        /// This returns the orderID supplied by the merchant when the order was created,
        /// enabling the merchant to link the return data from PayEx with their local orderID.
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// Returns the payment method used to pay for this transaction (Payex, VISA, MC, etc).
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Returns the amount credited the merchant.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Returns false the first time complete is called successfully, but if complete is ever called with the same orderRef the
        /// returned value will be true.
        /// </summary>
        public bool? AlreadyCompleted { get; set; }

        /// <summary>
        /// Only used with Financing and PayPal payment methods. Returns true if we do not know the status of the transaction from
        /// third party, transactionStatus will be init.
        /// Swish: When implementing Complete its important to use the pending node in the xml response together with transaction
        /// status.
        /// Pending true means Payex is waiting transaction status from Swish. An example can be when end customer is redirected
        /// back to your merchant store before Payex has updated transaction status to 0(sale) – this can happend in the m-commerce
        /// flow.
        /// If Complete response returns transaction status 1 and pending true, you should wait for transaction callback before
        /// doing another Complete call to check the final status.
        /// </summary>
        public bool? Pending { get; set; }

        /// <summary>
        /// The agreementRef from CreateAgreement. Used on Autopay transactions.
        /// </summary>
        public string AgreementRef { get; set; }

        /// <summary>
        /// Expire date of the agreement.
        /// </summary>
        public DateTime? PaymentMethodExpireDate { get; set; }

        /// <summary>
        /// Returns the masked credit card number. Only returned for Agreements where the Initialize parameter View is set to CC.
        /// </summary>
        public string MaskedNumber { get; set; }

        /// <summary>
        /// Returns a error code of why the transaction failed.
        /// </summary>
        public string TransactionErrorCode { get; set; }

        /// <summary>
        /// Returns a description of why the transaction failed.
        /// </summary>
        public string TransactionErrorDescription { get; set; }

        /// <summary>
        /// Returns the thirdPartyError of why the transaction failed. We recommend all merchants to log this error with your
        /// orders. This info is very useful when contacting our support team.
        /// </summary>
        public string TransactionThirdPartyError { get; set; }

        /// <summary>
        /// Gets a value indicating if both the result and the complete transaction was successful.
        /// </summary>
        public override bool IsTransactionSuccessful => IsRequestSuccessful && TransactionStatus.HasValue &&
                                                        (TransactionStatus == Enumerations.TransactionStatusCode.Sale ||
                                                         TransactionStatus
                                                         == Enumerations.TransactionStatusCode.Authorize ||
                                                         TransactionStatus
                                                         == Enumerations.TransactionStatusCode.Capture);

        /// <summary>
        /// Gets a string describing any error during the request.
        /// </summary>
        public override string GetErrorDescription()
        {
            return $"PayEx Complete failed: {base.GetErrorDescription()}";
        }
    }
}