namespace MyWebApp.Models
{
    public class ItemFarmer
    {
        public int Id { get; set; } // ✅ Oracle ID
        public string Category { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
