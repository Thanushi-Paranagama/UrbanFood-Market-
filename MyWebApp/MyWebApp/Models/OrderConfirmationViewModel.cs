using System;
using System.Collections.Generic;
using MyWebApp.Controllers;

namespace MyWebApp.Models
{
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContactNumber { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public decimal GrandTotal { get; set; }
    }
}