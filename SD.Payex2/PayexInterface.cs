using System;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SD.Payex2.Crypto;
using SD.Payex2.Entities;
using SD.Payex2.Services.PxAgreement;
using SD.Payex2.Services.PxOrder;
using SD.Payex2.Utilities;
using InitializeRequest = SD.Payex2.Entities.InitializeRequest;

namespace SD.Payex2
{
    public class PayexInterface
    {
        public enum PurchaseOperation
        {
            Sale,
            Authorization,
        }

        #region Constructors

        public PayexInterface(PayexAccount account)
        {
            Account = account;
        }

        #endregion

        #region Private Methods

        private string GetSupportedLanguage(string[] clientLanguages)
        {
            // Supported languages: nb-NO,da-DK,en-US,sv-SE,es-ES,de-DE,fi-FI,fr-FR. If no language is specified, the default language for client UI is used. If wrong language, then an error occurs.
            string lang = null;

            foreach (var l in clientLanguages)
            {
                var a = l.ToLower();
                if (a.StartsWith("sv"))
                    lang = "sv-SE";
                else if (a.StartsWith("en"))
                    lang = "en-US";
                else if (a.StartsWith("fi"))
                    lang = "fi-FI";
                else if (a.StartsWith("de"))
                    lang = "de-DE";
                else if (a.StartsWith("fr"))
                    lang = "fr-FR";
                else if (a.StartsWith("es"))
                    lang = "es-ES";
                else if (a.StartsWith("no") || a.StartsWith("nb") || a.StartsWith("nn"))
                    lang = "nb-NO";
                else if (a.StartsWith("da") || a.StartsWith("dk"))
                    lang = "da-DK";

                if (lang != null)
                    break;
            }

            return lang ?? string.Empty;
        }

        private PxOrderSoapClient GetPxOrderClient()
        {
			//PayEx requires TLS 1.2 since 2017.11.20 at 09:00
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			
            Uri baseAddress;
            if (UseTestEnvironment)
                baseAddress = new Uri("https://external.externaltest.payex.com/pxorder/pxorder.asmx");
            else
                baseAddress = new Uri("https://external.payex.com/pxorder/pxorder.asmx");
            var payexOrder = new PxOrderSoapClient(
                new BasicHttpBinding(BasicHttpSecurityMode.Transport),
                new EndpointAddress(baseAddress)
            );
            return payexOrder;
        }

        private PxAgreementSoapClient GetPxAgreementClient()
        {
			//PayEx requires TLS 1.2 since 2017.11.20 at 09:00
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			
            Uri baseAddress;
            if (UseTestEnvironment)
                baseAddress = new Uri("https://external.externaltest.payex.com/pxagreement/pxagreement.asmx");
            else
                baseAddress = new Uri("https://external.payex.com/pxagreement/pxagreement.asmx");
            var payexOrder = new PxAgreementSoapClient(
                new BasicHttpBinding(BasicHttpSecurityMode.Transport),
                new EndpointAddress(baseAddress)
            );
            return payexOrder;
        }

        #endregion

        #region Public properties

        public PayexAccount Account { get; set; }
        public bool UseTestEnvironment { get; set; }

        #endregion

        #region Public Methods

        #region PxOrder

