using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class BidService : IBidService
    {
        private readonly IBidDAO _bidDAO;

        public BidService(IBidDAO bidDAO)
        {
            _bidDAO = bidDAO;
        }

        public List<Bid> GetAllBidsFromAuction(int auctionId)
        {
            return _bidDAO.GetBidsByAuctionId(auctionId).ToList();
        }
    }
}
