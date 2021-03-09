using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("Users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("pw")]
        public string Password { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("date_time_joined")]
        public DateTime DateTimeJoined { get; set; }
    }
}
