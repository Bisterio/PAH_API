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

        public Seller GetSeller(int id)
        {
            return _sellerDAO.GetSeller(id);
        }
    }
}
