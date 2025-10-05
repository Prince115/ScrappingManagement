namespace ScrappingManagement.Web.Models
{
    public enum PaymentMode
    {
        Cash,
        BankTransfer,
        Cheque,
        Online
    }

    public enum QuoteStatus
    {
        ReportPending,
        PaymentPending,
        NeedToCheck,
        Completed
    }
}
