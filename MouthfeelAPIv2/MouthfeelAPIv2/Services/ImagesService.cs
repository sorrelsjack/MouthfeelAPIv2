using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface IImagesService
    {
        Task<Models.FoodImage> UploadImage(CreateFoodImageRequest request);
        Task<IEnumerable<Models.FoodImage>> DownloadImages(int foodId);
        Task<IEnumerable<Models.FoodImage>> DownloadImagesForManyFoods(IEnumerable<int> foodIds);
    }

    public class ImagesService : IImagesService
    {
        private readonly MouthfeelContext _mouthfeel;

        public ImagesService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<Models.FoodImage> UploadImage(CreateFoodImageRequest request)
        {
            var img = new Models.FoodImage();

            MemoryStream ms = new MemoryStream();
            request.Image.CopyTo(ms);

            img.Image = ms.ToArray();

            ms.Close();
            ms.Dispose();

            var dbImage = new DbModels.FoodImage
            {
                UserId = request.UserId,
                FoodId = request.FoodId,
                Image = img.Image
            };

            _mouthfeel.FoodImages.Add(dbImage);
            await _mouthfeel.SaveChangesAsync();

            return img;
        }

        private async Task DownloadImage(int foodId)
        {
            
        }

        public async Task<IEnumerable<Models.FoodImage>> DownloadImages(int foodId)
        {
            var dbImgs = _mouthfeel.FoodImages.Where(i => i.FoodId == foodId);
            return dbImgs.Select(i => new Models.FoodImage { Id = i.Id, UserId = i.UserId, FoodId = i.FoodId, Image = i.Image });
            //var imgData = base64.Select(i => new Models.FoodImage { Id = i.Id, UserId = i.UserId, FoodId = i.FoodId, Image = string.Format("data:image/jpg;base64, {0}"), i.Base64Image });
        }

        public async Task<IEnumerable<Models.FoodImage>> DownloadImagesForManyFoods(IEnumerable<int> foodIds)
        {
            var dbImgs = _mouthfeel.FoodImages.Where(i => foodIds.Any(id => id == i.FoodId));
            return dbImgs.Select(i => new Models.FoodImage { Id = i.Id, UserId = i.UserId, FoodId = i.FoodId, Image = i.Image });
            //var imgData = base64.Select(i => new Models.FoodImage { Id = i.Id, UserId = i.UserId, FoodId = i.FoodId, Image = string.Format("data:image/jpg;base64, {0}"), i.Base64Image });
        }
    }
}
