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
        private readonly IAuctionDAO _auctionDAO;

        public BidService(IBidDAO bidDAO, IAuctionDAO auctionDAO)
        {
            _bidDAO = bidDAO;
            _auctionDAO = auctionDAO;
        }

        public List<Bid> GetAllBidsFromAuction(int auctionId)
        {
            return _bidDAO.GetBidsByAuctionId(auctionId).ToList();
        }

        public Bid GetHighestBidFromAuction(int auctionId)
        {
            Bid bid = _bidDAO.GetBidsByAuctionId(auctionId).OrderByDescending(a => a.BidAmount).FirstOrDefault();
            return bid;
        }

        public void PlaceBid(Bid bid)
        {
            if(bid.AuctionId != null)
            {
                Auction auction = _auctionDAO.GetAuctionById((int)bid.AuctionId);
                var auctionStatus = auction.Status;
                if(auctionStatus == (int)AuctionStatus.Unassigned
                    || auctionStatus == (int)AuctionStatus.Pending
                    || auctionStatus == (int)AuctionStatus.Approved
                    || auctionStatus == (int)AuctionStatus.Rejected
                    || auctionStatus == (int)AuctionStatus.Unavailable)
                {
                    throw new Exception("404: This auction hasn't opened");
                }
                else if (auctionStatus == (int) AuctionStatus.Ended
                    || auctionStatus == (int)AuctionStatus.Sold
                    || auctionStatus == (int)AuctionStatus.Expired)
                {
                    throw new Exception("404: This auction has ended.");
                }
                else
                {
                    
                    if (_bidDAO.GetBidsByAuctionId((int)bid.AuctionId).Any(b => b.Status == (int)BidStatus.Retracted))
                    {
                        throw new Exception("404: You have retracted before.");
                    }
                }
            }
        }
    }
}
