using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;

class Program
{
    static void Main()
    {
        var config = LoadConfiguration();
        var connectionString = config.ConnectionString;
        var logFilePath = config.LogFilePath;

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var results = ExecuteStoredProcedureWithResults(connection, "FiltrarUsuariosPorEdad", new SqlParameter[]
                {
                    new SqlParameter("@Edad", 25)
                });

                File.WriteAllText(logFilePath, results);
                Console.WriteLine("Procedimientos ejecutados y resultados registrados en el archivo de log.");
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(logFilePath, $"Error: {ex.Message}\n");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static (string ConnectionString, string LogFilePath) LoadConfiguration()
    {
        var xml = XDocument.Load("config.xml");
        var connectionString = xml.Root.Element("ConnectionStrings")
            .Element("add").Attribute("connectionString").Value;
        var logFilePath = xml.Root.Element("LogFilePath").Value;

        return (connectionString, logFilePath);
    }

    static void ExecuteStoredProcedure(SqlConnection connection, string procedureName, SqlParameter[] parameters)
    {
        using (var command = new SqlCommand(procedureName, connection))
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }
    }

    static string ExecuteStoredProcedureWithResults(SqlConnection connection, string procedureName, SqlParameter[] parameters)
    {
        using (var command = new SqlCommand(procedureName, connection))
        {
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddRange(parameters);

            using (var reader = command.ExecuteReader())
            {
                var result = "";
                while (reader.Read())
                {
                    result += $"{reader["Nombre"]}, {reader["Apellido"]}, {reader["Edad"]}, {reader["Correo"]}, {reader["Hobbies"]}, {reader["Activo"]}\n";
                }
                return result;
            }
        }
    }
}
