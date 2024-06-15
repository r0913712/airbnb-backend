using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
//this works
namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingController : ControllerBase
    {
        private readonly string _connectionString;

        public CampingController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }
        [HttpGet("GetAllSpots")]
        public ActionResult<IEnumerable<Camping>> GetAllCamping()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM camping";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var camping = new List<Camping>();
                        while (reader.Read())
                        {
                            var description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description");

                            camping.Add(new Camping
                            {
                                Camping_id = reader.GetInt32("Camping_ID"),
                                Name = reader.GetString("Name"),
                                User_id = reader.GetInt32("User_ID"),
                                Location_id = reader.GetInt32("Location_ID"),
                                Description = description,
                                Price = reader.GetDouble("Price")
                            });
                        }
                        return Ok(camping);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{user_id}")]
        public ActionResult<List<Camping>> GetCampingByUserId(int user_id)
        {
            try
            {
                var campingSpots = new List<Camping>();

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM camping WHERE User_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", user_id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                campingSpots.Add(new Camping
                                {
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Name = reader.GetString("Name"),
                                    User_id = reader.GetInt32("User_ID"),
                                    Location_id = reader.GetInt32("Location_ID"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                    Price = reader.GetDouble("Price")
                                });
                            }
                        }
                    }
                }

                return Ok(campingSpots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetCampingbyID/{camping_id}")]
        public ActionResult<Camping> GetCampingByCampingId(int camping_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM camping WHERE Camping_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", camping_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new Camping
                                {
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Name = reader.GetString("Name"),
                                    User_id = reader.GetInt32("User_ID"),
                                    Location_id = reader.GetInt32("Location_ID"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString("Description"),
                                    Price = reader.GetDouble("Price")
                                });
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("New Camping")]
        public ActionResult NewCamping(
    [FromForm] string Name,
    [FromForm] int User_ID,
    [FromForm] int Location_ID,
    [FromForm] string? Description,
    [FromForm] double Price)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                INSERT INTO camping (Name, User_ID, Location_ID, Description, Price)
                VALUES (@Name, @User_ID, @Location_ID, @Description, @Price);
                SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@User_ID", User_ID);
                        command.Parameters.AddWithValue("@Location_ID", Location_ID);
                        command.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Price", Price);

                        var campingId = Convert.ToInt32(command.ExecuteScalar());

                        return Ok(new { Camping_ID = campingId });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("UpdateCampingSpot/{camping_id}")]
        public ActionResult UpdateCampingSpot(
    int camping_id,
    [FromForm] string Name,
    [FromForm] string? Description,
    [FromForm] double Price)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                UPDATE camping 
                SET Name = @Name, Description = @Description, Price = @Price
                WHERE Camping_ID = @Camping_ID";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Camping_ID", camping_id);
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Price", Price);

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok("Camping spot updated successfully.");
                        }
                        else
                        {
                            return NotFound("Camping spot not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
