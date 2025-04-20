using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace MyWebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly DatabaseService _databaseService;
        private readonly CustomerService _customerService;
        private readonly ILogger<OrderController> _logger;
        private readonly string _connectionString;

        public OrderController(
            DatabaseService databaseService,
            CustomerService customerService,
            ILogger<OrderController> logger,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        [HttpGet]
        public IActionResult GetAllItems()
        {
            try
            {
                var items = _databaseService.GetAllItems();
                return Json(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting items: {ex.Message}");
                return Json(new { success = false, message = "Failed to load items" });
            }
        }

        [HttpPost]
        public IActionResult SaveOrder([FromBody] OrderViewModel orderData)
        {
            try
            {
                _logger.LogInformation($"Order received for customer ID: {orderData.CustomerId} with {orderData.Items.Count} items");

                if (orderData == null || orderData.Items == null || orderData.Items.Count == 0)
                {
                    return Json(new { success = false, message = "No items in order" });
                }

                int orderId = SaveOrderToDatabase(orderData);
                if (orderId > 0)
                {
                    return Json(new { success = true, orderId = orderId });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save order" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving order: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int SaveOrderToDatabase(OrderViewModel orderData)
        {
            int orderId = 0;
            decimal orderTotal = 0;

            // Calculate order total
            foreach (var item in orderData.Items)
            {
                orderTotal += item.Total;
            }

            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Use a transaction to ensure all operations succeed or fail together
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {

                        // Call the save_order stored procedure
                        using (var cmd = new OracleCommand("save_order", conn))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            cmd.Parameters.Add(new OracleParameter("p_customer_id", OracleDbType.Int32)).Value = orderData.CustomerId;
                            cmd.Parameters.Add(new OracleParameter("p_total_amount", OracleDbType.Decimal)).Value = orderTotal;

                            // Output parameter for the new order ID
                            var orderIdParam = new OracleParameter("p_order_id", OracleDbType.Int32);
                            orderIdParam.Direction = System.Data.ParameterDirection.Output;
                            cmd.Parameters.Add(orderIdParam);

                            cmd.ExecuteNonQuery();
                            orderId = Convert.ToInt32(orderIdParam.Value.ToString());
                        }

                        // Add each order item
                        foreach (var item in orderData.Items)
                        {
                            using (var cmd = new OracleCommand("add_order_item", conn))
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                                cmd.Parameters.Add(new OracleParameter("p_order_id", OracleDbType.Int32)).Value = orderId;
                                cmd.Parameters.Add(new OracleParameter("p_item_id", OracleDbType.Int32)).Value = item.Id;
                                cmd.Parameters.Add(new OracleParameter("p_item_name", OracleDbType.Varchar2)).Value = item.ItemName;
                                cmd.Parameters.Add(new OracleParameter("p_category", OracleDbType.Varchar2)).Value = item.Category;
                                cmd.Parameters.Add(new OracleParameter("p_unit_price", OracleDbType.Decimal)).Value = item.UnitPrice;
                                cmd.Parameters.Add(new OracleParameter("p_quantity", OracleDbType.Int32)).Value = item.Quantity;
                                cmd.Parameters.Add(new OracleParameter("p_total_price", OracleDbType.Decimal)).Value = item.Total;

                                cmd.ExecuteNonQuery();
                            }
                        

                        
                        }

                        // Commit the transaction if all operations succeeded
                        transaction.Commit();
                        _logger.LogInformation($"Order saved successfully with ID: {orderId}");
                        return orderId;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any operation failed
                        transaction.Rollback();
                        _logger.LogError($"Error saving order: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        // Add these methods to your existing OrderController class

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentViewModel paymentData)
        {
            try
            {
                _logger.LogInformation($"Processing payment for order ID: {paymentData.OrderId}");

                if (paymentData == null)
                {
                    return Json(new { success = false, message = "No payment data received" });
                }

                // Save delivery and payment details to database
                bool success = await SaveDeliveryAndPaymentDetails(paymentData);

                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save payment details" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<bool> SaveDeliveryAndPaymentDetails(PaymentViewModel paymentData)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Use a transaction to ensure all operations succeed or fail together
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new OracleCommand("process_payment", conn))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            cmd.Parameters.Add(new OracleParameter("p_order_id", OracleDbType.Int32)).Value = paymentData.OrderId;
                            cmd.Parameters.Add(new OracleParameter("p_cardholder_name", OracleDbType.Varchar2)).Value = paymentData.CardholderName;

                            // Store only the last 4 digits of the card number for security
                            string cardLastFour = paymentData.CardNumber.Length >= 4
                                ? paymentData.CardNumber.Substring(paymentData.CardNumber.Length - 4)
                                : paymentData.CardNumber;

                            cmd.Parameters.Add(new OracleParameter("p_card_last_four", OracleDbType.Varchar2)).Value = cardLastFour;
                            cmd.Parameters.Add(new OracleParameter("p_amount", OracleDbType.Decimal)).Value = decimal.Parse(paymentData.TotalAmount);
                            cmd.Parameters.Add(new OracleParameter("p_address_line1", OracleDbType.Varchar2)).Value = paymentData.AddressLine1;
                            cmd.Parameters.Add(new OracleParameter("p_address_line2", OracleDbType.Varchar2)).Value = paymentData.AddressLine2 ?? string.Empty;
                            cmd.Parameters.Add(new OracleParameter("p_city", OracleDbType.Varchar2)).Value = paymentData.City;
                            cmd.Parameters.Add(new OracleParameter("p_state", OracleDbType.Varchar2)).Value = paymentData.State;
                            cmd.Parameters.Add(new OracleParameter("p_zip_code", OracleDbType.Varchar2)).Value = paymentData.ZipCode;
                            cmd.Parameters.Add(new OracleParameter("p_delivery_date", OracleDbType.Date)).Value = DateTime.Parse(paymentData.DeliveryDate);
                            cmd.Parameters.Add(new OracleParameter("p_delivery_time", OracleDbType.Varchar2)).Value = paymentData.DeliveryTime;
                            cmd.Parameters.Add(new OracleParameter("p_delivery_notes", OracleDbType.Varchar2)).Value = paymentData.DeliveryNotes ?? string.Empty;

                            await cmd.ExecuteNonQueryAsync();
                        }


                        // Commit the transaction if all operations succeeded
                        transaction.Commit();
                        _logger.LogInformation($"Payment details saved successfully for order ID: {paymentData.OrderId}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any operation failed
                        transaction.Rollback();
                        _logger.LogError($"Error saving payment details: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Add controller actions for success and failure pages
        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            return View("~/Views/Home/PaymentSuccess.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            try
            {
                // Get order details
                var order = await _databaseService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound();
                }

                // Get customer details
                var customer = await _customerService.GetCustomerById(order.CustomerId);
                if (customer == null)
                {
                    return NotFound();
                }

                // Get order items
                var items = await _databaseService.GetOrderItems(orderId);

                // Create view model
                var viewModel = new OrderConfirmationViewModel
                {
                    OrderId = orderId,
                    CustomerId = customer.ID,
                    CustomerName = customer.Name,
                    ContactNumber = customer.ContactNumber,
                    Items = items,
                    GrandTotal = items.Sum(i => i.Total)
                };

                // Fixed this line to pass the viewModel to the view
                return View("~/Views/Home/OrderConfirmation.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading order confirmation: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }
    }

    public class OrderViewModel
    {
        public int CustomerId { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

}