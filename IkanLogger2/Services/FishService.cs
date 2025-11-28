using IkanLogger.Services;
using IkanLogger2.Models;
using IkanLogger2.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IkanLogger.Services
{
    public static class FishService
    {
        public static async Task<List<FishLocation>> GetFishLocationsAsync()
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            const string sql = @"
                SELECT 
                    l.idlocation,
                    l.latitude,
                    l.longitude,
                    f.idfish,
                    f.fishname,
                    f.marketprice
                FROM location l
                LEFT JOIN fish f ON f.idloc = l.idlocation
                ORDER BY l.idlocation;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);

            var result = new List<FishLocation>();
            FishLocation current = null;
            int lastLocation = -1;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int idLoc = Convert.ToInt32(reader["idlocation"]);

                // Jika lokasi baru → buat entri baru
                if (idLoc != lastLocation)
                {
                    current = new FishLocation
                    {
                        IdLocation = idLoc,
                        Latitude = Convert.ToDouble(reader["latitude"]),
                        Longitude = Convert.ToDouble(reader["longitude"]),
                        Fishes = new List<Fish>()
                    };

                    result.Add(current);
                    lastLocation = idLoc;
                }

                // Jika ada data ikan
                if (reader["idfish"] != DBNull.Value)
                {
                    current.Fishes.Add(new Fish
                    {
                        IdFish = Convert.ToInt32(reader["idfish"]),
                        FishName = reader["fishname"].ToString(),
                        MarketPrice = Convert.ToDouble(reader["marketprice"]),
                        IdLocation = idLoc
                    });
                }
            }

            return result;
        }
    }
}
