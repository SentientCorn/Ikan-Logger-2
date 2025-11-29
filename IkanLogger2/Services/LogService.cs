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
            SELECT logdate, notes, totalweight, totalprice, latitude, longitude
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
                    latitude = Convert.ToDouble(reader["latitude"]),
                    longitude = Convert.ToDouble(reader["longitude"])
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
                cl.latitude,
                cl.longitude,
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
                        latitude = reader["latitude"] is DBNull ? 0 : Convert.ToDouble(reader["latitude"]),
                        longitude = reader["longitude"] is DBNull ? 0 : Convert.ToDouble(reader["longitude"]),
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

        // Replace the CreateCatchLog method in LogService.cs with this async version:

        public static async Task<bool> CreateCatchLogAsync(int userid, string notes, double lat, double lng, List<FishCatchInput> catches)
        {
            NpgsqlConnection conn = null;
            NpgsqlTransaction transaction = null;

            try
            {
                conn = await DatabaseService.GetOpenConnectionAsync();
                transaction = await conn.BeginTransactionAsync();

                // Calculate totals
                double totalWeight = catches.Sum(c => c.Weight);
                double totalPrice = catches.Sum(c => c.SalePrice);

                // Insert CatchLog
                const string logSql = @"
                    INSERT INTO CatchLog (logdate, notes, iduser, totalweight, totalprice, latitude, longitude)
                    VALUES (@LogDate, @Notes, @UserId, @TotalWeight, @TotalPrice, @Latitude, @Longitude)
                    RETURNING idlog";


                int logId;
                using (var logCmd = new NpgsqlCommand(logSql, conn, transaction))
                {
                    logCmd.Parameters.AddWithValue("@LogDate", DateTime.Now);
                    logCmd.Parameters.AddWithValue("@Notes", notes ?? "");
                    logCmd.Parameters.AddWithValue("@UserId", userid);
                    logCmd.Parameters.AddWithValue("@TotalWeight", totalWeight);
                    logCmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
                    logCmd.Parameters.AddWithValue("@Latitude", lat);
                    logCmd.Parameters.AddWithValue("@Longitude", lng);


                    logId = Convert.ToInt32(await logCmd.ExecuteScalarAsync());
                }

                // Insert FishCatch entries

                if (catches.Count > 0)
                {
                    var sqlBuilder = new StringBuilder();
                    sqlBuilder.Append("INSERT INTO FishCatch (weight, fishid, idlog, saleprice) VALUES ");

                    var valuesList = new List<string>();

                    await using (var catchCmd = new NpgsqlCommand())
                    {
                        catchCmd.Connection = conn;
                        catchCmd.Transaction = transaction;

                        for (int i = 0; i < catches.Count; i++)
                        {
                            var catchItem = catches[i];
                            valuesList.Add($"(@Weight{i}, @FishId{i}, @LogId{i}, @SalePrice{i})");

                            catchCmd.Parameters.AddWithValue($"@Weight{i}", catchItem.Weight);
                            catchCmd.Parameters.AddWithValue($"@FishId{i}", catchItem.FishId);
                            catchCmd.Parameters.AddWithValue($"@LogId{i}", logId);
                            catchCmd.Parameters.AddWithValue($"@SalePrice{i}", catchItem.SalePrice);
                        }

                        sqlBuilder.Append(string.Join(", ", valuesList));
                        catchCmd.CommandText = sqlBuilder.ToString();

                        await catchCmd.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Safely rollback transaction
                if (transaction != null)
                {
                    try
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore rollback errors - connection may already be broken
                    }
                }

                throw new Exception($"Error creating catch log: {ex.Message}", ex);
            }
            finally
            {
                // Ensure connection is properly closed
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
                if (conn != null)
                {
                    try
                    {
                        // Make sure connection is closed before disposing
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            await conn.CloseAsync();
                        }
                        await conn.DisposeAsync();
                    }
                    catch { }
                }
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
