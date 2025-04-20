using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

public class CustomerService
{
    private readonly string _connectionString;
    private readonly ILogger<CustomerService> _logger; // Logger for debugging

    public CustomerService(string connectionString, ILogger<CustomerService> logger)
    {
        _connectionString = connectionString;
        _logger = logger; // Initialize logger
    }

    public async Task<bool> SaveCustomerAsync(Customer customer)
    {
        try
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();
                _logger.LogInformation("[DEBUG] Database connection opened successfully.");

                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Customers (ID, Name, ContactNumber) VALUES (customers_seq.NEXTVAL, :Name, :ContactNumber)";
                    cmd.Parameters.Add(new OracleParameter("Name", OracleDbType.Varchar2)).Value = customer.Name;
                    cmd.Parameters.Add(new OracleParameter("ContactNumber", OracleDbType.Varchar2)).Value = customer.ContactNumber;

                    int result = await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"[DEBUG] Query executed. Rows affected: {result}");

                    return result > 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR] Database error: {ex.Message}");
            return false;
        }
    }
    public async Task<Customer> GetCustomerByNameAndNumberAsync(string name, string contactNumber)
    {
        try
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ID, Name, ContactNumber FROM Customers WHERE Name = :Name AND ContactNumber = :ContactNumber";
                    cmd.Parameters.Add(new OracleParameter("Name", OracleDbType.Varchar2)).Value = name;
                    cmd.Parameters.Add(new OracleParameter("ContactNumber", OracleDbType.Varchar2)).Value = contactNumber;

                    using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Customer
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ContactNumber = reader.GetString(2)
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR] Database error: {ex.Message}");
        }
        return null;
    }
    public async Task<Customer> GetCustomerById(int customerId)
    {
        try
        {
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ID, Name, ContactNumber FROM Customers WHERE ID = :CustomerId";
                    cmd.Parameters.Add(new OracleParameter("CustomerId", OracleDbType.Int32)).Value = customerId;

                    using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Customer
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ContactNumber = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR] Database error: {ex.Message}");
            return null;
        }
    }

}
