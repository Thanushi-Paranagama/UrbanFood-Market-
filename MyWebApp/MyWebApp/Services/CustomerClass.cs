
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

public class CustomerClass
{
    private readonly string _connectionString;
    private readonly ILogger<CustomerClass> _logger;

    public CustomerClass(string connectionString, ILogger<CustomerClass> logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task InitializeDatabase()
    {
        try
        {
            _logger?.LogInformation("[DEBUG] Initializing database...");

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();
                _logger?.LogInformation("[DEBUG] Database connection opened successfully.");

                // Check and create the Customers table
                string checkAndCreateTable = @"
                    DECLARE
                        v_count NUMBER;
                    BEGIN
                        SELECT COUNT(*) INTO v_count FROM user_tables WHERE table_name = 'CUSTOMERS';
                        IF v_count = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE TABLE Customers (
                                ID NUMBER PRIMARY KEY,
                                Name VARCHAR2(100),
                                ContactNumber VARCHAR2(15)
                            )';
                            DBMS_OUTPUT.PUT_LINE('Customers table created.');
                        END IF;
                    END;";

                using (OracleCommand command = new OracleCommand(checkAndCreateTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    _logger?.LogInformation("[DEBUG] Customers table check completed.");
                }

                // Check and create the sequence
                string checkAndCreateSequence = @"
                    DECLARE
                        v_count NUMBER;
                    BEGIN
                        SELECT COUNT(*) INTO v_count FROM user_sequences WHERE sequence_name = 'CUSTOMERS_SEQ';
                        IF v_count = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE SEQUENCE customers_seq START WITH 1 INCREMENT BY 1';
                            DBMS_OUTPUT.PUT_LINE('Customers sequence created.');
                        END IF;
                    END;";

                using (OracleCommand command = new OracleCommand(checkAndCreateSequence, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    _logger?.LogInformation("[DEBUG] Customers sequence check completed.");
                }

                // Check and create the trigger
                string checkAndCreateTrigger = @"
                    DECLARE
                        v_count NUMBER;
                    BEGIN
                        SELECT COUNT(*) INTO v_count FROM user_triggers WHERE trigger_name = 'CUSTOMERS_TRIGGER';
                        IF v_count = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE OR REPLACE TRIGGER customers_trigger
                            BEFORE INSERT ON Customers
                            FOR EACH ROW
                            BEGIN
                                SELECT customers_seq.NEXTVAL INTO :NEW.ID FROM dual;
                            END;';
                            DBMS_OUTPUT.PUT_LINE('Customers trigger created.');
                        END IF;
                    END;";

                using (OracleCommand command = new OracleCommand(checkAndCreateTrigger, connection))
                {
                    await command.ExecuteNonQueryAsync();
                    _logger?.LogInformation("[DEBUG] Customers trigger check completed.");
                }

                _logger?.LogInformation("[DEBUG] Database initialization completed successfully.");
            }
        }
         catch (Exception ex)
          {
           _logger?.LogError($"[ERROR] Database initialization failed: {ex.Message}");
          throw;
          }
    }
}
