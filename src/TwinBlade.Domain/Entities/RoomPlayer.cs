using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class RoomPlayer
    {
        public Guid PlayerId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public bool IsReady { get; set; }
        public RoomPlayerState State { get; set; } = new();

    }
}