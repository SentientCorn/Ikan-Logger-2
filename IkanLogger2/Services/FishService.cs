using IkanLogger2.Models;
using IkanLogger2.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IkanLogger2.Services
{
    public static class FishService
    {
        public static async Task<List<FishLocation>> GetFishLocationsAsync()
        {
            using var conn = await DatabaseService.GetOpenConnectionAsync();

            // QUERY BARU: Menggunakan JOIN ke tabel penghubung dan master_fish
            const string sql = @"SELECT * FROM get_fish_location()";

            using var cmd = new NpgsqlCommand(sql, conn);
            var result = new List<FishLocation>();

            FishLocation current = null;
            int lastLocation = -1;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int idLoc = Convert.ToInt32(reader["idlocation"]);

                // Logika Grouping: Jika lokasi berubah, buat objek lokasi baru
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

                // Jika ada data ikan (karena LEFT JOIN bisa null jika lokasi tidak ada ikannya)
                if (reader["idfish"] != DBNull.Value)
                {
                    current.Fishes.Add(new Fish
                    {
                        IdFish = Convert.ToInt32(reader["idfish"]),
                        FishName = reader["fishname"].ToString(),
                        MarketPrice = Convert.ToDouble(reader["marketprice"]),
                    });
                }
            }

            return result;
        }

        public static async Task<List<Fish>> GetAllFishAsync()
        {
            var fishes = new List<Fish>();

            using var conn = await DatabaseService.GetOpenConnectionAsync();
            const string sql = @"SELECT * FROM get_all_fish()";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                fishes.Add(new Fish
                {
                    IdFish = Convert.ToInt32(reader["idfish"]),
                    FishName = reader["fishname"].ToString(),
                    MarketPrice = Convert.ToDouble(reader["marketprice"])
                });
            }

            return fishes;
        }
    }
}