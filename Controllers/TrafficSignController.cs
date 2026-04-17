using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Middleware;
using ApiThiBangLaiXeOto.Utils;
using Microsoft.AspNetCore.Authorization;
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
                var query = "SELECT TrafficSign.Id,TrafficSign.Name, ImageUrl, Description, Category.name as CategoryName, Category.id as CategoryId FROM TrafficSign LEFT JOIN Category ON TrafficSign.CategoryId = Category.Id";

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
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [Route("Json")]
        [HttpPost]
        public async Task<IActionResult> CreateTrafficSign([FromBody] List<TrafficSignCreateDto> dtos)
        {
            foreach (TrafficSignCreateDto dto in dtos)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

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
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [Route("Form")]
        [HttpPost]
        public async Task<IActionResult> CreateTrafficSign([FromForm] TrafficSignCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string fileName = String.Empty;
            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        if (dto.Image != null)
                        {
                            fileName = await ImageUtils.UploadImage(dto.Image);
                        }

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ImageUrl", fileName),
                            new SqlParameter("@Name", dto.Name),
                            new SqlParameter("@Description", dto.Description),
                            new SqlParameter("@CategoryId", dto.CategoryId)
                        };

                        string query = "INSERT INTO TrafficSign (ImageUrl, Name, Description, CategoryId) VALUES (@ImageUrl, @Name, @Description, @CategoryId)";

                        await _sql.ExecuteNonQueryAsync(query, connection, trans, parameters);
                        await trans.CommitAsync();
                        return Created();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            ImageUtils.DeleteImage(fileName);
                        }
                        return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
                    }
                }
            }


        }

        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrafficSign(int id)
        {
            var userid =  authHelper.GetUser(User, _sql).Id;

            string getImageQuery = "SELECT ImageUrl FROM TrafficSign WHERE Id = @Id";
            string oldFileName = string.Empty;

            var oldDataParameters = new SqlParameter[]
{
                new SqlParameter("@Id", id)
};

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };

            var oldData = await _sql.ExecuteScalarAsync<string>(getImageQuery, oldDataParameters);
            if (oldData != null)
            {
                oldFileName = oldData;
            }


            string query = "DELETE FROM TrafficSign WHERE Id = @Id";

            try
            {
                await _sql.ExecuteNonQueryAsync(query, parameters);
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    ImageUtils.DeleteImage(oldFileName);
                }
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
        //        [HttpPut]
        //        public async Task<IActionResult> UpdateTrafficSign([FromBody] TrafficSignUpdateDto dto)
        //        {
        //            string query = String.Empty;
        //            string fileName = String.Empty;
        //            var parameters = new SqlParameter[]
        //{
        //                new SqlParameter("@Name", dto.Name),
        //                new SqlParameter("@Description", dto.Description),
        //                new SqlParameter("@CategoryId", dto.CategoryId),
        //                new SqlParameter("@Id", dto.Id)
        //};
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }

        //            if(dto.RemoveImage == false)
        //            {   
        //                    query = "UPDATE TrafficSign SET Name = @Name, Description = @Description, CategoryId = @CategoryId WHERE Id = @Id";
        //            }else
        //            {
        //                query = "UPDATE TrafficSign SET ImageUrl = @ImageUrl, Name = @Name, Description = @Description, CategoryId = @CategoryId WHERE Id = @Id";
        //                if (dto.Image != null)
        //                {
        //                    fileName = await ImageUtils.UploadImage(dto.Image);
        //                }
        //                else
        //                {
        //                    return BadRequest("Image file is required when RemoveImage is true.");
        //                }
        //                parameters = parameters.Append(new SqlParameter("@ImageUrl", fileName)).ToArray();
        //            }

        //            try
        //            {
        //                await _sql.ExecuteNonQueryAsync(query, parameters);
        //                return NoContent();
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log the exception (you can use a logging framework here)
        //                Console.WriteLine($"Error updating traffic sign: {ex.Message}");
        //                return StatusCode(500, "An error occurred while updating the traffic sign.");
        //            }
        //        }

        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrafficSign([FromForm] TrafficSignUpdateDto dto, int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string newFileName = string.Empty;
            string oldFileName = string.Empty;

            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Lấy thông tin biển báo hiện tại để biết tên file cũ
                        string getOldQuery = "SELECT ImageUrl FROM TrafficSign WHERE Id = @Id";
                        var getOldParams = new List<SqlParameter> { new SqlParameter("@Id", id) };

                        var oldData = await _sql.ExecuteScalarAsync<string>(getOldQuery, getOldParams.ToArray());

                        if(oldData != null) {
                            oldFileName = oldData;
                        }

                        // 2. Chuẩn bị query và parameters
                        string query;
                        var parameters = new List<SqlParameter>
                        {
                            new SqlParameter("@Name", dto.Name),
                            new SqlParameter("@Description", (object)dto.Description ?? DBNull.Value),
                            new SqlParameter("@CategoryId", dto.CategoryId),
                            new SqlParameter("@Id", id)
                        };

                        // 3. Xử lý ảnh nếu có thay đổi
                        if (dto.RemoveImage)
                        {
                            if (dto.Image == null) return BadRequest("Phải chọn ảnh mới khi RemoveImage là true.");

                            newFileName = await ImageUtils.UploadImage(dto.Image);
                            parameters.Add(new SqlParameter("@ImageUrl", newFileName));
                            query = "UPDATE TrafficSign SET ImageUrl = @ImageUrl, Name = @Name, Description = @Description, CategoryId = @CategoryId WHERE Id = @Id";
                        }
                        else
                        {
                            query = "UPDATE TrafficSign SET Name = @Name, Description = @Description, CategoryId = @CategoryId WHERE Id = @Id";
                        }

                        // 4. Thực thi Update
                        await _sql.ExecuteNonQueryAsync(query, connection, (SqlTransaction)trans, parameters.ToArray());

                        // 5. Commit Transaction
                        await trans.CommitAsync();

                        // 6. CHỈ XÓA ẢNH CŨ KHI MỌI THỨ ĐÃ THÀNH CÔNG
                        if (dto.RemoveImage)
                        {
                            try
                            {
                                ImageUtils.DeleteImage(oldFileName);
                            }
                            catch (Exception ex)
                            {
                                // Log the exception (you can use a logging framework here)
                                Console.WriteLine($"Error deleting old image: {ex.Message}");
                            }
                        }

                        return NoContent();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        // Nếu DB lỗi thì xóa file mới vừa upload (nếu có) để tránh rác
                        if (!string.IsNullOrEmpty(newFileName)) ImageUtils.DeleteImage(newFileName);

                        return StatusCode(500, $"Lỗi cập nhật: {ex.Message}");
                    }
                }
            }
        }

    }
}