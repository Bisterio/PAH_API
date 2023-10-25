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

        public void PlaceBid(int bidderId, Bid bid) 
        {
            if (bid.AuctionId != null)
            {
                Auction auction = _auctionDAO.GetAuctionById((int)bid.AuctionId);
                var registrationList = _bidDAO.GetBidsByAuctionId(auction.Id).Where(b => b.Status == (int)BidStatus.Register).ToList();
                if (auction.Product.SellerId == bidderId)
                {
                    throw new Exception("400: Bạn không được tham dự cuộc đấu giá của mình");
                }
                // CHECK FOR REGISTERED
                if (!registrationList.Any(b => b.BidderId == bidderId))
                {
                    throw new Exception("400: Bạn chưa đăng ký tham gia cuộc đấu giá này");
                }
                var auctionStatus = auction.Status;
                if (auctionStatus < (int)AuctionStatus.Opened && DateTime.Now < auction.StartedAt)
                {
                    throw new Exception("400: Cuộc đấu giá này chưa mở");
                }
                else if (auctionStatus > (int)AuctionStatus.Opened && DateTime.Now > auction.EndedAt)
                {
                    throw new Exception("400: Cuộc đấu giá này đã kết thúc");
                }
                else
                {
                    // CHECK FOR RETRACTED
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
                        if (bidderId == highestBid.BidderId && highestBid.Status == 1)
                        {
                            throw new Exception("400: Giá của bạn đang là cao nhất");
                        }

                        if(bid.BidAmount <= highestBid.BidAmount)
                        {
                            throw new Exception("400: Bạn cần phải đưa giá cao hơn giá hiện tại");
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

                            //DateTime auctionEndDate = (DateTime)auction.EndedAt;
                            //auctionEndDate.AddSeconds(30);
                            //auction.EndedAt = auctionEndDate;
                            //_auctionDAO.UpdateAuction(auction);
                        }
                        else
                        {
                            throw new Exception("400: Your balance is not sufficient.");
                        }
                    }
                }
            }
        }

        public void RegisterToJoinAuction(int bidderId, int auctionId)
        {
            Auction auction = _auctionDAO.GetAuctionById(auctionId);
            var bidderWallet = _walletDAO.Get(bidderId);
            Bid bid = new Bid();
            if(auction.Status < (int)AuctionStatus.RegistrationOpen && DateTime.Now < auction.RegistrationStart)
            {
                throw new Exception("400: This auction hasn't been opened for registration");
            }
            else if (auction.Status > (int)AuctionStatus.RegistrationOpen && DateTime.Now > auction.RegistrationEnd)
            {
                throw new Exception("400: This auction registration has been closed");
            } 
            if(auction.Product.SellerId == bidderId)
            {
                throw new Exception("400: You cannot join your own auction");
            }
            else
            {
                var checkRegistration = _bidDAO.GetBidsByAuctionId(auctionId)
                    .Where(b => b.BidderId == bidderId && b.Status == (int)BidStatus.Register)
                    .Any();

                if (checkRegistration)
                {
                    throw new Exception("400: You have already registered for this auction");
                }

                if (bidderWallet.AvailableBalance >= (auction.EntryFee + auction.StartingPrice))
                {
                    bid.Id = 0;
                    bid.AuctionId = auctionId;
                    bid.BidderId = bidderId;
                    bid.BidAmount = auction.StartingPrice;
                    bid.BidDate = DateTime.Now;
                    bid.Status = (int)BidStatus.Register;

                    bidderWallet.AvailableBalance -= (auction.EntryFee + auction.StartingPrice);
                    bidderWallet.LockedBalance += auction.StartingPrice;

                    _walletDAO.Update(bidderWallet);
                    _transactionDAO.Create(new Transaction()
                    {
                        Id = 0,
                        WalletId = bidderWallet.Id,
                        PaymentMethod = (int)PaymentType.Wallet,
                        Amount = bid.BidAmount,
                        Type = (int)TransactionType.Deposit,
                        Date = DateTime.Now,
                        Description = $"Register to join auction {auction.Id}",
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
                _bidDAO.UpdateBid(bid);
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
