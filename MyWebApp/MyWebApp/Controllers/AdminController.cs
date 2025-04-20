using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace MyWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly string _connectionString;
        private readonly DatabaseService1 _databaseService;

        public AdminController(
            ILogger<AdminController> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            DatabaseService1 databaseService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = configuration.GetConnectionString("OracleDb");
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }
        public IActionResult ViewPlaceOrder()
        {
            return View();
        }
      
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardViewModel = new AdminDashboardViewModel
                {
                    RecentOrders = await GetRecentOrders(5), // Get 5 most recent orders
                    ItemsCount = await GetItemsCount(),
                    OrdersToday = await GetOrdersCountToday(),
                    ActiveCustomers = await GetActiveCustomersCount(),
                    RevenueToday = await GetRevenueTodayAmount()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading admin dashboard: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load dashboard data. Please try again later.";
                return View(new AdminDashboardViewModel());
            }
        }

        private async Task<List<RecentOrderViewModel>> GetRecentOrders(int count)
        {
            var orders = new List<RecentOrderViewModel>();

            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        SELECT o.ID as OrderID, 
                               c.Name as CustomerName, 
                               o.OrderDate, 
                               (SELECT COUNT(*) FROM OrderItems oi WHERE oi.OrderID = o.ID) as ItemCount,
                               o.TotalAmount,
                               o.Status
                        FROM Orders o
                        JOIN Customers c ON o.CustomerID = c.ID
                        ORDER BY o.OrderDate DESC
                        FETCH FIRST :Count ROWS ONLY";

                    cmd.Parameters.Add(new OracleParameter("Count", count));

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new RecentOrderViewModel
                            {
                                OrderId = $"#ORD-{reader.GetInt32(0)}",
                                CustomerName = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                ItemCount = reader.GetInt32(3),
                                TotalAmount = reader.GetDecimal(4),
                                Status = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return orders;
        }


        private async Task<int> GetItemsCount()
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(*) FROM Items";

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        private async Task<int> GetOrdersCountToday()
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT COUNT(*) FROM Orders WHERE TRUNC(OrderDate) = TRUNC(SYSDATE)";

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        private async Task<int> GetActiveCustomersCount()
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        SELECT COUNT(DISTINCT CustomerID) 
                        FROM Orders 
                        WHERE OrderDate >= SYSDATE - 30";

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        private async Task<decimal> GetRevenueTodayAmount()
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        SELECT NVL(SUM(TotalAmount), 0)
                        FROM Orders
                        WHERE TRUNC(OrderDate) = TRUNC(SYSDATE)";

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToDecimal(result);
                }
            }
        }

        // If your view is Views/Admin/Index.cshtml for displaying farmer items
        public IActionResult Index()
        {
            try
            {
                List<ItemFarmer> farmerItems = _databaseService.GetAllItems();
                return View(farmerItems);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading farmer items: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load farmer items. Please try again later.";
                return View(new List<ItemFarmer>());
            }
        }

        // Get method for edit
        public IActionResult Edit(int id)
        {
            try
            {
                ItemFarmer item = _databaseService.GetItemById(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving farmer item for edit: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to retrieve item. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Post method for edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ItemFarmer item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _databaseService.UpdateItemInOracle(item);
                    TempData["SuccessMessage"] = "Item updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating farmer item: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error updating item: {ex.Message}";
                }
            }
            return View(item);
        }

        // If your view is Views/Admin/DeleteItem.cshtml
        public IActionResult DeleteItem(int id)
        {
            try
            {
                var item = _databaseService.GetItemById(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving farmer item: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to retrieve item. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItemConfirmed(int id)
        {
            try
            {
                _databaseService.DeleteItemFromOracle(id);
                TempData["SuccessMessage"] = "Item deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting farmer item: {ex.Message}");
                TempData["ErrorMessage"] = $"Error deleting item: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // If your view is Views/Admin/ViewDeleteLogs.cshtml
        public IActionResult ViewDeleteLogs()
        {
            try
            {
                List<FarmerItemDeleteLog> logs = _databaseService.GetDeleteLogs();
                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading deletion logs: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load deletion logs. Please try again later.";
                return View(new List<FarmerItemDeleteLog>());
            }
        }

    }

}