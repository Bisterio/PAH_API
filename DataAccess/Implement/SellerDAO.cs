﻿using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class SellerDAO : DataAccessBase<Seller>, ISellerDAO
    {
        public SellerDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        public void CreateSeller(Seller seller)
        {
            Create(seller);
        }

        public Seller GetSeller(int id)
        {
            return GetAll()
                .Include(s => s.IdNavigation)
                .FirstOrDefault(s => s.Id == id);
        }
    }
}
