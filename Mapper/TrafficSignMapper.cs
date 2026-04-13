using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public class TrafficSignMapper
    {
        public static TrafficSignDto ToTrafficSignDto(SqlDataReader reader)
        {
            return new TrafficSignDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"))
            };
        }
    }
}
