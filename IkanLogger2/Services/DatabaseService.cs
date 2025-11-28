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
                            "Timeout=15;" +
                            "CommandTimeout=15;" +
                            "Pooling=false;";

        public static async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn;
        }
    }

}
