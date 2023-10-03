using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IAuctionService
    {
        public List<Auction> GetAuctions(string? title, int categoryId, int materialId, int orderBy);
        public Auction GetAuctionById(int id);
        public List<Auction> GetAuctionAssigned(int staffId);
        public List<Auction> GetAuctionJoined(int bidderId);
        public List<Auction> GetAuctionBySellerId(int sellerId);
        public void CreateAuction(Auction auction);
    }
}
