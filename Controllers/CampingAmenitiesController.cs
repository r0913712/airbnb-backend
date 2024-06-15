using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingAmenitiesController : ControllerBase
    {
        private readonly string _connectionString;

        public CampingAmenitiesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<Amenities>> GetAllAmenities()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM amenities";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var amenities = new List<Amenities>();
                        while (reader.Read())
                        {
                            amenities.Add(new Amenities
                            {
                                Amenities_id = reader.GetInt32("Amenities_ID"),
                                Amenity = reader.GetString("Amenity")
                            });
                        }
                        return Ok(amenities);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("campingid/{camping_id}")]
        public ActionResult<IEnumerable<Amenities>> GetAmenitiesByCampingID(int camping_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT a.Amenities_ID, a.Amenity 
                                  FROM amenities a
                                  JOIN campingamenities ca ON a.Amenities_ID = ca.Amenities_ID
                                  WHERE ca.Camping_ID = @camping_id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@camping_id", camping_id);
                        using (var reader = command.ExecuteReader())
                        {
                            var amenities = new List<Amenities>();
                            while (reader.Read())
                            {
                                amenities.Add(new Amenities
                                {
                                    Amenities_id = reader.GetInt32("Amenities_ID"),
                                    Amenity = reader.GetString("Amenity")
                                });
                            }
                            return Ok(amenities);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update/{camping_id}")]
        public ActionResult UpdateCampingAmenities(int camping_id, [FromForm] List<int> amenities)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Delete existing amenities
                    var deleteQuery = "DELETE FROM campingamenities WHERE Camping_ID = @camping_id";
                    using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@camping_id", camping_id);
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Insert new amenities
                    foreach (var amenityId in amenities)
                    {
                        var insertQuery = "INSERT INTO campingamenities (Camping_ID, Amenities_ID) VALUES (@camping_id, @amenity_id)";
                        using (var insertCommand = new MySqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@camping_id", camping_id);
                            insertCommand.Parameters.AddWithValue("@amenity_id", amenityId);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                return Ok("Amenities updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("register")]
        public ActionResult RegisterCampingAmenity([FromForm] int Camping_ID, [FromForm] int Amenities_ID)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var insertQuery = "INSERT INTO campingamenities (Camping_ID, Amenities_ID) VALUES (@camping_id, @amenity_id)";
                    using (var command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@camping_id", Camping_ID);
                        command.Parameters.AddWithValue("@amenity_id", Amenities_ID);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok("Amenity registered successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
