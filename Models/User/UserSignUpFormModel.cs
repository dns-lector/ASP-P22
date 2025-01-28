﻿using System.Text.Json.Serialization;

namespace ASP_P22.Models.User
{
    public class UserSignUpFormModel
    {
        public String UserName  { get; set; } = null!;
        public String UserEmail { get; set; } = null!;
        public String UserLogin { get; set; } = null!;
        public String Password1 { get; set; } = null!;
        public String Password2 { get; set; } = null!;


        public String UserPhone { get; set; } = null!;
        public String UserPosition { get; set; } = null!;

        [JsonIgnore]
        public IFormFile UserPhoto { get; set; } = null!;
        public String UserPhotoSavedName { get; set; } = null!;

    }
}
