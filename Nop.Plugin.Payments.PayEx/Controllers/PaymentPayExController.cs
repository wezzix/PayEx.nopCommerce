using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Models;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using SD.Payex2;
using SD.Payex2.Entities;

namespace Nop.Plugin.Payments.PayEx.Controllers
{
    public class PaymentPayExController : BasePaymentController
    {
        #region Constructors

        public PaymentPayExController(
            ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            IPermissionService permissionService,
            IWebHelper webHelper,
            IWorkContext workContext,
            PaymentSettings paymentSettings,
            PayExPaymentSettings payExPaymentSettings,
            IPayExAgreementService payExAgreementService)
        {
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _permissionService = permissionService;
            _webHelper = webHelper;
            _payExPaymentSettings = payExPaymentSettings;
            _paymentSettings = paymentSettings;
            _payExAgreementService = payExAgreementService;
        }

        #endregion

        #region Private Member Variables

        internal const string PaymentSystemName = "Payments.PayEx";

        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly PayExPaymentSettings _payExPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPayExAgreementService _payExAgreementService;

        #endregion

        #region NonAction Methods

        [NonAction]
        public async Task<Order> DoComplete(string orderRef, bool isCallback)
        {
            // Get our payment processor
            var processor = _paymentService.LoadPaymentMethodBySystemName(PaymentSystemName) as PayExPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayEx module is not active or cannot be loaded.");

            if (string.IsNullOrWhiteSpace(orderRef))
                return null;

            // Call complete
            CompleteResult result = await processor.Complete(orderRef);

            // Attempt to get the order associated with the transaction.
            Order order = null;
            if (!string.IsNullOrEmpty(result.OrderID)
                && Guid.TryParse(result.OrderID, out var orderGuid))
                order = _orderService.GetOrderByGuid(orderGuid);

            // If the transaction is pending, then we are expecting a callback to this method within 3 seconds due to occational delay from Swish in m-commerce flow.
            // Upon completion in separate process, the order PaymentStatus will have changed from Pending to Paid or Failed.
            if (order != null && result.Pending == true && !isCallback)
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(1000);
                    order = _orderService.GetOrderById(order.Id);
                    // If the order status changes from pending then the below code has already been run by a separate process. We are done.
                    if (order.PaymentStatus != PaymentStatus.Pending)
                        return order;
                }

                // This should very rarely happen. But in case the transaction callback failed, we can check it here again.
                result = await processor.Complete(orderRef);
            }

            if (order != null && result.IsTransactionSuccessful)
            {
                // Add order note
                var note = new StringBuilder();
                note.AppendLine($"PayEx: Complete SUCCEEDED {(isCallback ? "synchronously" : "via callback")}");
                note.AppendLine("PaymentMethod: " + result.PaymentMethod);
                note.AppendLine(string.Format("Amount: {0:n2}", result.Amount));
                note.AppendLine("TransactionNumber: " + result.TransactionNumber);
                note.AppendLine("TransactionStatus: " + result.TransactionStatus);
                order.OrderNotes.Add(
                    new OrderNote
                    {
                        Note = note.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                    });
                _orderService.UpdateOrder(order);

                // Validate order total
                if (_payExPaymentSettings.ValidateOrderTotal
                    && !Math.Round(result.Amount.Value, 2).Equals(Math.Round(order.OrderTotal, 2)))
                {
                    string errorStr = string.Format(
                        "PayEx Complete. Returned order total {0:n2} doesn't equal order total {1:n2}", result.Amount,
                        order.OrderTotal);
                    _logger.Error(errorStr, customer: order.Customer);

                    return order;
                }

                UpdateAgreement(result, order);

                UpdateOrderPaymentStatus(result, order);

                return order;
            }
            //else // Transaction not successful
            //{
            //    string note = "Transaction was not successful.";
            //    if (order != null)
            //        AddOrderNote(order, note);
            //    return false;
            //}
            else // Request or transaction not successful
            {
                // Logg error or failure
                var note = new StringBuilder();
                note.AppendLine("PayEx: Complete FAILED");
                note.AppendLine("ErrorCode: " + result.ErrorCode);
                note.AppendLine("Description: " + result.Description);
                note.AppendLine("ThirdPartyError: " + result.ThirdPartyError);
                note.AppendLine("ParamName: " + result.ParamName);
                note.AppendLine("TransactionStatus: " + result.TransactionStatus);
                if (result.TransactionErrorCode != null)
                {
                    note.AppendLine("TransactionErrorCode: " + result.TransactionErrorCode);
                    note.AppendLine("TransactionErrorDescription: " + result.TransactionErrorDescription);
                    note.AppendLine("TransactionThirdPartyError: " + result.TransactionThirdPartyError);
                }

                note.AppendLine("PaymentMethod: " + result.PaymentMethod);
                note.AppendLine(string.Format("Amount: {0:n2}", result.Amount));
                note.AppendLine("AlreadyCompleted: " + result.AlreadyCompleted);
                note.AppendLine("OrderID: " + result.OrderID);
                if (!result.IsRequestSuccessful)
                    _logger.Error(note.ToString());
                if (order != null)
                    AddOrderNote(order, note.ToString());
            }

