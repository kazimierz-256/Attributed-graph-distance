using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Postgres_enron_database.Models
{
    public enum SendType
    {
        TO,
        BCC,
        CC
    }
    public class DestinationEmail
    {
        [ForeignKey("Email")]
        [Required]
        public long EmailId { get; set; }
        [ForeignKey("EmailAddress")]
        [Required]
        public long EmailAddressId { get; set; }
        [Required]
        public SendType SendType { get; set; }
    }
}
