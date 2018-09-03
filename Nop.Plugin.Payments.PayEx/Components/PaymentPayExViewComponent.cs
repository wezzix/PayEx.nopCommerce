using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.PayEx.Controllers;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Models;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayEx.Components
{
    [ViewComponent(Name = "PaymentPayEx")]
    public class PaymentPayExViewComponent : NopViewComponent
    {
        private readonly IPayExAgreementService _payExAgreementService;
        private readonly PayExPaymentSettings _payExPaymentSettings;
        private readonly IWorkContext _workContext;

        public PaymentPayExViewComponent(
            IWorkContext workContext,
            PayExPaymentSettings payExPaymentSettings,
            IPayExAgreementService payExAgreementService)
        {
            _workContext = workContext;
            _payExPaymentSettings = payExPaymentSettings;
            _payExAgreementService = payExAgreementService;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            if (!_workContext.CurrentCustomer.IsGuest())
            {
                IEnumerable<PayExAgreement> agreements = _payExAgreementService
                    .GetValidAgreements(_workContext.CurrentCustomer.Id, PaymentPayExController.PaymentSystemName)
                    .OrderByDescending(o => o.LastUsedDate);
                model.Agreements = agreements.Select(
                    o => new SelectListItem
                    {
                        Text = $"{o.PaymentMethod} {o.Name}",
                        Value = o.Id.ToString()
                    }).ToList();
                model.AllowCreateAgreement = _payExPaymentSettings.AllowCreateAgreement;
            }

            return View("~/Plugins/Payments.PayEx/Views/PaymentInfo.cshtml", model);
        }
    }
}