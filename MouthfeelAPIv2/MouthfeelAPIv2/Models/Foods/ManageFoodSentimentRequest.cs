using MouthfeelAPIv2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models.Foods
{
    public class ManageFoodSentimentRequest
    {
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public Sentiment Sentiment { get; set; }
    }
}
