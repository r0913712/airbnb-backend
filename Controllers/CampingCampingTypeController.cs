using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
//this works
namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingCampingTypeController : ControllerBase
    {
        private readonly string _connectionString;

        public CampingCampingTypeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet("by-camping-type/{camping_type_id}")]
        public ActionResult<string> GetCampingIdbyCampingTypeId(int camping_type_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingcampingtype WHERE Camping_Type_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", camping_type_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new CampingCampingType
                                {
                                    Camping_Camping_Type_id = reader.GetInt32("Camping_Camping_Type_ID"),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Camping_Type_id = reader.GetInt32("Camping_Type_ID")
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


        [HttpGet("by-camping-id/{camping_id}")]
        public ActionResult<string> GetCampingTypeIdbyCampingId(int camping_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingcampingtype WHERE Camping_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", camping_id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new CampingCampingType
                                {
                                    Camping_Camping_Type_id = reader.GetInt32("Camping_Camping_Type_ID"),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Camping_Type_id = reader.GetInt32("Camping_Type_ID")
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

        [HttpPost("register")]
        public ActionResult NewCampingCampingType(
            [FromForm] int Camping_ID,
            [FromForm] int Camping_Type_ID)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO campingcampingtype (Camping_ID, Camping_Type_ID)
                        VALUES (@Camping_ID, @Camping_Type_ID)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Camping_ID", Camping_ID);
                        command.Parameters.AddWithValue("@Camping_Type_ID", Camping_Type_ID);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok("Camping to campingtype registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
