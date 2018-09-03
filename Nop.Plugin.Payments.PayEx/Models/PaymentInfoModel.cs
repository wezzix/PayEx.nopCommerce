using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.PayEx.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public bool AllowCreateAgreement { get; set; }
        public int AgreementId { get; set; }
        public bool CreateAgreement { get; set; }
        public IList<SelectListItem> Agreements { get; set; }
    }
}