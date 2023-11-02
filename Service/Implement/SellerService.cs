using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using Request;
using Request.ThirdParty.GHN;
using Respon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service.Implement
{
    public class SellerService : ISellerService
    {
        private readonly ISellerDAO _sellerDAO;
        private IHttpClientFactory _httpClientFactory;

        public SellerService(ISellerDAO sellerDAO, IHttpClientFactory httpClientFactory)
        {
            _sellerDAO = sellerDAO;
            _httpClientFactory = httpClientFactory;
        }

        public int CreateSeller(int id, Seller seller)
        {
            var existed = _sellerDAO.GetSeller(id);
            if (existed == null)
            {
                seller.RegisteredAt = DateTime.Now;
                seller.Ratings = 0;
                seller.Status = (int)SellerStatus.Pending;
                _sellerDAO.CreateSeller(seller);
                return 0;
            }
            return 1;
        }

        public void UpdateSeller(Seller seller)
        {
            var current = _sellerDAO.GetSeller(seller.Id);
            current.Name = seller.Name;
            current.Phone = seller.Phone;
            current.ProfilePicture = seller.ProfilePicture;
            current.RegisteredAt = DateTime.Now;
            current.Ratings = 0;
            current.Status = (int)SellerStatus.Pending;
            _sellerDAO.UpdateSeller(current);
        }

        public Seller GetSeller(int id)
        {
            return _sellerDAO.GetSeller(id);
        }

        public List<Seller> GetSellerRequestList()
        {
            return _sellerDAO.GetSellerRequestList().ToList();
        }

        public async Task<int> CreateShopIdAsync(SellerRequest request) {
            var client = _httpClientFactory.CreateClient("GHN");
            var data = new ShopRequest {
                district_id = request.DistrictId,
                ward_code = request.WardCode,
                name = request.Name,
                phone = request.Phone,
                address = request.Street
            };
            var responseMessage = await client.PostAsync("v2/shop/register", Utils.ConvertForPost<ShopRequest>(data));
            if (!responseMessage.IsSuccessStatusCode) {
                var temp = await responseMessage.Content.ReadAsAsync<BaseGHNResponse<string>>();
                throw new Exception("409: " + temp.message);
            }
            var responseData = await responseMessage.Content.ReadAsAsync<BaseGHNResponse<ShopResponse>>();
            return responseData.data.shop_id;
        }
    }
}
