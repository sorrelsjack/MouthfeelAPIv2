using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MouthfeelAPIv2.Services
{
    public interface IFoodsService
    {
        Task AddFood(Food food);
    }

    public class FoodsService : IFoodsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public FoodsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task AddFood(Food food)
        {
            var foods = await _mouthfeel.Foods.ToListAsync();

            if (foods.Any(f => String.Equals(f.Name, food.Name, StringComparison.OrdinalIgnoreCase))) 
                throw new HttpResponseException(HttpStatusCode.BadRequest); // TODO: Fix this. It returns something nasty: System.Web.Http.HttpResponseException: Processing of the HTTP request resulted in an exception. Please see the HTTP response returned by the 'Response' property of this exception for details.

            _mouthfeel.Foods.Add(food);
            _mouthfeel.SaveChanges();
        }
    }
}
