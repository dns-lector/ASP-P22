﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
    public class Product
    {
        public Guid    Id          { get; set; }
        public Guid    CategoryId  { get; set; }
        public String  Name        { get; set; } = String.Empty;
        public String? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price       { get; set; }
        public int     Stock       { get; set; }
        public String? ImagesCsv   { get; set; }
        public String? Slug        { get; set; }

        [JsonIgnore]
        public Category Category { get; set; } = null!;
    }
}
