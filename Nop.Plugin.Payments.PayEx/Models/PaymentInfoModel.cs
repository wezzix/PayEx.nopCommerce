using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

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