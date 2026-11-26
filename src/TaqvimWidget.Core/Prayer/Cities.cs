using System.Collections.Generic;
using System.Linq;

namespace TaqvimWidget.Core.Prayer;

public sealed class City
{
    public City(string name, double latitude, double longitude)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Name { get; }
    public double Latitude { get; }
    public double Longitude { get; }
}

public static class Cities
{
    /// <summary>O'zbekiston vaqt mintaqasi (UTC+5, yozgi vaqt yo'q).</summary>
    public const double UtcOffsetHours = 5;

    public static readonly IReadOnlyList<City> All = new[]
    {
        new City("Toshkent", 41.2995, 69.2401),
        new City("Samarqand", 39.6542, 66.9597),
        new City("Buxoro", 39.7747, 64.4286),
        new City("Andijon", 40.7821, 72.3442),
        new City("Namangan", 41.0011, 71.6683),
        new City("Farg'ona", 40.3864, 71.7864),
        new City("Nukus", 42.4531, 59.6103),
        new City("Qarshi", 38.8606, 65.7891),
        new City("Termiz", 37.2242, 67.2783),
        new City("Urganch", 41.5503, 60.6313),
        new City("Jizzax", 40.1158, 67.8422),
        new City("Guliston", 40.4897, 68.7842),
        new City("Navoiy", 40.0844, 65.3792),
    };

    public static City Find(string? name) => All.FirstOrDefault(c => c.Name == name) ?? All[0];
}
