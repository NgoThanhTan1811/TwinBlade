using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwinBlade.Domain.Enums;

namespace TwinBlade.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; set; }

        public string RoomCode { get; set; } = string.Empty;

        public Guid HostPlayerId { get; set; }

        public RoomStatus Status { get; set; }

        public int MaxPlayers { get; set; }

        public List<RoomPlayer> Players { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}