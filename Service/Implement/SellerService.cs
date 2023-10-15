using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class SellerService : ISellerService
    {
        private readonly ISellerDAO _sellerDAO;

        public SellerService(ISellerDAO sellerDAO)
        {
            _sellerDAO = sellerDAO;
        }

        public void CreateSeller(int id, Seller seller)
        {
            var existed = _sellerDAO.GetSeller(id);
            if (existed != null)
            {
                throw new Exception("400: You are already a seller.");
            }

            seller.RegisteredAt = DateTime.Now;
            seller.Ratings = 0;
            seller.Status = (int)SellerStatus.Pending;
            _sellerDAO.CreateSeller(seller);
        }

        public Seller GetSeller(int id)
        {
            return _sellerDAO.GetSeller(id);
        }
    }
}
