using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class CreateFoodImageRequest
    {
        public int UserId { get; set; }
        public int FoodId { get; set; }
        public IFormFile Image { get; set; }
    }
}
