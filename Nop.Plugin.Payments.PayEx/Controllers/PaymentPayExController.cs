using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Models;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using SD.Payex2;
using SD.Payex2.Entities;

namespace Nop.Plugin.Payments.PayEx.Controllers
{
    public class PaymentPayExController : BasePaymentController
    {
        #region Private Member Variables

        private const string PaymentSystemName = "Payments.PayEx";
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly PayExPaymentSettings _payExPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPayExAgreementService _payExAgreementService;

        #endregion

        #region Constructors
        public PaymentPayExController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger, IWebHelper webHelper,
            IWorkContext workContext,
            PaymentSettings paymentSettings,
            PayExPaymentSettings payExPaymentSettings,
            IPayExAgreementService payExAgreementService)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._payExPaymentSettings = payExPaymentSettings;
            this._paymentSettings = paymentSettings;
            this._payExAgreementService = payExAgreementService;
        }
        #endregion

        #region NonAction Methods

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public bool DoComplete(string orderRef, out int orderId)
        {
            orderId = 0;

            // Get our payment processor
            var processor = _paymentService.LoadPaymentMethodBySystemName(PaymentSystemName) as PayExPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayEx module is not active or cannot be loaded.");

            if (!string.IsNullOrWhiteSpace(orderRef))
            {
                // Call complete
                CompleteResult result = processor.Complete(orderRef);

                // Attempt to get the order associated with the transaction.
                Order order = null;
                if (!string.IsNullOrEmpty(result.OrderID))
                {
                    Guid orderGuid;
                    if (Guid.TryParse(result.OrderID, out orderGuid))
                        order = _orderService.GetOrderByGuid(orderGuid);
                }

                if (order != null)
                    orderId = order.Id;

                if (result.IsTransactionSuccessful && order != null)
                {
                    // Add order note
                    var note = new StringBuilder();
                    note.AppendLine("PayEx: Complete succeded");
                    note.AppendLine("PaymentMethod: " + result.PaymentMethod);
                    note.AppendLine(string.Format("Amount: {0:n2}", result.Amount));
                    note.AppendLine("TransactionNumber: " + result.TransactionNumber);
                    note.AppendLine("TransactionStatus: " + result.TransactionStatus);
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = note.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    // Validate order total
                    if (_payExPaymentSettings.ValidateOrderTotal && !Math.Round(result.Amount.Value, 2).Equals(Math.Round(order.OrderTotal, 2)))
                    {
                        string errorStr = string.Format("PayEx Complete. Returned order total {0:n2} doesn't equal order total {1:n2}", result.Amount, order.OrderTotal);
                        _logger.Error(errorStr, customer: order.Customer);

                        return false;
                    }

                    // Check if the transaction is based on an agreement
                    if (!string.IsNullOrEmpty(result.AgreementRef))
                    {
                        order.SubscriptionTransactionId = result.AgreementRef;
                        // Check if the agreement already exists
                        PayExAgreement agreement = _payExAgreementService.GetByAgreementRef(result.AgreementRef);
                        if (agreement == null)
                        {
                            // Save the new agreement
                            agreement = new PayExAgreement()
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

                    // Update order payment status
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
                        default:
                            break;
                    }
                    return true;
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
                    note.AppendLine("PayEx: Complete failed");
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
            }
            return false;
        }

        [NonAction]
        private void AddOrderNote(Order order, string note)
        {
            if (order != null)
            {
                // Add order note
                order.OrderNotes.Add(new OrderNote()
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

        [AdminAuthorize]
        [ChildActionOnly]
        public virtual ActionResult Configure()
        {
            var model = new ConfigurationModel();
            // Transfer settings to configuration model
            model.AccountNumber = _payExPaymentSettings.AccountNumber;
            model.EncryptionKey = _payExPaymentSettings.EncryptionKey;
            model.UseTestEnvironment = _payExPaymentSettings.UseTestEnvironment || _payExPaymentSettings.AccountNumber <= 0;
            model.PassProductNamesAndTotals = _payExPaymentSettings.PassProductNamesAndTotals;
            model.ValidateOrderTotal = _payExPaymentSettings.ValidateOrderTotal;
            model.AdditionalFee = _payExPaymentSettings.AdditionalFee;
            model.AllowCreateAgreement = _payExPaymentSettings.AllowCreateAgreement;
            model.AgreementMaxAmount = _payExPaymentSettings.AgreementMaxAmount;
            model.TransactionModeId = Convert.ToInt32(_payExPaymentSettings.TransactionMode);

            PopulateConfigurationModel(model);

            return View("~/Plugins/Payments.PayEx/Views/PaymentPayEx/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public virtual ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            // Save settings from configuration model
            _payExPaymentSettings.AccountNumber = model.AccountNumber;
            _payExPaymentSettings.EncryptionKey = model.EncryptionKey;
            _payExPaymentSettings.UseTestEnvironment = model.UseTestEnvironment || _payExPaymentSettings.AccountNumber <= 0;
            _payExPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            _payExPaymentSettings.ValidateOrderTotal = model.ValidateOrderTotal;
            _payExPaymentSettings.AdditionalFee = model.AdditionalFee;
            _payExPaymentSettings.AllowCreateAgreement = model.AllowCreateAgreement;
            _payExPaymentSettings.AgreementMaxAmount = model.AgreementMaxAmount;
            _payExPaymentSettings.TransactionMode = (TransactionMode)model.TransactionModeId;

            _settingService.SaveSetting(_payExPaymentSettings);

            PopulateConfigurationModel(model);

            return View("~/Plugins/Payments.PayEx/Views/PaymentPayEx/Configure.cshtml", model);
        }

        [ChildActionOnly]
        public virtual ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
#if AGREEMENT
            if (!_workContext.CurrentCustomer.IsGuest())
            {
                IEnumerable<PayExAgreement> agreements = _payExAgreementService
                    .GetValidAgreements(_workContext.CurrentCustomer.Id, PaymentSystemName)
                    .OrderByDescending(o => o.LastUsedDate);
                model.Agreements = agreements.Select(o => new SelectListItem()
                    {
                        Text = string.Format("{0} {1}", o.PaymentMethod, o.Name),
                        Value = o.Id.ToString()
                    }).ToList();
                model.AllowCreateAgreement = _payExPaymentSettings.AllowCreateAgreement;
            }
#endif

            return View("~/Plugins/Payments.PayEx/Views/PaymentPayEx/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            // Process info from the payment form
#if AGREEMENT
            if (!_workContext.CurrentCustomer.IsGuest() && _payExPaymentSettings.AllowCreateAgreement)
            {
                int agreementId;
                int.TryParse(form["payexagreement"], out agreementId);
                bool createAgreement = form["CreateAgreement"] != null && form["CreateAgreement"].Contains("true");
                // There are no dedicated fields we can use, so reuse PurchaseOrderNumber for agreement info.
                if (agreementId > 0)
                {
                    PayExAgreement agreement = _payExAgreementService.GetById(agreementId);
                    if (agreement != null && agreement.CustomerId == _workContext.CurrentCustomer.Id)
                        paymentInfo.PurchaseOrderNumber = agreement.AgreementRef;
                }
                else if (createAgreement)
                    paymentInfo.PurchaseOrderNumber = "new";
            }
#endif
            return paymentInfo;
        }

        public virtual ActionResult Complete(string orderRef)
        {
            int orderId;
            if (DoComplete(orderRef, out orderId))
                return RedirectToRoute("CheckoutCompleted", new { orderId = orderId });
            else
            {
                PaymentFailedModel model = new PaymentFailedModel() { OrderId = orderId };
                return View("~/Plugins/Payments.PayEx/Views/PaymentPayEx/PaymentFailed.cshtml", model);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public virtual ActionResult TransactionCallback(FormCollection form)
        {
            string statusCode = "FAILURE";
            //string transactionRef = form["transactionRef"];
            //string transactionNumber = form["transactionNumber"];
            string orderRef = form["orderRef"];
            if (!string.IsNullOrWhiteSpace(orderRef))
            {
                try
                {
                    int orderId;
                    DoComplete(orderRef, out orderId);
                    if (orderId > 0)
                        statusCode = "OK";
                }
                catch (Exception)
                {
                    // FAILURE
                }
            }

            return Content(statusCode);
        }

#if DEBUG
        [HttpGet]
        public virtual ActionResult TransactionCallbackGet(string orderRef)
        {
            // Used for debugging with GET.
            string statusCode = "FAILURE";
            if (!string.IsNullOrWhiteSpace(orderRef))
            {
                try
                {
                    int orderId;
                    DoComplete(orderRef, out orderId);
                    if (orderId > 0)
                        statusCode = "OK";
                }
                catch (Exception)
                {
                    // FAILURE
                }
            }

            return Content(statusCode);
        }
#endif

        public virtual ActionResult CancelOrder(FormCollection form)
        {
            string id = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(id))
            {
                //return RedirectToRoute("OrderDetails", new { orderId = id });
                int orderId;
                if (int.TryParse(id, out orderId))
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
                            // Does not restore gift cards until issue fixed: https://nopcommerce.codeplex.com/workitem/11436
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