using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Infrastructure.Options
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";

        public string Configuration { get; set; } = string.Empty;
        public string RoomKeyPrefix { get; set; } = "room:";
    }
}