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
        public async Task<IActionResult> GetAllCategories()
        {
            string query = "SELECT Id, Name FROM Category";
            var categories = await _sql.ExecuteQueryAsync(query, CategoryMapper.ToCategoryDto);            
            return Ok(categories);

        }
    }
}
