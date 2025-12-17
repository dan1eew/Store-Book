using System.Data;
using System.Data.SqlClient;

namespace Store_Book.Data
{
    internal class DataHelper
    {
        //private static string ConnectionString = "Data Source=(LocalDB)\\132;AttachDbFilename=|DataDirectory|\\BookStoreDB132.mdf;Integrated Security=True"; // перенести бд в папку Data

        private static string ConnectionString = "Server=DanteewPC\\EQS_DB_HOME42;Database=BookStoreDB130;Trusted_Connection=True;TrustServerCertificate=True;"; // тест с бд 
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    DataTable dataTable = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    return dataTable;
                }
            }
        }
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }
    }
}