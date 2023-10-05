using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IBidService
    {
        public List<Bid> GetAllBidsFromAuction(int auctionId);
        public Bid GetHighestBidFromAuction(int auctionId);
        public void PlaceBid(Bid bid);
    }
}
