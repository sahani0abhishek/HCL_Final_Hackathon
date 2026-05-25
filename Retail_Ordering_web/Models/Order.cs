namespace Retail_Ordering_web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";
    }
}