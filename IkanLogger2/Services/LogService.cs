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

        public static async Task<List<CatchLogDetail>> GetAllLogs(int userid)
        {
            var logsDict = new Dictionary<int, CatchLogDetail>();

            using var conn = await DatabaseService.GetOpenConnectionAsync();

            // Query dengan JOIN untuk ambil semua data sekaligus
            const string sql = @"
            SELECT 
                cl.idlog,
                cl.logdate,
                cl.notes,
                cl.totalweight,
                cl.totalprice,
                fc.idfishcatch,
                fc.weight,
                fc.saleprice,
                f.fishname
            FROM CatchLog cl
            LEFT JOIN FishCatch fc ON cl.idlog = fc.idlog
            LEFT JOIN Fish f ON fc.fishid = f.idfish
            WHERE cl.iduser = @UserId
            ORDER BY cl.logdate DESC, f.fishname";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userid);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int logId = Convert.ToInt32(reader["idlog"]);

                // Cek apakah log sudah ada di dictionary
                if (!logsDict.ContainsKey(logId))
                {
                    logsDict[logId] = new CatchLogDetail
                    {
                        idlog = logId,
                        logdate = Convert.ToDateTime(reader["logdate"]),
                        notes = reader["notes"].ToString(),
                        totalweight = Convert.ToDouble(reader["totalweight"]),
                        totalprice = Convert.ToDouble(reader["totalprice"]),
                        Catches = new List<FishCatchDetail>()
                    };
                }

                // Tambahkan fish catch jika ada (LEFT JOIN bisa return null)
                if (!reader.IsDBNull(reader.GetOrdinal("idfishcatch")))
                {
                    logsDict[logId].Catches.Add(new FishCatchDetail
                    {
                        idfishcatch = Convert.ToInt32(reader["idfishcatch"]),
                        fishname = reader["fishname"].ToString(),
                        weight = Convert.ToDouble(reader["weight"]),
                        saleprice = Convert.ToDouble(reader["saleprice"])
                    });
                }
            }

            return logsDict.Values.ToList();
        }

        public static async Task<bool> CreateCatchLog(int userid, string notes, List<FishCatchInput> catches)
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Hitung total weight dan total price
                double totalWeight = catches.Sum(c => c.Weight);
                double totalPrice = catches.Sum(c => c.SalePrice);

                // Insert CatchLog
                const string logSql = @"
                INSERT INTO CatchLog (logdate, notes, iduser, totalweight, totalprice)
                VALUES (@LogDate, @Notes, @UserId, @TotalWeight, @TotalPrice)
                RETURNING idlog";

                int logId;
                using (var logCmd = new NpgsqlCommand(logSql, conn, transaction))
                {
                    logCmd.Parameters.AddWithValue("@LogDate", DateTime.Now);
                    logCmd.Parameters.AddWithValue("@Notes", notes ?? "");
                    logCmd.Parameters.AddWithValue("@UserId", userid);
                    logCmd.Parameters.AddWithValue("@TotalWeight", totalWeight);
                    logCmd.Parameters.AddWithValue("@TotalPrice", totalPrice);

                    logId = Convert.ToInt32(await logCmd.ExecuteScalarAsync());
                }

                // Insert FishCatch untuk setiap ikan
                const string catchSql = @"
                INSERT INTO FishCatch (weight, fishid, idlog, saleprice)
                VALUES (@Weight, @FishId, @LogId, @SalePrice)";

                foreach (var catchItem in catches)
                {
                    using var catchCmd = new NpgsqlCommand(catchSql, conn, transaction);
                    catchCmd.Parameters.AddWithValue("@Weight", catchItem.Weight);
                    catchCmd.Parameters.AddWithValue("@FishId", catchItem.FishId);
                    catchCmd.Parameters.AddWithValue("@LogId", logId);
                    catchCmd.Parameters.AddWithValue("@SalePrice", catchItem.SalePrice);

                    await catchCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error creating catch log: {ex.Message}");
            }
        }

        // Model untuk input
        public class FishCatchInput
        {
            public int FishId { get; set; }
            public double Weight { get; set; }
            public double SalePrice { get; set; }
        }
    }
}
