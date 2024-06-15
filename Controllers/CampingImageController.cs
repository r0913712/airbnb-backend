using C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace C_Sharp_Web_Programming_Final_Project_MySQL_Connection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampingImageController : ControllerBase
    {
        private readonly string _connectionString;

        public CampingImageController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        [HttpGet("{camping_id}")]
        public ActionResult<IEnumerable<CampingImage>> GetImagesByCampingID(int camping_id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingimage WHERE Camping_ID = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", camping_id);
                        using (var reader = command.ExecuteReader())
                        {
                            var images = new List<CampingImage>();
                            while (reader.Read())
                            {
                                images.Add(new CampingImage
                                {
                                    Camping_Image_id = reader.GetInt32("Camping_Image_ID"),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Camping_Image = (byte[])reader["Camping_Image"]
                                });
                            }
                            return Ok(images);
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
        public async Task<ActionResult> RegisterImage([FromForm] int Camping_ID, [FromForm] IFormFile? Image)
        {
            try
            {
                byte[] imageBytes;
                if (Image == null || Image.Length == 0)
                {
                    return BadRequest("No image uploaded.");
                }

                using (var ms = new MemoryStream())
                {
                    await Image.CopyToAsync(ms);
                    imageBytes = ms.ToArray();

                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var query = @"
                            INSERT INTO campingimage (Camping_ID, Camping_Image)
                            VALUES (@Camping_ID, @Camping_Image)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Camping_ID", Camping_ID);
                            command.Parameters.AddWithValue("@Camping_Image", imageBytes);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                return Ok("Image registered successfully.");
            }
            catch (Exception ex)
            {
                // Log the error (consider using a logging library like NLog or Serilog for production apps)
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("update/{camping_id}")]
        public ActionResult UpdateImages([FromForm] int camping_id, [FromForm] List<int> existingImageIds, [FromForm] List<IFormFile> newImages)
        {
            try
            {
                if (existingImageIds.Count + newImages.Count < 2 || existingImageIds.Count + newImages.Count > 10)
                {
                    return BadRequest("A camping spot must have at least 2 and at most 10 images.");
                }

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Delete images not in the existingImageIds
                    var deleteQuery = "DELETE FROM campingimage WHERE Camping_ID = @Camping_ID AND Camping_Image_ID NOT IN (@ExistingImageIds)";
                    using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@Camping_ID", camping_id);
                        deleteCommand.Parameters.AddWithValue("@ExistingImageIds", string.Join(",", existingImageIds));
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Add new images
                    foreach (var image in newImages)
                    {
                        if (image.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                image.CopyTo(ms);
                                var imageBytes = ms.ToArray();

                                var insertQuery = @"
                                    INSERT INTO campingimage (Camping_ID, Camping_Image)
                                    VALUES (@Camping_ID, @Camping_Image)";
                                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@Camping_ID", camping_id);
                                    insertCommand.Parameters.AddWithValue("@Camping_Image", imageBytes);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                return Ok("Images updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<IEnumerable<CampingImage>> GetAllImages()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM campingimage";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var images = new List<CampingImage>();
                            while (reader.Read())
                            {
                                images.Add(new CampingImage
                                {
                                    Camping_Image_id = reader.GetInt32("Camping_Image_ID"),
                                    Camping_id = reader.GetInt32("Camping_ID"),
                                    Camping_Image = (byte[])reader["Camping_Image"]
                                });
                            }
                            return Ok(images);
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
