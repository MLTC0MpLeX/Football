namespace Football.Models;

using Microsoft.EntityFrameworkCore;
public class PlayerDBContext: DbContext
{
    public PlayerDBContext(DbContextOptions<PlayerDBContext> options) : base(options) { }
    public required DbSet<Player> Players { get; set; }
}