namespace MyWebApp.Models
{
    public class OrderItemViewModel
    {
        
            public int Id { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Total { get; set; }
       
    }
}
