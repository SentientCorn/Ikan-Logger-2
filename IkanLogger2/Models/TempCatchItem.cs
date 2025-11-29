namespace IkanLogger2.Models
{
    public class TempCatchItem
    {
        public int FishId { get; set; }
        public string FishName { get; set; }
        public double MarketPrice { get; set; }
        public double Weight { get; set; }
        public double TotalPrice => Weight * MarketPrice;
    }
}