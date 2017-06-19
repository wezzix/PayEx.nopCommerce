namespace SD.Payex2.Entities
{
    public class AddSingleOrderLineRequest
    {
        /// <summary>
        /// A reference to the initial order.
        /// </summary>
        public string OrderRef { get; set; }

        /// <summary>
        /// The number of the item.
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// A description of the item.
        /// </summary>
        public string ItemDescription1 { get; set; }

        public string ItemDescription2 { get; set; }
        public string ItemDescription3 { get; set; }
        public string ItemDescription4 { get; set; }
        public string ItemDescription5 { get; set; }

        /// <summary>
        /// Number of items.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Total amount including vat (if vat is supplied).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Vat amount.
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Vat percent for this item, in %.
        /// </summary>
        public decimal VatPercent { get; set; }
    }
}