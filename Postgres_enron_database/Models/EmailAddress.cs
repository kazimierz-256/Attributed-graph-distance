﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Postgres_enron_database.Models
{
    public class EmailAddress
    {
        [Key]
        [Required]
        public long Id { get; set; }
        [Required]
        [Column(TypeName = "varchar")]
        public string Address { get; set; }
        public bool BelongsToEnron { get; set; }
    }
}
