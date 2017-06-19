namespace Nop.Plugin.Payments.PayEx
{
    /// <summary>
    /// Represents PayEx payment processor transaction mode
    /// </summary>
    public enum TransactionMode : int
    {
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture= 2
    }
}
