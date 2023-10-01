using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class ProductDAO : DataAccessBase<Product>, IProductDAO
    {
        public ProductDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        public IQueryable<Product> GetProducts()
        {
            return GetAll().Include(c => c.Category)
                    .Include(m => m.Material)
                    .Include(s => s.Seller);
        }

        public Product GetProductById(int id)
        {
            return GetAll().Include(c => c.Category)
                    .Include(m => m.Material)
                    .Include(s => s.Seller).FirstOrDefault(p => p.Id == id && p.Status != 0);
        }

        public IQueryable<Product> GetProductsBySellerId(int sellerId)
        {
            return GetAll().Include(c => c.Category)
                    .Include(m => m.Material)
                    .Include(s => s.Seller).Where(p => p.SellerId == sellerId);
        }

        public void CreateProduct(Product product)
        {
            Create(product);
        }

        public void UpdateProduct(Product product)
        {
            Update(product);
        }
    }
}
