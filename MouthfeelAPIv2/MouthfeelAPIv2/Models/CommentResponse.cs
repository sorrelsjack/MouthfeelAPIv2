using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FoodId { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }
        public int Votes { get; set; }
        public int Sentiment { get; set; }

        public CommentResponse(Comment comment, int votes, int sentiment)
        {
            Id = comment.Id;
            UserId = comment.UserId;
            FoodId = comment.FoodId;
            Body = comment.Body;
            DateTime = comment.DateTime;
            Votes = votes;
            Sentiment = sentiment;
        }
    }
}
