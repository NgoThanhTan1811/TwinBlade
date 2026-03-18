using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class MatchResult
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public DateTime FinishedAt { get; set; }

        public List<PlayerMatchResult> Players { get; set; } = new();
    }
}