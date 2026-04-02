using ApiThiBangLaiXeOto.DTOs;
using System.Collections.Concurrent;

namespace ApiThiBangLaiXeOto.Service
{
    public static class OnlineStore
    {
        public static ConcurrentDictionary<string, OnlineUserDto> Users = new();
    }
}
