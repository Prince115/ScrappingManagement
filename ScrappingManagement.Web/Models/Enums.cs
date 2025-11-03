namespace ScrappingManagement.Web.Models
{
    public enum PaymentMode
    {
        Cash,
        BankTransfer,
        Cheque,
        GPay
    }

    public enum QuoteStatus
    {
        ReportPending,
        PaymentPending,
        NeedToCheck,
        Completed
    }
}
