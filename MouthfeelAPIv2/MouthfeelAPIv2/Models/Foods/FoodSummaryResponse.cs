using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attribute = MouthfeelAPIv2.DbModels.Attribute;

namespace MouthfeelAPIv2.Models.Foods
{
    public class FoodSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<FoodImage> Images { get; set; }

        // TODO: Maybe make an enum for this
        public int Sentiment { get; set; }
        public bool ToTry { get; set; }
        public IEnumerable<Attribute> TopThree { get; set; }

        public FoodSummaryResponse(Food food, IEnumerable<FoodImage> images, int sentiment, bool toTry, IEnumerable<Attribute> topThree)
        {
            Id = food.Id;
            Name = food.Name;
            Images = images;
            Sentiment = sentiment;
            ToTry = toTry;
            TopThree = topThree;
        }
    }
}
