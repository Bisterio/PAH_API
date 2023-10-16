using DataAccess;
using DataAccess.Implement;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Service.Implement
{
    public class BidService : IBidService
    {
        private readonly IBidDAO _bidDAO;
        private readonly IAuctionDAO _auctionDAO;
        private readonly IUserDAO _userDAO;
        private readonly IWalletDAO _walletDAO;
        private readonly ITransactionDAO _transactionDAO;

        public BidService(IBidDAO bidDAO, IAuctionDAO auctionDAO, IUserDAO userDAO, IWalletDAO walletDAO, ITransactionDAO transactionDAO)
        {
            _bidDAO = bidDAO;
            _auctionDAO = auctionDAO;
            _userDAO = userDAO;
            _walletDAO = walletDAO;
            _transactionDAO = transactionDAO;
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

        public void PlaceBid(int bidderId, Bid bid) // chua tru di entry fee
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
                        var remainder = bid.BidAmount - previousBid.BidAmount;
                        if (bidderWallet.AvailableBalance >= remainder)
                        {
                            bid.Id = 0;
                            bid.BidderId = bidderId;
                            bid.BidDate = DateTime.Now;
                            bid.Status = (int)BidStatus.Active;

                            bidderWallet.AvailableBalance -= remainder;
                            bidderWallet.LockedBalance += remainder;

                            _walletDAO.Update(bidderWallet);
                            _transactionDAO.Create(new Transaction()
                            {
                                Id = 0,
                                WalletId = bidderWallet.Id,
                                PaymentMethod = (int)PaymentType.Wallet,
                                Amount = remainder,
                                Type = (int)TransactionType.Deposit,
                                Date = DateTime.Now,
                                Description = $"Place bid for auction {auction}",
                                Status = (int)Status.Available,
                            });
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
                        if (bidderWallet.AvailableBalance >= bid.BidAmount)
                        {
                            bid.Id = 0;
                            bid.BidderId = bidderId;
                            bid.BidDate = DateTime.Now;
                            bid.Status = (int)BidStatus.Active;

                            bidderWallet.AvailableBalance -= bid.BidAmount;
                            bidderWallet.LockedBalance += bid.BidAmount;

                            _walletDAO.Update(bidderWallet);
                            _transactionDAO.Create(new Transaction()
                            {
                                Id = 0,
                                WalletId = bidderWallet.Id,
                                PaymentMethod = (int)PaymentType.Wallet,
                                Amount = bid.BidAmount,
                                Type = (int)TransactionType.Deposit,
                                Date = DateTime.Now,
                                Description = $"Place bid for auction {auction.Id}",
                                Status = (int)Status.Available,
                            });
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

        public void RetractBid(int auctionId, int bidderId)
        {
            var bidList = _bidDAO.GetBidsByAuctionId(auctionId)
                .Where(b => b.BidderId == bidderId)
                .OrderByDescending(b => b.BidAmount)
                .ToList();
            foreach (var bid in bidList)
            {
                if (bid.Status == (int)BidStatus.Retracted)
                {
                    throw new Exception("400: You have retracted before");
                }
                bid.Status = (int)BidStatus.Retracted;
            }
            var bidderWallet = _walletDAO.Get(bidderId);
            var previousBid = _bidDAO.GetBidsByAuctionId(auctionId)
                            .Where(b => b.BidderId == bidderId)
                            .OrderByDescending(b => b.BidAmount)
                            .FirstOrDefault();
            bidderWallet.AvailableBalance += previousBid.BidAmount;
            bidderWallet.LockedBalance -= previousBid.BidAmount;

            _walletDAO.Update(bidderWallet);
            _transactionDAO.Create(new Transaction()
            {
                Id = 0,
                WalletId = bidderWallet.Id,
                PaymentMethod = (int)PaymentType.Wallet,
                Amount = previousBid.BidAmount,
                Type = (int)TransactionType.Refund,
                Date = DateTime.Now,
                Description = $"Return balance due to retracting from auction {auctionId}",
                Status = (int)Status.Available,
            });
        }
    }
}
