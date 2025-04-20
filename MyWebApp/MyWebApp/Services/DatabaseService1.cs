using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using MyWebApp.Models;




    public class DatabaseService1
    {

    private readonly string oracleConnectionString;

    public DatabaseService1(string connectionString)
        {
        oracleConnectionString = connectionString;
    }

        public List<ItemFarmer> GetAllItems()
        {
            List<ItemFarmer> items = new List<ItemFarmer>();

            using (OracleConnection connection = new OracleConnection(oracleConnectionString))
            {
                connection.Open();
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Category, ItemName, Quantity, Price FROM FarmerItems ORDER BY Id";

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new ItemFarmer
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

        public ItemFarmer GetItemById(int id)
        {
            using (OracleConnection connection = new OracleConnection(oracleConnectionString))
            {
                connection.Open();
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Category, ItemName, Quantity, Price FROM FarmerItems WHERE Id = :id";
                    command.Parameters.Add(new OracleParameter("id", id));

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ItemFarmer
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

        public void SaveItemToOracle(ItemFarmer item)
        {
            using (OracleConnection connection = new OracleConnection(oracleConnectionString))
            {
                connection.Open();
                using (OracleCommand command = connection.CreateCommand())
                {

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "AddItem";

                command.Parameters.Add(new OracleParameter("p_Category", OracleDbType.Varchar2, item.Category, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_ItemName", OracleDbType.Varchar2, item.ItemName, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_Quantity", OracleDbType.Int32, item.Quantity, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_Price", OracleDbType.Decimal, item.Price, ParameterDirection.Input));

                command.ExecuteNonQuery();
            }
            }
        }

        public void UpdateItemInOracle(ItemFarmer item)
        {
            using (OracleConnection connection = new OracleConnection(oracleConnectionString))
            {
                connection.Open();
                using (OracleCommand command = connection.CreateCommand())
                {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "UpdateItem";

                command.Parameters.Add(new OracleParameter("p_Id", OracleDbType.Int32, item.Id, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_Category", OracleDbType.Varchar2, item.Category, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_ItemName", OracleDbType.Varchar2, item.ItemName, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_Quantity", OracleDbType.Int32, item.Quantity, ParameterDirection.Input));
                command.Parameters.Add(new OracleParameter("p_Price", OracleDbType.Decimal, item.Price, ParameterDirection.Input));

                command.ExecuteNonQuery();
                }
            }
        }
    public List<FarmerItemLog> GetItemLogs()
    {
        List<FarmerItemLog> logs = new List<FarmerItemLog>();

        using (OracleConnection connection = new OracleConnection(oracleConnectionString))
        {
            connection.Open();
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT LogID, ItemID, Category, ItemName, ActionDate FROM FarmerItems_Log ORDER BY ActionDate DESC";

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new FarmerItemLog
                        {
                            LogID = Convert.ToInt32(reader["LogID"]),
                            ItemID = Convert.ToInt32(reader["ItemID"]),
                            Category = reader["Category"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            ActionDate = Convert.ToDateTime(reader["ActionDate"])
                        });
                    }
                }
            }
        }

        return logs;
    }

    public void DeleteItemFromOracle(int id)
    {
        using (OracleConnection connection = new OracleConnection(oracleConnectionString))
        {
            connection.Open();
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "DeleteFarmerItem";

                command.Parameters.Add(new OracleParameter("p_Id", OracleDbType.Int32, id, ParameterDirection.Input));

                command.ExecuteNonQuery();
            }
        }
    }

    public List<FarmerItemDeleteLog> GetDeleteLogs()
    {
        List<FarmerItemDeleteLog> logs = new List<FarmerItemDeleteLog>();

        using (OracleConnection connection = new OracleConnection(oracleConnectionString))
        {
            connection.Open();
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT LogID, ItemID, Category, ItemName, DeletedDate, DeletedBy FROM FarmerItems_DeleteLog ORDER BY DeletedDate DESC";

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new FarmerItemDeleteLog
                        {
                            LogID = Convert.ToInt32(reader["LogID"]),
                            ItemID = Convert.ToInt32(reader["ItemID"]),
                            Category = reader["Category"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            DeletedDate = Convert.ToDateTime(reader["DeletedDate"]),
                            DeletedBy = reader["DeletedBy"].ToString()
                        });
                    }
                }
            }
        }

        return logs;
    }
}  
