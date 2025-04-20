namespace MyWebApp.Models
{
    public class PaymentViewModel
    {
        // Order details
        public int OrderId { get; set; }
        public string TotalAmount { get; set; }

        // Delivery details
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryNotes { get; set; }

        // Payment details
        public string CardholderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CVV { get; set; }
    }
}
