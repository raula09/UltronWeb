﻿namespace ProductWeb.Models
{
    
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Revoked { get; set; } = false;
        public string CreatedByIp { get; set; } 
    }

}
