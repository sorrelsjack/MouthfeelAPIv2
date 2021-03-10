using MouthfeelAPIv2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class ManageCommentVoteRequest
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }

        public int FoodId { get; set; }

        public VoteState Vote { get; set; }
    }
}
