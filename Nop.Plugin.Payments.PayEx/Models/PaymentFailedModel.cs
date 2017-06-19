using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.PayEx.Models
{
    public class PaymentFailedModel : BaseNopModel
    {
        public int OrderId { get; set; }
    }
}