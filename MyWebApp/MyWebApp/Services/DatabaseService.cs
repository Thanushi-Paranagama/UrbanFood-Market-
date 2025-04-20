using MyWebApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MyWebApp.Controllers;

public class DatabaseService
{
    private readonly string oracleConnectionString;

    public DatabaseService(string connectionString)
    {
        oracleConnectionString = connectionString;
    }

    public List<Item> GetAllItems()
    {
        List<Item> items = new List<Item>();

        using (var conn = new OracleConnection(oracleConnectionString))
        {
            conn.Open();
            using (var cmd = new OracleCommand("SELECT Id, Category, ItemName, Quantity, Price FROM Items", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new Item
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Category = reader["Category"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }
        }

        return items;
    }

    public Item GetItemById(int id)
    {
        using (var conn = new OracleConnection(oracleConnectionString))
        {
            conn.Open();
            using (var cmd = new OracleCommand("SELECT Id, Category, ItemName, Quantity, Price FROM Items WHERE Id = :id", conn))
            {
                cmd.Parameters.Add(new OracleParameter("id", id));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Item
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Category = reader["Category"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Price = Convert.ToDecimal(reader["Price"])
                        };
                    }
                }
            }
        }

        return null;
    }

    public void SaveItemToOracle(Item item)
    {
        using (var conn = new OracleConnection(oracleConnectionString))
        {
            conn.Open();
            using (var cmd = new OracleCommand("INSERT INTO Items (Category, ItemName, Quantity, Price) VALUES (:category, :itemName, :quantity, :price)", conn))
            {
                cmd.Parameters.Add(new OracleParameter("category", item.Category));
                cmd.Parameters.Add(new OracleParameter("itemName", item.ItemName));
                cmd.Parameters.Add(new OracleParameter("quantity", item.Quantity));
                cmd.Parameters.Add(new OracleParameter("price", item.Price));
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void UpdateItemInOracle(Item item)
    {
        using (var conn = new OracleConnection(oracleConnectionString))
        {
            conn.Open();
            using (var cmd = new OracleCommand("UPDATE Items SET Category = :category, ItemName = :itemName, Quantity = :quantity, Price = :price WHERE Id = :id", conn))
            {
                cmd.Parameters.Add(new OracleParameter("category", item.Category));
                cmd.Parameters.Add(new OracleParameter("itemName", item.ItemName));
                cmd.Parameters.Add(new OracleParameter("quantity", item.Quantity));
                cmd.Parameters.Add(new OracleParameter("price", item.Price));
                cmd.Parameters.Add(new OracleParameter("id", item.Id));
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void DeleteItem(int id)
    {
        using (var conn = new OracleConnection(oracleConnectionString))
        {
            conn.Open();
            using (var cmd = new OracleCommand("DELETE FROM Items WHERE Id = :id", conn))
            {
                cmd.Parameters.Add(new OracleParameter("id", id));
                cmd.ExecuteNonQuery();
            }
        }
    }

    public async Task<MyWebApp.Models.Order> GetOrderById(int orderId)
    {
        using (var conn = new OracleConnection(oracleConnectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new OracleCommand("SELECT Id, CustomerId, OrderDate, TotalAmount, Status, PaymentDate FROM Orders WHERE Id = :orderId", conn))
            {
                cmd.Parameters.Add(new OracleParameter("orderId", orderId));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new MyWebApp.Models.Order
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CustomerId = Convert.ToInt32(reader["CustomerId"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                            PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : (DateTime?)null
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<List<MyWebApp.Models.OrderItemViewModel>> GetOrderItems(int orderId)
    {
        List<MyWebApp.Models.OrderItemViewModel> items = new List<MyWebApp.Models.OrderItemViewModel>();

        using (var conn = new OracleConnection(oracleConnectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new OracleCommand(
                "SELECT ItemId, ItemName, Category, UnitPrice, Quantity, TotalPrice FROM OrderItems WHERE OrderId = :orderId",
                conn))
            {
                cmd.Parameters.Add(new OracleParameter("orderId", orderId));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new MyWebApp.Models.OrderItemViewModel
                        {
                            Id = Convert.ToInt32(reader["ItemId"]),
                            ItemName = reader["ItemName"].ToString(),
                            Category = reader["Category"].ToString(),
                            UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Total = Convert.ToDecimal(reader["TotalPrice"])
                        });
                    }
                }
            }
        }

        return items;
    }
}
