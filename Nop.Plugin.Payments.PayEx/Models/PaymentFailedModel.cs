﻿using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.PayEx.Models
{
    public class PaymentFailedModel : BaseNopModel
    {
        public int OrderId { get; set; }
    }
}