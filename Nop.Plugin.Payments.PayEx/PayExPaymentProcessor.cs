using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.PayEx.Controllers;
using Nop.Plugin.Payments.PayEx.Data;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using SD.Payex2;
using SD.Payex2.Entities;

namespace Nop.Plugin.Payments.PayEx
{
    /// <summary>
    /// PayEx payment processor
    /// </summary>
    public class PayExPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Constructor

        public PayExPaymentProcessor(
            PayExPaymentSettings payExPaymentSettings,
            PayExAgreementObjectContext payExAgreementObjectContext,
            IPayExAgreementService payExAgreementService,
            ISettingService settingService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser,
            ITaxService taxService,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            IOrderService orderService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _payExPaymentSettings = payExPaymentSettings;
            _payExAgreementObjectContext = payExAgreementObjectContext;
            _settingService = settingService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _webHelper = webHelper;
            _checkoutAttributeParser = checkoutAttributeParser;
            _taxService = taxService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _orderService = orderService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
            _payExAgreementService = payExAgreementService;
        }

        #endregion

        #region Fields

        private const string PaymentViewCreditCard = "CREDITCARD";
        private readonly PayExAgreementObjectContext _payExAgreementObjectContext;
        private readonly PayExPaymentSettings _payExPaymentSettings;
        private readonly IPayExAgreementService _payExAgreementService;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        public static string AgreementRefKey = "AgreementRef";

        #endregion

        #region Utilities

        /// <summary>
        /// This determines the default payment method shown in the PayEx payment gateway.
        /// It can be overridden to show a different payment method.
        /// </summary>
        protected virtual string PaymentView => PaymentViewCreditCard;

        protected virtual PayexInterface.PurchaseOperation GetPurchaseOperation()
        {
            // Authorize only allowed for credit card payments and PayEx account (PX)
            if (_payExPaymentSettings.TransactionMode == TransactionMode.Authorize &&
                (PaymentView == PaymentViewCreditCard || PaymentView == "PX"))
                return PayexInterface.PurchaseOperation.Authorization;

            return PayexInterface.PurchaseOperation.Sale;
        }

        // Test account specifically created for testing the Payex for nopCommerce plugin. Any other use is not allowed.
        private const int TestAccount = 60006806;
        private const string TestEncryptionKey = "ub98c93DB9eKv42XTVRd";

        private PayexInterface GetPayexInterface()
        {
            PayexAccount account;
            if (_payExPaymentSettings.AccountNumber > 0)
                account = new PayexAccount(_payExPaymentSettings.AccountNumber, _payExPaymentSettings.EncryptionKey);
            else
                account = new PayexAccount(TestAccount, TestEncryptionKey);
            PayexInterface payex = new PayexInterface(account);
            payex.UseTestEnvironment =
                _payExPaymentSettings.UseTestEnvironment || _payExPaymentSettings.AccountNumber <= 0;
            return payex;
        }

        internal Task<CompleteResult> Complete(string orderRef)
        {
            PayexInterface payex = GetPayexInterface();

            var result = payex.Complete(orderRef);

            return result;
        }

        private void AddOrderLines(PayexInterface payex, string orderRef, Order order)
        {
            //get the items in the cart
            decimal cartTotal = decimal.Zero;
            var cartItems = order.OrderItems;
            int itemIndex = 1;
            foreach (var item in cartItems)
            {
                AddOrderLine(
                        payex, orderRef, item.Product.Sku ?? string.Format("{0}", itemIndex++), item.Product.Name,
                        item.Quantity, item.PriceInclTax)
                    .GetAwaiter().GetResult();
                cartTotal += item.PriceInclTax;
            }

            //the checkout attributes that have a value and send them to PayEx as items to be paid for
            var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);
            foreach (var val in caValues)
            {
                var caPriceInclTax = _taxService.GetCheckoutAttributePrice(val, true, order.Customer);
                CheckoutAttribute ca = val.CheckoutAttribute;
                if (caPriceInclTax > decimal.Zero && ca != null) //if it has a price
                {
                    AddOrderLine(payex, orderRef, string.Format("{0}", itemIndex++), ca.Name, 1, caPriceInclTax)
                        .GetAwaiter().GetResult();
                    cartTotal += caPriceInclTax;
                }
            }

