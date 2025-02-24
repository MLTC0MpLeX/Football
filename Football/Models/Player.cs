namespace Football.Models;

public class Player
{
    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? LastUpdated { get; set; }
    public string? Nationality { get; set; }
    public string? Position { get; set; }
    public int PlayerMatched { get; set; }
    public int GoalCount { get; set; }
}