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
        public CategoryController(SqlHelper sql) { 
            _sql = sql;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] string? PhanLoai)
        {
            int parameter;
            if(PhanLoai == "BienBao")
            {
                parameter = 2;
            }
            else
            {
                parameter=1;
            }

            string query = "SELECT Id, Name FROM Category Where Type = @PhanLoai";
            var parameters = new[] { new SqlParameter("@PhanLoai", parameter) };
            var categories = await _sql.ExecuteQueryAsync(query, CategoryMapper.ToCategoryDto, parameters);            
            return Ok(categories);

        }
    }
}