            //order totals

            //shipping
            var orderShippingInclTax = order.OrderShippingInclTax;
            if (orderShippingInclTax > decimal.Zero)
            {
                AddOrderLine(
                        payex, orderRef, string.Format("{0}", itemIndex++),
                        _localizationService.GetResource("Order.Shipping"), 1, orderShippingInclTax)
                    .GetAwaiter().GetResult();
                cartTotal += orderShippingInclTax;
            }

            //payment method additional fee
            var paymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
            if (paymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                AddOrderLine(
                        payex, orderRef, string.Format("{0}", itemIndex++),
                        _localizationService.GetResource("Order.PaymentMethodAdditionalFee"), 1,
                        paymentMethodAdditionalFeeInclTax)
                    .GetAwaiter().GetResult();
                cartTotal += paymentMethodAdditionalFeeInclTax;
            }

            if (cartTotal > order.OrderTotal)
            {
                /* Take the difference between what the order total is and what it should be and use that as the "discount".
                 * The difference equals the amount of the gift card and/or reward points used.
                 */
                decimal discountTotal = cartTotal - order.OrderTotal;
                //gift card or rewared point amount applied to cart in nopCommerce
                AddOrderLine(
                        payex, orderRef, string.Format("{0}", itemIndex++),
                        _localizationService.GetResource("Admin.Orders.Products.Discount"), 1, -discountTotal)
                    .GetAwaiter().GetResult();
            }
        }

        private async Task<bool> AddOrderLine(
            PayexInterface payex, string orderRef, string itemNumber, string itemDescription, int quantity,
            decimal amount)
        {
            if (string.IsNullOrEmpty(itemNumber))
                return true; // Skip

            BaseResult lineResult = await payex.AddSingleOrderLine(
                new AddSingleOrderLineRequest
                {
                    OrderRef = orderRef,
                    ItemNumber = itemNumber,
                    ItemDescription1 = itemDescription,
                    Quantity = quantity,
                    Amount = amount,
                });
            return lineResult.IsRequestSuccessful;
        }

        #endregion

        #region IPaymentMethod Methods

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _payExPaymentSettings.AdditionalFee;
        }

        private string TryGetAgreementRef(Dictionary<string, object> customValues)
        {
            if (!customValues.TryGetValue(AgreementRefKey, out object agreementRefObject) || agreementRefObject == null)
                return null;

            return agreementRefObject.ToString();
        }

        /// <summary>
        /// This method is always invoked right before a customer places an order.
        /// Use it when you need to process a payment before an order is stored into database.
        /// For example, capture or authorize credit card. Usually this method is used when a customer
        /// is not redirected to third-party site for completing a payment and all payments
        /// are handled on your site (for example, PayPal Direct).
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;

            // Use an existing agreement to make the payment, if the customer chose this option.
            if (!processPaymentRequest.CustomValues.TryGetValue(AgreementRefKey, out object agreementRefObject)
                || agreementRefObject == null)
                return result;

            var agreementRef = TryGetAgreementRef(processPaymentRequest.CustomValues);
            if (!string.IsNullOrEmpty(agreementRef)
                && agreementRef != "new")
            {
                string currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)
                    .CurrencyCode;
                string description = string.Format("{0} - Order", _storeContext.CurrentStore.Name);
                AutoPayRequest request = new AutoPayRequest
                {
                    PurchaseOperation = GetPurchaseOperation(),
                    Amount = processPaymentRequest.OrderTotal,
                    CurrencyCode = currencyCode,
                    OrderID = processPaymentRequest.OrderGuid.ToString(),
                    ProductNumber = "ncOrder",
                    Description = description,
                    AgreementRef = agreementRef,
                };
                PayexInterface payex = GetPayexInterface();
                AutoPayResult autopayResult = payex.AutoPay(request).GetAwaiter().GetResult();

                // Check result and set new payment status
                if (autopayResult.IsTransactionSuccessful)
                {
                    result.SubscriptionTransactionId = agreementRef;
                    if (autopayResult.TransactionStatus.Value == Enumerations.TransactionStatusCode.Authorize)
                    {
                        result.NewPaymentStatus = PaymentStatus.Authorized;
                        result.AuthorizationTransactionId = autopayResult.TransactionNumber;
                        result.AuthorizationTransactionResult = autopayResult.ErrorCode;
                    }
                    else if (autopayResult.TransactionStatus.Value == Enumerations.TransactionStatusCode.Sale)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                        result.CaptureTransactionId = autopayResult.TransactionNumber;
                        result.CaptureTransactionResult = autopayResult.ErrorCode;
                    }
                }
                else
                {
                    _logger.Error(
                        string.Format("PayEx: AutoPay failed for order {0}.", processPaymentRequest.OrderGuid),
                        new NopException(autopayResult.GetErrorDescription()));
                }
            }

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            Order order = postProcessPaymentRequest.Order;

            // Make sure order is not already paid or authorized
            if (order.PaymentStatus == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.Authorized)
                return;

            string currencyCode =
                _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            string description = string.Format("{0} - Order", _storeContext.CurrentStore.Name);
            string userAgent;
            if (_httpContextAccessor.HttpContext.Request != null)
                userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            else
                userAgent = null;

            string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayEx/Complete";
            string cancelUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayEx/CancelOrder?id=" + order.Id;

            PayexInterface payex = GetPayexInterface();

            string agreementRef = null;
            // If the customer wishes to save his payment details, we make an agreement. 
            // This should be saved later in the complete operation, if it occurs.
            agreementRef = TryGetAgreementRef(order.DeserializeCustomValues());
            if (agreementRef == "new")
            {
                CreateAgreementRequest agreementRequest = new CreateAgreementRequest
                {
                    PurchaseOperation = GetPurchaseOperation(),
                    MerchantRef = order.OrderGuid.ToString(),
                    MaxAmount = _payExPaymentSettings.AgreementMaxAmount,
                    Description = description,
                };
                CreateAgreementResult agreementResult =
                    payex.CreateAgreement(agreementRequest).GetAwaiter().GetResult();
                if (agreementResult.IsRequestSuccessful)
                    agreementRef = agreementResult.AgreementRef;
                else
                {
                    _logger.Error(
                        string.Format("PayEx: CreateAgreement (for AutoPay) failed for order {0}.", order.Id),
                        new NopException(agreementResult.GetErrorDescription()), order.Customer);
                }
            }

            // Initialize the purchase and get the redirect URL
            InitializeRequest request = new InitializeRequest
            {
                PurchaseOperation = GetPurchaseOperation(),
                Amount = order.OrderTotal,
                CurrencyCode = currencyCode,
                OrderID = order.OrderGuid.ToString(),
                ProductNumber = "ncOrder",
                Description = description,
                AgreementRef = agreementRef,
                //VatPercent = 100M * order.OrderTax / order.OrderTotal,
                ClientIPAddress = order.CustomerIp,
                UserAgent = userAgent,
                ReturnURL = returnUrl,
                CancelUrl = cancelUrl,
                View = PaymentView,
                ClientLanguage = _workContext.WorkingLanguage?.LanguageCulture,
            };
            BeforeInitialize(postProcessPaymentRequest, request);

            InitializeResult result = payex.Initialize(request).GetAwaiter().GetResult();

            if (result.IsRequestSuccessful)
            {
                // Save OrderRef in case TransactionCallback fails or is implemented externally.
                order.AuthorizationTransactionCode = result.OrderRef;
                _orderService.UpdateOrder(order);
                if (_payExPaymentSettings.PassProductNamesAndTotals)
                    AddOrderLines(payex, result.OrderRef, order);
                // Redirect to PayEx
                _httpContextAccessor.HttpContext.Response.Redirect(result.RedirectUrl);
            }
            else
                throw new NopException(result.GetErrorDescription());
        }

        /// <summary>
        /// Makes it possible for derived controllers to modify the initialize request before it is sent.
        /// </summary>
        protected virtual void BeforeInitialize(
            PostProcessPaymentRequest postProcessPaymentRequest, InitializeRequest request)
        {
        }

        /// <summary>
        /// Some payment gateways allow you to authorize payments before they're captured. It allows store owners to review order
        /// details before the payment is actually done.
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            Order order = capturePaymentRequest.Order;
            if (int.TryParse(order.AuthorizationTransactionId, out int transactionNumber))
            {
                decimal amount = Math.Round(order.OrderTotal, 2);

                PayexInterface payex = GetPayexInterface();
                CaptureResult captureResult = payex.Capture(transactionNumber, amount, order.OrderGuid.ToString())
                    .GetAwaiter().GetResult();

                result.CaptureTransactionResult = captureResult.ErrorCode;
                result.CaptureTransactionId = captureResult.TransactionNumber;

                if (captureResult.IsTransactionSuccessful)
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    // Add order note
                    var note = new StringBuilder();
                    note.AppendLine("PayEx: Capture succeeded");
                    note.AppendLine(string.Format("Amount: {0:n2}", amount));
                    note.AppendLine("TransactionNumber: " + captureResult.TransactionNumber);
                    note.AppendLine("TransactionStatus: " + captureResult.TransactionStatus);
                    order.OrderNotes.Add(
                        new OrderNote
                        {
                            Note = note.ToString(),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    _orderService.UpdateOrder(order);
                }
                else
                    result.AddError(captureResult.GetErrorDescription());
            }
            else
            {
                result.Errors.Add(
                    string.Format(
                        "The order did not contain a valid TransactionNumber in the AuthorizationTransactionId field ('{0}').",
                        order.AuthorizationTransactionId));
            }

            return result;
        }

        /// <summary>
        /// This method allows you void an authorized but not captured payment. In this case a Void button will be visible on the
        /// order details page in admin area. Note that an order should be authorized and SupportVoid property should return true.
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            Order order = voidPaymentRequest.Order;
            if (int.TryParse(order.AuthorizationTransactionId, out int transactionNumber))
            {
                PayexInterface payex = GetPayexInterface();
                CancelResult cancelResult = payex.Cancel(transactionNumber).GetAwaiter().GetResult();

                if (cancelResult.IsRequestSuccessful)
                    result.NewPaymentStatus = PaymentStatus.Voided;
                else
                    result.AddError(cancelResult.GetErrorDescription());
            }
            else
            {
                result.AddError(
                    string.Format(
                        "The order did not contain a valid TransactionNumber in the AuthorizationTransactionId field ('{0}').",
                        order.AuthorizationTransactionId));
            }

            return result;
        }

        /// <summary>
        /// This method allows you make a refund. In this case a Refund button will be visible on the order details page in admin
        /// area. Note that an order should be paid, and SupportRefund or SupportPartiallyRefund property should return true.
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            Order order = refundPaymentRequest.Order;
            if (int.TryParse(order.CaptureTransactionId, out int transactionNumber))
            {
                decimal amount = Math.Round(refundPaymentRequest.AmountToRefund, 2);

                PayexInterface payex = GetPayexInterface();
                CreditResult creditResult = payex.Credit(transactionNumber, amount, order.OrderGuid.ToString())
                    .GetAwaiter().GetResult();

                if (creditResult.IsRequestSuccessful)
                {
                    // NOTE: We should save the transaction id for the refund, but no field is available in order.
                    // Add order note
                    var note = new StringBuilder();
                    note.AppendLine("PayEx: Credit succeeded");
                    note.AppendLine(string.Format("Credited amount: {0:n2}", amount));
                    note.AppendLine("TransactionNumber: " + creditResult.TransactionNumber);
                    note.AppendLine("TransactionStatus: " + creditResult.TransactionStatus);
                    order.OrderNotes.Add(
                        new OrderNote
                        {
                            Note = note.ToString(),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    _orderService.UpdateOrder(order);
                    // Set new payment status
                    if (refundPaymentRequest.IsPartialRefund
                        && refundPaymentRequest.AmountToRefund + order.RefundedAmount < order.OrderTotal)
                        result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                    else
                        result.NewPaymentStatus = PaymentStatus.Refunded;
                }
                else
                    result.AddError(creditResult.GetErrorDescription());
            }
            else
            {
                result.AddError(
                    string.Format(
                        "The order did not contain a valid TransactionNumber in the AuthorizationTransactionId field ('{0}').",
                        order.AuthorizationTransactionId));
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for
        /// redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //PayEx uses the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            //if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
            //    return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public virtual IList<string> ValidatePaymentForm(IFormCollection form) => new List<string>();

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public virtual ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            // Process info from the payment form
            if (!_workContext.CurrentCustomer.IsGuest() && _payExPaymentSettings.AllowCreateAgreement)
            {
                int.TryParse(form["payexagreement"], out int agreementId);
                bool createAgreement = !StringValues.IsNullOrEmpty(form["CreateAgreement"])
                    && string.Compare(form["CreateAgreement"][0], "true", StringComparison.InvariantCultureIgnoreCase) == 0;
                // There are no dedicated fields we can use, so reuse PurchaseOrderNumber for agreement info.
                if (agreementId > 0)
                {
                    PayExAgreement agreement = _payExAgreementService.GetById(agreementId);
                    if (agreement != null && agreement.CustomerId == _workContext.CurrentCustomer.Id)
                        paymentInfo.CustomValues.Add(AgreementRefKey, agreement.AgreementRef);
                }
                else if (createAgreement)
                    paymentInfo.CustomValues.Add(AgreementRefKey, "new");
            }

            return paymentInfo;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl() =>
            $"{_webHelper.GetStoreLocation()}Admin/PaymentPayEx/Configure";

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        public virtual void GetPublicViewComponent(out string viewComponentName) =>
            viewComponentName = "PaymentPayEx";

        #endregion

        #region BasePlugin Methods

        public override void Install()
        {
            // Do not overwrite existing settings.
            if (string.IsNullOrEmpty(_payExPaymentSettings.EncryptionKey))
            {
                var settings = new PayExPaymentSettings
                {
                    UseTestEnvironment = true,
                    AccountNumber = 0,
                    //EncryptionKey = "your encryption key",
                    TransactionMode = TransactionMode.AuthorizeAndCapture,
                    AgreementMaxAmount = 20000M,
                };
                _settingService.SaveSetting(settings);
            }

            this.AddOrUpdatePluginLocaleResource("Plugins.FriendlyName.Payments.PayEx", "Debit/Credit card");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.UseTestEnvironment", "Use test environment");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.UseTestEnvironment.Hint",
                "Uses the test service instead of the production service. Don't forget to update your account number and encryption key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AdditionalFee.Hint",
                "Enter an additional fee to charge your customers when using this payment method.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.PassProductNamesAndTotals", "Pass product names and totals");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.PassProductNamesAndTotals.Hint",
                "Check if product names and order totals should be passed to PayEx, instead of just order total.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.Fields.AccountNumber", "Account number");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AccountNumber.Hint", "Specify your account number with PayEx.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.Fields.EncryptionKey", "Encryption key");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.EncryptionKey.Hint", "Specify the encryption key for this account.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.TransactionModeValues", "Transaction mode credit card");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.TransactionModeValues.Hint",
                "When using Authorize, you will need to perform Capture manually to complete the transaction. When using Authorize and Capture, the sale will be completed instantly. Authorize is only possible with card payments.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.ValidateOrderTotal", "Validate order total");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.ValidateOrderTotal.Hint",
                "Check if we should validate our order total with the actual amount received from PayEx upon completing the transaction.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.CurrencyNotes",
                "If you're using this gateway ensure that your primary store currency is supported by PayEx.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.TransactionCallbackNotes",
                "It is required by PayEx that you use Transaction Callback to ensure Direct Debit payment info is received, and it's recommended for Credit Card payments. Enter the following URL for Transaction Callback in PayEx Merchant Admin:");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.RedirectionTip",
                "You will be redirected to the PayEx site to complete the payment, once you click Confirm.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.PaymentFailedPageTitle", "Your payment could not be processed");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.PaymentFailedInfo",
                "You may try to pay the order again by going to order details. If you wish to use another payment method you may place the order again by clicking order details. Please do not hesitate to contact us if you need any help.");

            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AllowCreateAgreement", "Allow save credit card");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AllowCreateAgreement.Hint",
                "Check to enable the customer to save a reference to a payment agreement on the initial purchase to be used for subsequent orders (AutoPay).");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AgreementMaxAmount", "Max amount for saved payments");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.Fields.AgreementMaxAmount.Hint",
                "Enter a maximum amount that should be allowed for purchases made using saved payment agreements.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.NewCard", "New card");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.SavedCards", "Saved cards");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEx.CreateAgreement", "Save my card");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.CreateAgreementMotivation",
                "- safely with PayEx. Your saved card may be used for future purchases.");
            this.AddOrUpdatePluginLocaleResource(
                "Plugins.Payments.PayEx.CreateAgreementInfo",
                @"The credit card information entered in the next step will be safely saved by our payment provider PayEx. In future checkouts you may choose the same credit card without having to enter the information again.
You may be prompted to enter your 3D secure code via an external link to your bank if you choose to save your credit card information.");

            if (PaymentView == PaymentViewCreditCard)
                _payExAgreementObjectContext.Install();

            base.Install();
        }

        public override void Uninstall()
        {
            // Only delete settings if uninstalling the main plugin, not the child plugins.
            if (PaymentView == "CREDITCARD")
            {
                _settingService.DeleteSetting<PayExPaymentSettings>();

                this.DeletePluginLocaleResource("Plugins.FriendlyName.Payments.PayEx");
                DeleteLocaleResource("Fields.UseTestEnvironment");
                DeleteLocaleResource("Fields.AdditionalFee");
                DeleteLocaleResource("Fields.PassProductNamesAndTotals");
                DeleteLocaleResource("Fields.AccountNumber");
                DeleteLocaleResource("Fields.EncryptionKey");
                DeleteLocaleResource("Fields.TransactionModeValues");
                DeleteLocaleResource("Fields.ValidateOrderTotal");
                DeleteLocaleResource("RegistrationNotes");
                DeleteLocaleResource("CurrencyNotes");
                DeleteLocaleResource("TransactionCallbackNotes");
                DeleteLocaleResource("RedirectionTip");
                DeleteLocaleResource("PaymentFailedPageTitle");
                DeleteLocaleResource("PaymentFailedInfo");

                DeleteLocaleResource("Fields.AllowCreateAgreement");
                DeleteLocaleResource("Fields.AgreementMaxAmount");
                DeleteLocaleResource("NewCard");
                DeleteLocaleResource("SavedCards");
                DeleteLocaleResource("CreateAgreement");
                DeleteLocaleResource("CreateAgreementMotivation");
                DeleteLocaleResource("CreateAgreementInfo");
                if (PaymentView == PaymentViewCreditCard)
                {
                    try
                    {
                        _payExAgreementObjectContext.Uninstall();
                    }
                    catch (Exception)
                    {
                        // Ignore table does not exist
                    }
                }
            }

            base.Uninstall();
        }

        protected void DeleteLocaleResource(string name)
        {
            this.DeletePluginLocaleResource($"Plugins.Payments.PayEx.{name}");
            this.DeletePluginLocaleResource($"Plugins.Payments.PayEx.{name}.Hint");
        }

        #endregion

        #region IPaymentMethod Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public virtual bool SkipPaymentInfo => !_payExPaymentSettings.AllowCreateAgreement && !RopcEnabled;

        public virtual string PaymentMethodDescription
            => _localizationService.GetResource("Plugins.Payments.PayEx.RedirectionTip");

        protected bool RopcEnabled => _settingService.GetSettingByKey(
            "realonepagecheckoutsettings.enablerealonepagecheckout", false, _storeContext.CurrentStore.Id, true);

        #endregion
    }
}