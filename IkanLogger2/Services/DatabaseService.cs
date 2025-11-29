using Npgsql;

namespace IkanLogger2.Services
{
    public static class DatabaseService
    {
        private static readonly string connString =
                            "Host=aws-1-ap-southeast-1.pooler.supabase.com;" +
                            "Port=6543;" +
                            "Username=postgres.ihgmwmzlforobjkjxxqg;" +
                            "Password=HLSvizxbIpMIKSrZ;" +
                            "Database=postgres;" +
                            "SSL Mode=Require;" +
                            "Trust Server Certificate=true;" +
                            "Timeout=30;" +
                            "CommandTimeout=30;" +
                            "Pooling=false;" +           // Enable pooling
                            "MinPoolSize=0;" +          // Minimum connections
                            "MaxPoolSize=100;";         // Maximum connections

        public static async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            try
            {
                var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to database: {ex.Message}", ex);
            }
        }

        public static NpgsqlConnection GetOpenConnection()
        {
            try
            {
                var conn = new NpgsqlConnection(connString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to database: {ex.Message}", ex);
            }
        }

        public static string GetConnectionString()
        {
            return connString;
        }
    }

}
