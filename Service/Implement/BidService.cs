using DataAccess;
using DataAccess.Implement;
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
        private readonly IUserDAO _userDAO;
        private readonly IWalletDAO _walletDAO;

        public BidService(IBidDAO bidDAO, IAuctionDAO auctionDAO, IUserDAO userDAO, IWalletDAO walletDAO)
        {
            _bidDAO = bidDAO;
            _auctionDAO = auctionDAO;
            _userDAO = userDAO;
            _walletDAO = walletDAO;
        }

        public List<Bid> GetAllBidsFromAuction(int auctionId, int status)
        {
            var bids = _bidDAO.GetBidsByAuctionId(auctionId).Where(b => status == 0 || b.Status == status).OrderByDescending(b => b.BidDate).ToList();
            return bids;
        }

        public Bid GetHighestBidFromAuction(int auctionId)
        {
            Bid bid = _bidDAO.GetBidsByAuctionId(auctionId)
                .Where(b => b.Status == (int)BidStatus.Active)
                .OrderByDescending(a => a.BidAmount)
                .FirstOrDefault();
            return bid;
        }

        public int GetNumberOfBidders(int auctionId)
        {
            return GetAllBidsFromAuction(auctionId, (int)BidStatus.Active)
                .GroupBy(b => b.BidderId)
                .Count();
        }

        public int GetNumberOfBids(int auctionId)
        {
            return GetAllBidsFromAuction(auctionId, (int)BidStatus.Active)
                .Count();
        }

        public void PlaceBid(int bidderId, Bid bid)
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
                    throw new Exception("400: This auction hasn't opened");
                }
                else if (auctionStatus == (int) AuctionStatus.Ended
                    || auctionStatus == (int)AuctionStatus.Sold
                    || auctionStatus == (int)AuctionStatus.Expired)
                {
                    throw new Exception("400: This auction has ended");
                }
                else
                {                    
                    if (_bidDAO.GetBidsByAuctionId((int)bid.AuctionId).Where(b => b.BidderId == bidderId).Any(b => b.Status == (int)BidStatus.Retracted))
                    {
                        throw new Exception("400: You have retracted before");
                    }
                    var bidderWallet = _walletDAO.Get(bidderId);
                    var check = _bidDAO.GetBidsByAuctionId((int)bid.AuctionId).Where(b => b.BidderId == bidderId);
                    if (check.Any())
                    {
                        // CASE HIGHEST BID
                        var highestBid = _bidDAO.GetBidsByAuctionId((int)bid.AuctionId).OrderByDescending(b => b.BidAmount).FirstOrDefault();
                        if (bidderId == highestBid.BidderId)
                        {
                            throw new Exception("400: Your bid is the highest bid currently");
                        }

                        // CASE SECOND BID ONWARD
                        var previousBid = _bidDAO.GetBidsByAuctionId((int)bid.AuctionId)
                            .Where(b => b.BidderId == bidderId)
                            .OrderByDescending(b => b.BidAmount)
                            .FirstOrDefault();
                        if (bidderWallet.AvailableBalance - previousBid.BidAmount - auction.EntryFee >= bid.BidAmount)
                        {
                            bid.Id = 0;
                            bid.BidderId = bidderId;
                            bid.BidDate = DateTime.Now;
                            bid.Status = (int)BidStatus.Active;
                            _bidDAO.CreateBid(bid);
                        }
                        else
                        {
                            throw new Exception("400: Your balance is not sufficient.");
                        }
                    }
                    else
                    {
                        // CASE FIRST BID                    
                        if (bidderWallet.AvailableBalance - auction.EntryFee >= bid.BidAmount)
                        {
                            bid.Id = 0;
                            bid.BidderId = bidderId;
                            bid.BidDate = DateTime.Now;
                            bid.Status = (int)BidStatus.Active;
                            _bidDAO.CreateBid(bid);
                        }
                        else
                        {
                            throw new Exception("400: Your balance is not sufficient.");
                        }
                    }                    
                }
            }
        }
    }
}
