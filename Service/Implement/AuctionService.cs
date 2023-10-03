using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionDAO _auctionDAO;

        public AuctionService (IAuctionDAO auctionDAO)
        {
            _auctionDAO = auctionDAO;
        }

        public List<Auction> GetAuctions(string? title, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => a.Status == (int)Status.Available
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderByDescending(a => a.Product.Condition);
                        break;
                    //case 2:
                    //    auctions = auctions.OrderByDescending(p => p.StartedAt);
                    //    break;
                    //case 3:
                    //    auctions = auctions.OrderBy(p => p.Price);
                    //    break;
                    default:
                        auctions = auctions.OrderBy(a => a.Product.Condition);
                        break;
                }

                auctionList = auctions
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return auctionList;
        }

        public List<Auction> GetAuctionAssigned(int staffId)
        {
            return _auctionDAO.GetAuctionAssigned(staffId).ToList();
        }

        public Auction GetAuctionById(int id)
        {
            return _auctionDAO.GetAuctionById(id);
        }

        public List<Auction> GetAuctionBySellerId(int sellerId)
        {
            return _auctionDAO.GetAuctionBySellerId(sellerId).ToList();
        }

        public List<Auction> GetAuctionJoined(int bidderId)
        {
            throw new NotImplementedException();
        }

        public void CreateAuction(Auction auction)
        {
            auction.EntryFee = 0.1m * auction.StartingPrice;
            auction.StaffId = null;
            auction.Status = (int)Status.Unavailable;
            auction.StartedAt = null;
            auction.EndedAt = null;
            auction.CreatedAt = DateTime.Now;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.CreateAuction(auction);
        }
    }
}
