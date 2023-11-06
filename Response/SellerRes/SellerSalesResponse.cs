using Respon.OrderRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respon.SellerRes
{
    public class SellerSalesResponse
    {
        public decimal TotalSales { get; set; }
        public List<OrderSalesResponse> OrderList { get; set; }
    }
}
