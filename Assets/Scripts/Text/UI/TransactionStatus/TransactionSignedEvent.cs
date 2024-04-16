namespace Pepemon.UI.TransactionStatus
{
    struct TransactionSignedEvent
    {
        public string Hash { get; set; }
        public bool Success { get; set; }
    }
}
