using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper;

public class CategoryMapper
{
    public static CategoryDto ToCategoryDto(SqlDataReader reader)
    {
        return new CategoryDto
        {
            CategoryId = reader.GetInt32(reader.GetOrdinal("Id")),
            CategoryName = reader.GetString(reader.GetOrdinal("Name"))
        };
    }
}
