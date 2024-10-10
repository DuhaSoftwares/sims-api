namespace Duha.SIMS.ServiceModels.Invoice
{
    public class PurchaseDetailsSM 
    {
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int QuantityPurchased { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalPrice { get; set; }

        public decimal MoneyPaid { get; set; }
    }
}
