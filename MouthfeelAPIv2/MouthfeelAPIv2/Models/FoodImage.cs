using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class FoodImage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FoodId { get; set; }
        // TODO: probably return this as a string. data:image/png;base64,xxxxxx
        public byte[] Image { get; set; }
    }
}
