using IkanLogger2.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkanLogger2.Services
{
    public static class LogService
    {
        public static async Task<List<CatchLog>> GetRecentLog(int userid)
        {
            var logs = new List<CatchLog>();
            
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            const string sql = @"
            SELECT logdate, notes, totalweight, totalprice 
            FROM catchlog 
            WHERE iduser = @uid
            ORDER BY logdate DESC 
            LIMIT 3
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userid);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) 
            {
                logs.Add(new CatchLog
                {
                    logdate = Convert.ToDateTime(reader["logdate"]),
                    notes = reader["notes"].ToString(),
                    totalweight = Convert.ToDouble(reader["totalweight"]),
                    totalprice = Convert.ToDouble(reader["totalprice"]),
                });
            }
            return logs;

        }
    }
}
