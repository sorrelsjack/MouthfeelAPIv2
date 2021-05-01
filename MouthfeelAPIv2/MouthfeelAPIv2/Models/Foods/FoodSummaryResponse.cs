using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models.Foods
{
    public class FoodSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        // TODO: Maybe make an enum for this
        public int Sentiment { get; set; }
        public bool ToTry { get; set; }
        public IEnumerable<VotableAttribute> TopThree { get; set; }

        public FoodSummaryResponse(Food food, int sentiment, bool toTry, IEnumerable<VotableAttribute> topThree)
        {
            Id = food.Id;
            Name = food.Name;
            ImageUrl = food.ImageUrl;
            Sentiment = sentiment;
            ToTry = toTry;
            TopThree = topThree;
        }
    }
}
