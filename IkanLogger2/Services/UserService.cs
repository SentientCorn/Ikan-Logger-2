using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using IkanLogger2.Models;

namespace IkanLogger2.Services
{
    public static class UserService
    {
        public static async Task<User> LoginAsync(string username, string password)
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            const string sql = @"select * from login_user(:_username, :_password)";
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("_username", username);
            cmd.Parameters.AddWithValue("_password", password);

            // This returns the user ID (or 0 / -1 if invalid)
            var result = await cmd.ExecuteScalarAsync();
            var userId = Convert.ToInt32(result);

            if (userId <= 0)
                return null;

            // Fetch the full profile
            const string profileSql = @"SELECT * FROM get_user_profile(:_id)";
            using var profileCmd = new NpgsqlCommand(profileSql, conn);
            profileCmd.Parameters.AddWithValue("_id", userId);

            using var reader = await profileCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User(
                    userId,
                    reader["username"].ToString()
                );
            }

            return null;
        }

        public static async Task<bool> RegisterAsync(string username, string password)
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            const string sql = @"select * from register_user(:_username, :_password)";
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("_username", username);
            cmd.Parameters.AddWithValue("_password", password);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) == 1;
        }

        public static async Task<User> GetProfileAsync(int id)
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            const string sql = @"SELECT * FROM get_user_profile(:_id)";
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("_id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User(
                    id,
                    reader["username"].ToString()
                );
            }


            return null;
        }
    }
}
