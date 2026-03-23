using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public PlayerProgress Progress { get; set; } = new();
        
    }
}