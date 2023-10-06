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

        public Seller GetSeller(int id)
        {
            return _sellerDAO.GetSeller(id);
        }
    }
}
