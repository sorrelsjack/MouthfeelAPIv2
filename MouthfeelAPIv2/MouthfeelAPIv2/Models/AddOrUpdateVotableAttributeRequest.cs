using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class AddOrUpdateVotableAttributeRequest
    {
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public int AttributeId { get; set; }

    }
}
