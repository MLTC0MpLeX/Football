using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Football.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

 
namespace FootballAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerDBContext _context;
 
        private static readonly HttpClient _httpClient = new HttpClient();
 
        public PlayersController(PlayerDBContext context)
        {
            _context = context;
        }
 
        // GET: api/Players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }
 
        // GET: api/Players/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer(long id)
        {
            var player = await _context.Players.FindAsync(id);
 
            if (player == null)
            {
                player = await RetrievePlayerFromAPI(id);
 
                if(player == null)
                {
                    return NotFound();
                }
 
                //save the data in the DB to cache locally the retrieved data
                _context.Players.Add(player);
                await _context.SaveChangesAsync();
                
                player = await RetrievePlayerStatsFromAPI(player);
                
                
 
                return player;
            }
 
            return player;
        }
 
        // PUT: api/Players/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayer(long id, Player player)
        {
            if (id != player.Id)
            {
                return BadRequest();
            }
 
            _context.Entry(player).State = EntityState.Modified;
 
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
 
            return NoContent();
        }
 
        // POST: api/Players
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Player>> PostPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
 
            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }
 
        // DELETE: api/Players/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(long id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
 
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
 
            return NoContent();
        }
 
        private bool PlayerExists(long id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
 
        [HttpGet("{id}")]
        private async Task<Player> RetrievePlayerFromAPI(long id)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"http://api.football-data.org/v4/persons/{id}");
 
                request.Headers.Add("X-Auth-Token", "df9f64b5f9e440fd8b2c6865a1888ef0");
 
                using var response = await _httpClient.SendAsync(request);
 
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error retrieving player {response.StatusCode}");
                }
 
                var json = await response.Content.ReadAsStringAsync();
 
                PlayerDTO? playerData = JsonConvert.DeserializeObject<PlayerDTO>(json);
 
                if(playerData == null)
                {
                    Console.WriteLine("Failed to deserialize player data");
 
                    return null;
                }
 
                return new Player
                {
                    Id = id,
                    FirstName = playerData.FirstName,
                    LastName = playerData.LastName,
                    LastUpdated = playerData.LastUpdated,
                    Nationality = playerData.Nationality,
                    Position = playerData.Position
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception Occured {ex.Message}");
                return null;
            }
        }
        
        private async Task<Player> RetrievePlayerStatsFromAPI(Player player)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "http://api.football-data.org/v4/competitions/PL/scorers");
 
                request.Headers.Add("X-Auth-Token", "df9f64b5f9e440fd8b2c6865a1888ef0");
 
                using var response = await _httpClient.SendAsync(request);
 
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error retrieving player stats {response.StatusCode}");
                }
 
                var json = await response.Content.ReadAsStringAsync();
 
                var stats = JsonConvert.DeserializeObject<GoalsDTO>(json);
 
                if(stats == null)
                {
                    Console.WriteLine("Failed to deserialize player stats");
 
                    return player;
                }
                
                var playerStats = stats.Scorers.FirstOrDefault(s => s.Player.Id == player.Id);
 
                if(playerStats == null)
                {
                    Console.WriteLine("Player not found in stats");
 
                    return player;
                }
 
                player.Apps = playerStats.Appearances;
                player.GoalCount = playerStats.Goals;
 
                return player;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception Occured {ex.Message}");
                return player;
            }
        }
    }
}