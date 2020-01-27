using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Postgres_enron_database.Models
{
    public class EmailObject
    {
        [Key]
        [Required]
        public long Id { get; set; }
        [Required]
        [Column(TypeName = "varchar")]
        public string URL { get; set; }
        [Required]
        public DateTime SendDate { get; set; }
        [ForeignKey("EmailAddress")]
        [Required]
        public long FromId { get; set; }
    }
}
