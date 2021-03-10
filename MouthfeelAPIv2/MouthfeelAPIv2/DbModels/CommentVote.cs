using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("comment_votes")]
    public class CommentVote
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("comment_id")]
        public int CommentId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("vote")]
        public int Vote { get; set; }
    }
}
