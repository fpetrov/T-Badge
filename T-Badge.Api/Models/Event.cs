using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace T_Badge.Models;

public class Event
{
    public int Id { get; set; }
    public int Code { get; private set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public double Rating { get; set; }
    [JsonIgnore]
    public User Author { get; set; }
    [JsonIgnore]
    public List<User> VisitedBy { get; set; } = [];
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public Event()
    {
        Code = RandomNumberGenerator.GetInt32(100_000, 1_000_000);
    }
}