        /// <summary>
        /// The Initialize method call is the backbone of the credit card implementation and all other payment methods. This method
        /// is used for setting up the transaction, specifying the price of the transaction and the purchase operation.<br />
        /// Initialize is used to supply PayEx with all the necessary data to initialize an order. Upon the successful
        /// initialization of an order, a reference (orderRef) is returned to the merchant. The merchant may then redirect the user
        /// or alternatively call one of the server to server methods.
        /// Documentation: http://www.payexpim.com/technical-reference/pxorder/initialize8/
        /// </summary>
        /// <param name="request">The parameters to the Initialize request</param>
        public async Task<InitializeResult> Initialize(InitializeRequest request)
        {
            // Validation
            if (request == null)
                throw new ArgumentNullException(nameof(request), "request is required");
            if (string.IsNullOrEmpty(request.OrderID))
                throw new ArgumentNullException("OrderID", "OrderID is required");
            if (string.IsNullOrEmpty(request.CurrencyCode))
                throw new ArgumentNullException("CurrencyCode", "CurrencyCode is required");
            if (request.Amount <= 0)
                throw new ArgumentOutOfRangeException("Price", "Price must be non-zero.");

            var clientIdentifier = "USERAGENT=" + (request.UserAgent ?? string.Empty);

            var additionalValues = "RESPONSIVE=1";
            if (!string.IsNullOrWhiteSpace(request.MobilePhoneNumber))
                additionalValues += $"&MSISDN={Regex.Replace(request.MobilePhoneNumber, @"\D", "")}";

            var convPrice = request.Amount.ToPayEx();
            var priceArgList = string.Empty;
            // string.Format("VISA={0},MC={0},AMEX={0},FSPA={0},NB={0},SHB={0},SEB={0},SWISH={0}", convPrice);
            long usedPrice = convPrice;
            var vat = request.VatPercent.ToPayEx(); // Percent * 100
            var externalID = string.Empty;
            var view = request.View ?? "CREDITCARD"; // Default payment method.

            // Build string for md5 including all fields except empty strings and description field
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(request.PurchaseOperation.ToPayEx());
            hashInput.Append(usedPrice);
            hashInput.Append(priceArgList);
            hashInput.Append(request.CurrencyCode);
            hashInput.Append(vat);
            hashInput.Append(request.OrderID);
            hashInput.Append(request.ProductNumber);
            hashInput.Append(request.Description);
            hashInput.Append(request.ClientIPAddress);
            hashInput.Append(clientIdentifier);
            hashInput.Append(additionalValues);
            hashInput.Append(externalID);
            hashInput.Append(request.ReturnURL);
            hashInput.Append(view);
            hashInput.Append(request.AgreementRef);
            hashInput.Append(request.CancelUrl);
            hashInput.Append(request.ClientLanguage);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.Initialize8Async(
                Account.AccountNumber,
                request.PurchaseOperation.ToPayEx(),
                usedPrice,
                priceArgList,
                request.CurrencyCode,
                vat,
                request.OrderID,
                request.ProductNumber ?? "",
                request.Description ?? "",
                request.ClientIPAddress ?? "",
                clientIdentifier,
                additionalValues,
                externalID,
                request.ReturnURL ?? "",
                view,
                request.AgreementRef ?? "",
                request.CancelUrl ?? "",
                request.ClientLanguage ?? "",
                hash);

            // Parse the result
            var result = ResultParser.ParseInitializeResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// This will allow merchants to do add multiple orderlines to a PayEx transaction.
        /// Run this method for every order line you want to add to the order. It is run after initialize since you need the
        /// orderRef to reference the order lines to.
        /// </summary>
        public async Task<BaseResult> AddSingleOrderLine(AddSingleOrderLineRequest request)
        {
            // Validation
            if (request == null)
                throw new ArgumentNullException(nameof(request), "request is required");
            if (string.IsNullOrEmpty(request.OrderRef))
                throw new ArgumentNullException("OrderRef", "OrderRef is required");
            if (string.IsNullOrEmpty(request.ItemNumber))
                throw new ArgumentNullException("ItemNumber", "ItemNumber is required");
            if (string.IsNullOrEmpty(request.ItemDescription1))
                throw new ArgumentNullException("ItemDescription1", "ItemDescription1 is required");
            //if (request.Quantity <= 0)
            //    throw new ArgumentOutOfRangeException("Quantity", "Quantity must be positive.");
            //if (request.Amount < 0)
            //    throw new ArgumentOutOfRangeException("Amount", "Amount must be positive.");

            var convAmount = request.Amount.ToPayEx();
            var vatPrice = request.VatAmount.ToPayEx();
            var vatPercent = request.VatPercent.ToPayEx();

            // Build string for md5 hash
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(request.OrderRef);
            hashInput.Append(request.ItemNumber);
            hashInput.Append(request.ItemDescription1);
            hashInput.Append(request.ItemDescription2);
            hashInput.Append(request.ItemDescription3);
            hashInput.Append(request.ItemDescription4);
            hashInput.Append(request.ItemDescription5);
            hashInput.Append(request.Quantity);
            hashInput.Append(convAmount);
            hashInput.Append(vatPrice);
            hashInput.Append(vatPercent);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.AddSingleOrderLine2Async(
                Account.AccountNumber, request.OrderRef, request.ItemNumber,
                request.ItemDescription1,
                request.ItemDescription2 ?? "",
                request.ItemDescription3 ?? "",
                request.ItemDescription4 ?? "",
                request.ItemDescription5 ?? "",
                request.Quantity, convAmount, vatPrice, vatPercent, hash);

            // Parse the result
            var result = ResultParser.ParseBaseResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// After performing an authorization, the merchant typically ensures that the customer will receive the item/service he
        /// ordered (i.e waits until the goods have been shipped) before performing a capture. The capture actually charges the
        /// customer with the authorized amount.
        /// </summary>
        /// <param name="transactionNumber">The transactionNumber of the transaction you wish to capture.</param>
        /// <param name="amount">The amount you wish to capture.</param>
        /// <param name="orderID">
        /// Order Id that will be presented in PayEx report. This value have to be numeric if merchant have
        /// chosen to send orderId to the aquiring institution.
        /// </param>
        public async Task<CaptureResult> Capture(int transactionNumber, decimal amount, string orderID)
        {
            // Validation
            if (transactionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), "transactionNumber is required");
            if (amount <= decimal.Zero)
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must be non-zero.");
            // Null not allowed
            orderID = orderID ?? string.Empty;
            var vatAmount = 0;
            var additionalValues = string.Empty;

            // Build string for md5 including all fields except empty strings
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(transactionNumber);
            hashInput.Append(amount.ToPayEx());
            hashInput.Append(orderID);
            hashInput.Append(vatAmount);
            hashInput.Append(additionalValues);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.Capture5Async(
                Account.AccountNumber,
                transactionNumber,
                amount.ToPayEx(),
                orderID,
                vatAmount,
                additionalValues,
                hash);

            // Parse the result and retrieve code-node to figure out if the service method was invoked successfully.
            var result = ResultParser.ParseCaptureResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// Credit a transaction after a completed purchase.
        /// The credit functionality can behave differently based on the payment instrument and/or if the financial institution
        /// accepts credit instructions (not all do). Also note that some financial institutions don’t support partial credit
        /// before 24 hours have past. Please contact PayEx Support for more information.
        /// </summary>
        /// <param name="transactionNumber">The transactionNumber of the transaction you wish to credit.</param>
        /// <param name="amount">The amount you wish to credit.</param>
        /// <param name="orderID">
        /// Order Id that will be presented in PayEx report. This value have to be numeric if merchant have
        /// chosen to send orderId to the aquiring institution.
        /// </param>
        public async Task<CreditResult> Credit(int transactionNumber, decimal amount, string orderID)
        {
            // Validation
            if (transactionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), "transactionNumber is required");
            if (amount <= decimal.Zero)
                throw new ArgumentOutOfRangeException(nameof(amount), "amount must be non-zero.");
            // Null not allowed
            orderID = orderID ?? string.Empty;
            var convAmount = amount.ToPayEx();
            var vatAmount = 0;
            var additionalValues = string.Empty;

            // Build string for md5 including all fields except empty strings
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(transactionNumber);
            hashInput.Append(convAmount);
            hashInput.Append(orderID);
            hashInput.Append(vatAmount);
            hashInput.Append(additionalValues);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.Credit5Async(
                Account.AccountNumber,
                transactionNumber,
                convAmount,
                orderID,
                vatAmount,
                additionalValues,
                hash);

            // Parse the result and retrieve code-node to figure out if the service method was invoked successfully.
            var result = ResultParser.ParseCreditResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// Send a cancel transaction after an authorize transaction to cancel it.
        /// </summary>
        /// <param name="transactionNumber">The transactionNumber of the transaction you wish to cancel.</param>
        public async Task<CancelResult> Cancel(int transactionNumber)
        {
            // Validation
            if (transactionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), "transactionNumber is required");

            // Build string for md5 including all fields except empty strings
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(transactionNumber);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.Cancel2Async(Account.AccountNumber, transactionNumber, hash);

            // Parse the result and retrieve code-node to figure out if the service method was invoked successfully.
            var result = ResultParser.ParseCancelResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// When the user is returned after performing/cancelling an order, the status of the transaction is not returned in the
        /// querystring.
        /// Thus the merchant needs to call the Completes method to retrieve the transaction status.
        /// It is important to notice that this function can only be used in combination with Initialize.
        /// If you at a later time want to get the transaction status you will need to call the Check function.
        /// Complete may only be called once for each transaction.
        /// Note: You have to check both errorCode and transactionStatus to be sure the transaction was successful.
        /// You need to save transactionRef and optionally transactionNumber as a reference to the transaction.
        /// The orderRef used during initialize and purchase is deleted upon completion of the transaction.
        /// Returns a boolean indicating if the request was successful.
        /// </summary>
        /// <param name="orderRef"></param>
        /// <returns>A boolean indicating if the result was transferred without errors. See also TransactionComplete.</returns>
        public async Task<CompleteResult> Complete(string orderRef)
        {
            // Validation
            if (string.IsNullOrEmpty(orderRef))
                throw new ArgumentNullException(nameof(orderRef), "orderRef is required");

            // Build string for md5 including all fields except empty strings and description field
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(orderRef);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Make hash
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Call web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.CompleteAsync(Account.AccountNumber, orderRef, hash);

            // Parse the result
            var result = ResultParser.ParseCompleteResult(xmlReturn);

            return result;
        }

        public async Task<TransactionDetailsResult> GetTransactionDetails(int transactionNumber)
        {
            // Validation
            if (transactionNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), "transactionNumber is required");

            // Build string for md5 including all fields except empty strings
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(transactionNumber);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexOrder = GetPxOrderClient();
            var xmlReturn = await payexOrder.GetTransactionDetails2Async(Account.AccountNumber, transactionNumber, hash);

            // Parse the result and retrieve code-node to figure out if the service method was invoked successfully.
            var result = ResultParser.ParseTransactionDetailsResult(xmlReturn);
            return result;
        }

        #endregion

        #region PxAgreement

        /// <summary>
        /// Creates a new agreement between the merchant and the client. Before any AutoPay transactions can take place the client
        /// has to complete a purchase with the agreement reference. The agreement will be set to verified when this is done.
        /// Documentation: http://www.payexpim.com/technical-reference/pxagreement/createagreement3/
        /// </summary>
        /// <param name="request">The parameters to the CreateAgreement request</param>
        public async Task<CreateAgreementResult> CreateAgreement(CreateAgreementRequest request)
        {
            // Validation
            if (request == null)
                throw new ArgumentNullException(nameof(request), "request is required");

            var notifyUrl = string.Empty; // Deprecated, leave blank

            // Build string for md5 including all fields except empty strings and description field
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(request.MerchantRef);
            hashInput.Append(request.Description);
            hashInput.Append(request.PurchaseOperation.ToPayEx());
            hashInput.Append(request.MaxAmount.ToPayEx());
            hashInput.Append(notifyUrl);
            hashInput.Append(request.StartDate.ToPayEx());
            hashInput.Append(request.StopDate.ToPayEx());
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexAgreement = GetPxAgreementClient();
            var xmlReturn = await payexAgreement.CreateAgreement3Async(
                Account.AccountNumber,
                request.MerchantRef ?? "",
                request.Description ?? "",
                request.PurchaseOperation.ToPayEx(),
                request.MaxAmount.ToPayEx(),
                notifyUrl,
                request.StartDate.ToPayEx(),
                request.StopDate.ToPayEx(),
                hash);

            // Parse the result
            var result = ResultParser.ParseCreateAgreementResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// Makes a transaction when there exist a verified agreement between the client and the merchant.
        /// In case of payment failure: Please use a minimum of 30 minutes delay between the first try and the second. If the
        /// transaction still fails, please wait a couple of hours before the next try. After a total period of 8 hours, you should
        /// stop trying to charge the customer.
        /// Documentation: http://www.payexpim.com/technical-reference/pxagreement/autopay/
        /// </summary>
        /// <param name="request">The parameters to the AutoPay request</param>
        public async Task<AutoPayResult> AutoPay(AutoPayRequest request)
        {
            // Validation
            if (request == null)
                throw new ArgumentNullException(nameof(request), "request is required");

            // Build string for md5 including all fields except empty strings and description field
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(request.AgreementRef);
            hashInput.Append(request.Amount.ToPayEx());
            hashInput.Append(request.ProductNumber);
            hashInput.Append(request.Description);
            hashInput.Append(request.OrderID);
            hashInput.Append(request.PurchaseOperation.ToPayEx());
            hashInput.Append(request.CurrencyCode);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexAgreement = GetPxAgreementClient();
            var xmlReturn = await payexAgreement.AutoPay3Async(
                Account.AccountNumber,
                request.AgreementRef ?? "",
                request.Amount.ToPayEx(),
                request.ProductNumber ?? "",
                request.Description ?? "",
                request.OrderID ?? "",
                request.PurchaseOperation.ToPayEx(),
                request.CurrencyCode ?? "",
                hash);

            // Parse the result
            var result = ResultParser.ParseAutoPayResult(xmlReturn);

            return result;
        }

        /// <summary>
        /// This method deletes an existing agreement between a customer and a merchant with given agreementRef.
        /// All recurring agreements connected to the agreementRef will also be deleted.
        /// </summary>
        /// <param name="request">Ref to an agreement that will be deleted.</param>
        public async Task<DeleteAgreementResult> DeleteAgreement(string agreementRef)
        {
            // Build string for md5 including all fields except empty strings and description field
            var hashInput = new StringBuilder();
            hashInput.Append(Account.AccountNumber);
            hashInput.Append(agreementRef);
            // Add encryption key at the end of string to be hashed
            hashInput.Append(Account.EncryptionKey);
            // Create a hash string from the parameters
            string hash;
            MD5Hash.Hash(hashInput.ToString(), out hash);

            // Invoke Initialize method on external PayEx PxOrder web service
            var payexAgreement = GetPxAgreementClient();
            var xmlReturn = await payexAgreement.DeleteAgreementAsync(Account.AccountNumber, agreementRef, hash);

            // Parse the result
            var result = ResultParser.ParseDeleteAgreementResult(xmlReturn);

            return result;
        }

        #endregion

        #endregion
    }
}