            return null;
        }

        private void UpdateAgreement(CompleteResult result, Order order)
        {
            // Check if the transaction is based on an agreement
            if (string.IsNullOrEmpty(result.AgreementRef))
                return;

            order.SubscriptionTransactionId = result.AgreementRef;
            // Check if the agreement already exists
            PayExAgreement agreement = _payExAgreementService.GetByAgreementRef(result.AgreementRef);
            if (agreement == null)
            {
                // Save the new agreement
                agreement = new PayExAgreement
                {
                    AgreementRef = result.AgreementRef,
                    CustomerId = order.Customer.Id,
                    MaxAmount = _payExPaymentSettings.AgreementMaxAmount,
                    Name = result.MaskedNumber ?? "Sparat kort", // TODO
                    PaymentMethod = result.PaymentMethod,
                    PaymentMethodExpireDate = result.PaymentMethodExpireDate,
                    PaymentMethodSystemName = order.PaymentMethodSystemName,
                    NumberOfUsages = 1,
                    LastUsedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                };
                _payExAgreementService.InsertPayExAgreement(agreement);
            }
            else
            {
                // Log usage
                agreement.NumberOfUsages++;
                agreement.LastUsedDate = DateTime.UtcNow;
                _payExAgreementService.UpdatePayExAgreement(agreement);
            }
        }

        private void UpdateOrderPaymentStatus(BaseTransactionResult result, Order order)
        {
            switch (result.TransactionStatus.Value)
            {
                case Enumerations.TransactionStatusCode.Sale:
                case Enumerations.TransactionStatusCode.Capture: // Capture should not happen here
                    order.CaptureTransactionId = result.TransactionNumber;
                    _orderService.UpdateOrder(order);
                    // Mark order as paid
                    if (result.TransactionStatus == Enumerations.TransactionStatusCode.Sale &&
                        _orderProcessingService.CanMarkOrderAsPaid(order))
                        _orderProcessingService.MarkOrderAsPaid(order);
                    break;
                case Enumerations.TransactionStatusCode.Authorize:
                    order.AuthorizationTransactionId = result.TransactionNumber;
                    _orderService.UpdateOrder(order);
                    if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                        _orderProcessingService.MarkAsAuthorized(order);
                    break;
            }
        }

        [NonAction]
        private void AddOrderNote(Order order, string note)
        {
            if (order != null)
            {
                // Add order note
                order.OrderNotes.Add(
                    new OrderNote
                    {
                        Note = note,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                _orderService.UpdateOrder(order);
            }
        }

        #endregion

        #region Action Methods

        private void PopulateConfigurationModel(ConfigurationModel model)
        {
            model.TransactionCallbackUrl = _webHelper.GetStoreLocation() + "Plugins/PaymentPayEx/TransactionCallback";
            model.TransactionModeValues = _payExPaymentSettings.TransactionMode.ToSelectList();
            if (_payExPaymentSettings.UseTestEnvironment)
                model.Message = "NOTE: Running against test servers.";
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            // Transfer settings to configuration model
            model.AccountNumber = _payExPaymentSettings.AccountNumber;
            model.EncryptionKey = _payExPaymentSettings.EncryptionKey;
            model.UseTestEnvironment =
                _payExPaymentSettings.UseTestEnvironment || _payExPaymentSettings.AccountNumber <= 0;
            model.PassProductNamesAndTotals = _payExPaymentSettings.PassProductNamesAndTotals;
            model.ValidateOrderTotal = _payExPaymentSettings.ValidateOrderTotal;
            model.AdditionalFee = _payExPaymentSettings.AdditionalFee;
            model.AllowCreateAgreement = _payExPaymentSettings.AllowCreateAgreement;
            model.AgreementMaxAmount = _payExPaymentSettings.AgreementMaxAmount;
            model.TransactionModeId = Convert.ToInt32(_payExPaymentSettings.TransactionMode);

            PopulateConfigurationModel(model);

            return View("~/Plugins/Payments.PayEx/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            // Save settings from configuration model
            _payExPaymentSettings.AccountNumber = model.AccountNumber;
            _payExPaymentSettings.EncryptionKey = model.EncryptionKey;
            _payExPaymentSettings.UseTestEnvironment =
                model.UseTestEnvironment || _payExPaymentSettings.AccountNumber <= 0;
            _payExPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            _payExPaymentSettings.ValidateOrderTotal = model.ValidateOrderTotal;
            _payExPaymentSettings.AdditionalFee = model.AdditionalFee;
            _payExPaymentSettings.AllowCreateAgreement = model.AllowCreateAgreement;
            _payExPaymentSettings.AgreementMaxAmount = model.AgreementMaxAmount;
            _payExPaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;

            _settingService.SaveSetting(_payExPaymentSettings);

            PopulateConfigurationModel(model);

            return View("~/Plugins/Payments.PayEx/Views/Configure.cshtml", model);
        }

        public virtual async Task<IActionResult> Complete(string orderRef)
        {
            var order = await DoComplete(orderRef, false);
            if (order?.PaymentStatus == PaymentStatus.Paid
                || order?.PaymentStatus == PaymentStatus.Authorized)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            PaymentFailedModel model = new PaymentFailedModel { OrderId = order?.Id ?? 0 };
            return View("~/Plugins/Payments.PayEx/Views/PaymentFailed.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TransactionCallback(IFormCollection form)
        {
            //string transactionRef = form["transactionRef"];
            //string transactionNumber = form["transactionNumber"];
            return await TransactionCallback(form["orderRef"]);
        }

#if DEBUG
        [HttpGet]
        public virtual async Task<IActionResult> TransactionCallbackGet(string orderRef)
        {
            // Used for debugging with GET.
            return await TransactionCallback(orderRef);
        }
#endif

        private async Task<IActionResult> TransactionCallback(string orderRef)
        {
            if (!string.IsNullOrWhiteSpace(orderRef))
            {
                try
                {
                    var order = await DoComplete(orderRef, true);
                    if (order != null)
                        return Content("OK");
                }
                catch (Exception)
                {
                    // FAILURE
                }
            }

            return Content("FAILURE");
        }

        public virtual IActionResult CancelOrder(IFormCollection form)
        {
            string id = Request.Query["id"];
            if (!string.IsNullOrEmpty(id))
            {
                //return RedirectToRoute("OrderDetails", new { orderId = id });
                if (int.TryParse(id, out int orderId))
                {
                    // Get order that was canceled
                    Order order = _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        if (order.OrderStatus == OrderStatus.Pending && _orderProcessingService.CanCancelOrder(order))
                        {
                            // Reorder items - place order items in shopping cart.
                            _orderProcessingService.ReOrder(order);
                            // Delete the order to avoid customer confusion
                            _orderProcessingService.DeleteOrder(order);
                        }

                        // Redirect customer to shopping cart
                        return RedirectToRoute("ShoppingCart");
                    }
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion
    }
}