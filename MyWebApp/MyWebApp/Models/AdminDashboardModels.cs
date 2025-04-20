using System;
using System.Collections.Generic;

namespace MyWebApp.Models
{
    public class AdminDashboardViewModel
    {
        public int ItemsCount { get; set; }
        public int OrdersToday { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal RevenueToday { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    }

    public class RecentOrderViewModel
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public string FormattedDate => OrderDate.ToString("MMM d, yyyy");
        public string StatusBadgeClass
        {
            get
            {
                return Status.ToLower() switch
                {
                    "completed" => "badge-success",
                    "processing" => "badge-warning",
                    "paid" => "badge-warning",
                    "shipped" => "badge-success",
                    "cancelled" => "badge-danger",
                    _ => "badge-warning"
                };
            }
        }
    }
}