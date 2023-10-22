using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface ISellerService
    {
        public Seller GetSeller(int id);
        public List<Seller> GetSellerRequestList();
        public int CreateSeller(int id, Seller seller);
        public void UpdateSeller(Seller seller);
    }
}
