using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Controllers
{
    [Route("api/BienBao")]
    public class TrafficSignController : Controller
    {
        private readonly SqlHelper _sql;

        public TrafficSignController(SqlHelper sql)
        {
            _sql = sql;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrafficSigns()
        {
            try
            {
                var query = "SELECT TrafficSign.Id,TrafficSign.Name, ImageUrl, Description, Category.name as CategoryName FROM TrafficSign LEFT JOIN Category ON TrafficSign.CategoryId = Category.Id";

                var result = await _sql.ExecuteQueryAsync(query, Mapper.TrafficSignMapper.ToTrafficSignDto);

                return Ok(result);

            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error retrieving traffic signs: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving traffic signs.");

            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateTrafficSign([FromBody] List<TrafficSignCreateDto> dtos)
        {
            foreach (TrafficSignCreateDto dto in dtos)
            {
                string query = "INSERT INTO TrafficSign (ImageUrl, Name, Description, CategoryId) VALUES (@ImageUrl, @Name, @Description, @CategoryId)";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ImageUrl", dto.ImageUrl),
                    new SqlParameter("@Name", dto.Name),
                    new SqlParameter("@Description", dto.Description),
                    new SqlParameter("@CategoryId", dto.CategoryId)
                };

                try
                {
                    await _sql.ExecuteNonQueryAsync(query, parameters);
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use a logging framework here)
                    Console.WriteLine($"Error inserting traffic sign: {ex.Message}");
                    return StatusCode(500, "An error occurred while inserting the traffic sign.");
                }
            }
            return Created();
        }
        //create traffic sign with image file
        //[HttpPost]
        //public async Task<IActionResult> CreateTrafficSign([FromForm] TrafficSignCreateDto dto)
        //{

        //}


        [HttpDelete]
        public async Task<IActionResult> DeleteTrafficSign([FromBody] int id)
        {
            string query = "DELETE FROM TrafficSign WHERE Id = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };
            try
            {
                await _sql.ExecuteNonQueryAsync(query, parameters);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error deleting traffic sign: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the traffic sign.");
            }
        }

        // Update traffic sign (image file) change the content or image if nessecary
        //[HttpPut] 
        //public async Task<IActionResult> UpdateTrafficSign([FromBody] TrafficSignUpdateDto dto)
        //{
        //    string query = "UPDATE TrafficSign SET ImageUrl = @ImageUrl, Name = @Name, Description = @Description, CategoryId = @CategoryId WHERE Id = @Id";
        //    var parameters = new SqlParameter[]
        //    {
        //        new SqlParameter("@ImageUrl", dto.ImageUrl),
        //        new SqlParameter("@Name", dto.Name),
        //        new SqlParameter("@Description", dto.Description),
        //        new SqlParameter("@CategoryId", dto.CategoryId),
        //        new SqlParameter("@Id", dto.Id)
        //    };
        //    try
        //    {
        //        await _sql.ExecuteNonQueryAsync(query, parameters);
        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (you can use a logging framework here)
        //        Console.WriteLine($"Error updating traffic sign: {ex.Message}");
        //        return StatusCode(500, "An error occurred while updating the traffic sign.");
        //    }
        //}
    }
}