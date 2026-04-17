using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/chuong")]
    public class CategoryController : Controller
    {
        private readonly SqlHelper _sql;
        public CategoryController(SqlHelper sql)
        {
            _sql = sql;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] string? PhanLoai)
        {
            int parameter;
            if (PhanLoai == "bienbao")
            {
                parameter = 2;
            }
            else
            {
                parameter = 1;
            }

            string query = "SELECT Id, Name FROM Category Where Type = @PhanLoai";
            var parameters = new[] { new SqlParameter("@PhanLoai", parameter) };
            var categories = await _sql.ExecuteQueryAsync(query, CategoryMapper.ToCategoryDto, parameters);
            return Ok(categories);

        }

        //Them Xoa sua
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            string query = "INSERT INTO Category (Name, Type) VALUES (@Name, @Type)";
            var parameters = new[] {
                new SqlParameter("@Name", categoryDto.CategoryName),
                new SqlParameter("@Type", 1)
            };
            try
            {
                await _sql.ExecuteNonQueryAsync(query, parameters);
                return Ok(new { message = "Danh mục đã được tạo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo danh mục", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            string query = "UPDATE Category SET Name = @Name WHERE Id = @Id";
            var parameters = new[] {
                    new SqlParameter("@Name", categoryDto.CategoryName)
                };
            try
            {
                await _sql.ExecuteNonQueryAsync(query, parameters);
                return Ok(new { message = "Danh mục đã được cập nhật" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật danh mục", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            string query = "DELETE FROM Category WHERE Id = @Id";
            var parameters = new[] { new SqlParameter("@Id", id) };
            try
            {
                await _sql.ExecuteNonQueryAsync(query, parameters);
                return Ok(new { message = "Danh mục đã được xóa" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa danh mục", error = ex.Message });
            }
        }
    }
}
