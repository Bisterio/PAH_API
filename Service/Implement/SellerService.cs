using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
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

        public SellerService(ISellerDAO sellerDAO)
        {
            _sellerDAO = sellerDAO;
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
    }
}
