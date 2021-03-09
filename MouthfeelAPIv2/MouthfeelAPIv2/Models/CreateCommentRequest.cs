using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class CreateCommentRequest
    {
        public int UserId { get; set; }
        public int FoodId { get; set; }
        public string Body { get; set; }
    }
}
