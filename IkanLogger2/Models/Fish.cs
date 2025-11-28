public class FishLocation
{
    public int IdLocation { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<Fish> Fishes { get; set; } = new List<Fish>();
}

public class Fish
{
    public int IdFish { get; set; }
    public string FishName { get; set; }
    public double MarketPrice { get; set; }
    public int IdLocation { get; set; }
}
