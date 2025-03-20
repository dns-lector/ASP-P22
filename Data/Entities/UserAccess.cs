﻿using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
    public class UserAccess
    {
        public Guid   Id     { get; set; }
        public Guid   UserId { get; set; }
        public String Login  { get; set; }
        public String Dk     { get; set; }
        public String Salt   { get; set; }

        public Guid?  RoleId { get; set; }


        [JsonIgnore]
        public User User { get; set; }
        public UserRole? UserRole { get; set; }
    }
}
