using DataAccess;
using DataAccess.Models;
using Hangfire;
using Hangfire.Storage;
using Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionDAO _auctionDAO;
        private readonly IBidDAO _bidDAO;
        private readonly IUserDAO _userDAO;
        private readonly IWalletDAO _walletDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IOrderDAO _orderDAO;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IAddressDAO _addressDAO;
        private readonly IWalletService _walletService;
        private readonly IOrderService _orderService;
        private readonly IProductImageDAO _productImageDAO;     

        public AuctionService (IAuctionDAO auctionDAO, IBackgroundJobClient backgroundJobClient, IUserDAO userDAO, 
            IBidDAO bidDAO, IWalletDAO walletDAO, ITransactionDAO transactionDAO, IOrderDAO orderDAO, 
            IAddressDAO addressDAO, IWalletService walletService, IProductImageDAO productImageDAO, IOrderService orderService)
        {
            _auctionDAO = auctionDAO;
            _userDAO = userDAO;
            _bidDAO = bidDAO;
            _backgroundJobClient = backgroundJobClient;
            _addressDAO = addressDAO;
            _orderDAO = orderDAO;
            _walletService = walletService;
            _walletDAO = walletDAO;
            _transactionDAO = transactionDAO;
            _orderService = orderService;
            _productImageDAO = productImageDAO;
        }

        public List<Auction> GetAuctions(string? title, int status, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => status == -1 || a.Status == status
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId)
                    && (a.RegistrationStart < DateTime.Now && DateTime.Now < a.RegistrationEnd));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderByDescending(a => a.StartedAt);
                        break;
                    case 2:
                        auctions = auctions.OrderBy(p => p.StartingPrice);
                        break;
                    case 3:
                        auctions = auctions.OrderByDescending(p => p.StartingPrice);
                        break;
                    default:
                        auctions = auctions.OrderBy(a => a.StartedAt);
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

        public List<Auction> GetAllAuctions(string? title, int status, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => status == -1 || a.Status == status
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderBy(a => a.CreatedAt);
                        break;
                    case 2:
                        auctions = auctions.OrderBy(p => p.StartingPrice);
                        break;
                    case 3:
                        auctions = auctions.OrderByDescending(p => p.StartingPrice);
                        break;
                    default:
                        auctions = auctions.OrderByDescending(a => a.CreatedAt);
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
            if (staffId == null)
            {
                throw new Exception("404: Staff not found");
            }
            return _auctionDAO.GetAuctionAssigned(staffId).ToList();
        }

        public List<Auction> GetAuctionsByProductId(int productId)
        {
            if (productId == null)
            {
                throw new Exception("404: Product not found");
            }
            return _auctionDAO.GetAuctionsByProductId(productId).ToList();
        }

        public Auction GetAuctionById(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Auction not found");
            }
            return _auctionDAO.GetAuctionById(id);
        }

        public List<Auction> GetAuctionBySellerId(int sellerId, int status)
        {
            if (sellerId == null)
            {
                throw new Exception("404: Seller not found");
            }
            return _auctionDAO.GetAuctionBySellerId(sellerId)
                .Where(a => status == -1 || a.Status == status)
                .ToList();
        }

        public List<Auction> GetAuctionJoined(int bidderId)
        {
            if (bidderId == null)
            {
                throw new Exception("404: Bidder not found");
            }
            return _auctionDAO.GetAuctionJoined(bidderId).ToList();
        }

        public List<Auction> GetAuctionJoinedByStatus(int status, int bidderId)
        {
            if (bidderId == null)
            {
                throw new Exception("404: Bidder not found");
            }
            return _auctionDAO.GetAuctionJoined(bidderId).Where(a => status == -1 || a.Status == status).ToList();
        }

        public void CreateAuction(Auction auction)
        {
            auction.EntryFee = 0.05m * auction.StartingPrice;
            auction.StaffId = null;
            auction.Status = (int) AuctionStatus.Unassigned;
            auction.CreatedAt = DateTime.Now;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.CreateAuction(auction);
        }

        public void AssignStaff(int id, int staffId)
        {
            if (id == null)
            {
                throw new Exception("404: Auction not found");
            } 
            else if (staffId == null || _userDAO.Get(staffId).Role != (int)Role.Staff)
            {
                throw new Exception("404: Staff not found");
            }
            Auction auction = _auctionDAO.GetAuctionById(id);
            if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction hasn been rejected");
            }
            else if (auction.Status > (int)AuctionStatus.Unassigned && auction.Status != (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction has been assigned");
            } 
            auction.StaffId = staffId;
            auction.Status = (int)AuctionStatus.Assigned;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.UpdateAuction(auction);
        }

        public void ManagerApproveAuction(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Auction not found");
            }
            Auction auction = GetAuctionById(id);

            if(auction.Status > (int) AuctionStatus.Pending && auction.Status != (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction is already approved.");
            } 
            else if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction is already rejected.");
            }
            else
            {                
                auction.Status = (int)AuctionStatus.Unassigned;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
                ////Schedule task to open/end auction
                //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int)AuctionStatus.Opened), auction.StartedAt.Value);
                //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int)AuctionStatus.Ended), auction.EndedAt.Value);
            }
        }

        public void ManagerRejectAuction(int id)
        {
            if (id == null)
            {
                throw new Exception("404: Auction not found");
            }
            Auction auction = GetAuctionById(id);

            if (auction.Status > (int)AuctionStatus.Pending && auction.Status != (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction is already approved.");
            }
            else if (auction.Status == (int)AuctionStatus.Rejected)
            {
                throw new Exception("400: This auction is already rejected.");
            }
            else
            {
                auction.Status = (int)AuctionStatus.Rejected;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
            }
        }

        public void StaffSetAuctionTime(int id, DateTime registrationStart, DateTime registrationEnd, DateTime startedAt, DateTime endedAt,
            decimal step)
        {
            if (id == null)
            {
                throw new Exception("404: Auction not found");
            }
            Auction auction = GetAuctionById(id);
            if(auction.Status < (int)AuctionStatus.Assigned)
            {
                throw new Exception("400: This auction hasn't been assigned to you");
            } 
            else if(auction.Status > (int)AuctionStatus.RegistrationOpen)
            {
                throw new Exception("400: You cannot edit this auction anymore");
            }
            else
            {
                auction.RegistrationStart = registrationStart;
                auction.RegistrationEnd = registrationEnd;
                auction.StartedAt = startedAt;
                auction.EndedAt = endedAt;
                auction.Status = (int)AuctionStatus.RegistrationOpen;
                auction.UpdatedAt = DateTime.Now;
                auction.Step = step;
                _auctionDAO.UpdateAuction(auction);
            }
        }

        public bool HostAuction(int auctionId, int status) {
            var auction = GetAuctionById(auctionId);

            if (auction == null) {
                return false;
            }

            if(status == (int)AuctionStatus.Opened && DateTime.Now < auction.StartedAt)
            {
                return false;
            }

            auction.Status = status;
            _auctionDAO.UpdateAuction(auction);
            return true;
        }

        public Bid EndAuction(int auctionId)
        {
            if (auctionId == null) 
            {
                throw new Exception("404: Auction not found");
            }
            Auction auction = GetAuctionById(auctionId);
            if (auction.Status < (int)AuctionStatus.Opened)
            {
                throw new Exception("400: This auction hasn't opened.");
            }
            if (auction.Status > (int)AuctionStatus.Opened)
            {
                throw new Exception("400: This auction has ended.");
            }
            List<Bid> activeBids = _bidDAO.GetBidsByAuctionId(auctionId).Where(b => b.Status == (int)BidStatus.Active).ToList();
            if (activeBids.Count == 0)
            {
                auction.Status = (int)AuctionStatus.EndedWithoutBids;
                _auctionDAO.UpdateAuction(auction);

                List<Bid> registerBidList = _bidDAO.GetBidsByAuctionId(auctionId)
                            .Where(b => b.Status == (int)BidStatus.Register).ToList();

                foreach (Bid registerBid in registerBidList)
                {
                    Wallet bidderWallet = _walletDAO.Get((int)registerBid.BidderId); // lay cai wallet ra
                    bidderWallet.AvailableBalance += registerBid.BidAmount; // tra tien
                    bidderWallet.LockedBalance -= registerBid.BidAmount;

                    registerBid.Status = (int)BidStatus.Refund; // set status tra tien
                    _bidDAO.UpdateBid(registerBid);
                }

                return null;
            }
            else
            {
                auction.Status = (int)AuctionStatus.Ended;
                _auctionDAO.UpdateAuction(auction);

                var winnerBid = activeBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
                User winner = _userDAO.Get((int)winnerBid.BidderId);

                var highestBidOfEachBidder = _bidDAO.GetBidsByAuctionId(auctionId).GroupBy(b => b.BidderId) // lay het tat ca bid cao nhat cua tung thang
                    .Select(s => s.OrderByDescending(b => b.BidAmount).First())
                    .ToList();

                foreach (var bid in highestBidOfEachBidder)
                {
                    if (bid.BidderId != winner.Id)
                    {
                        Wallet bidderWallet = _walletDAO.Get((int)bid.BidderId); // lay cai wallet ra
                        bidderWallet.AvailableBalance += bid.BidAmount; // tra tien
                        bidderWallet.LockedBalance -= bid.BidAmount;

                        List<Bid> previousBidList = _bidDAO.GetBidsByAuctionId(auctionId)
                            .Where(b => b.BidderId == bid.BidderId && b.Status == (int)BidStatus.Active).ToList();

                        foreach(Bid previousBid in previousBidList)
                        {
                            previousBid.Status = (int)BidStatus.Refund; // set status tra tien
                            _bidDAO.UpdateBid(previousBid);
                        }                        

                        _walletDAO.Update(bidderWallet);
                        //_transactionDAO.Create(new Transaction()
                        //{
                        //    Id = 0,
                        //    WalletId = bidderWallet.Id,
                        //    PaymentMethod = (int)PaymentType.Wallet,
                        //    Amount = bid.BidAmount,
                        //    Type = (int)TransactionType.Refund,
                        //    Date = DateTime.Now,
                        //    Description = $"Return balance due to ending auction {auctionId}",
                        //    Status = (int)Status.Available,
                        //});
                    }    
                    else
                    {
                        Wallet winnerWallet = _walletDAO.Get((int)bid.BidderId); // lay cai wallet cua thang thang ra
                        winnerWallet.LockedBalance -= bid.BidAmount;
                        _walletDAO.Update(winnerWallet);
                        _transactionDAO.Create(new Transaction()
                        {
                            Id = 0,
                            WalletId = winnerWallet.Id,
                            PaymentMethod = (int)PaymentType.Wallet,
                            Amount = bid.BidAmount,
                            Type = (int)TransactionType.Payment,
                            Date = DateTime.Now,
                            Description = $"Thanh toán cuộc đấu giá '{auction.Title}'",
                            Status = (int)Status.Available,
                        });
                    }
                }
                return winnerBid;
            }
        }

        public bool CheckRegistration(int bidderId, int auctionId)
        {
            if(bidderId == null)
            {
                throw new Exception("400: Bidder not found");
            }
            if(auctionId == null)
            {
                throw new Exception("400: Auction not found");
            }
            var checkRegistration = _bidDAO.GetBidsByAuctionId(auctionId)
                    .Where(b => b.BidderId == bidderId && b.Status == (int)BidStatus.Register)
                    .Any();
            return checkRegistration;
        }

        public void CreateAuctionOrder(int userId, AuctionOrderRequest request) {
            var auction = _auctionDAO.GetAuctionById(request.AuctionId);
            if (auction == null) {
                throw new Exception("404: Auction not found when creating order");
            }
            if (auction.Status != (int) AuctionStatus.Ended) {
                throw new Exception("401: This auction cannot be created order with");
            }

            var address = _addressDAO.Get(request.AddressId);
            if (address == null) {
                throw new Exception("404: Address not found when creating auction order");
            }

            var existOrder = _orderDAO.GetByProductId(auction.ProductId.Value);
            if (existOrder != null) {
                throw new Exception("409: This auction cannot be created with more order");
            }

            var wallet = _walletService.GetByCurrentUser(userId);
            if (wallet == null) {
                throw new Exception("404: Wallet not found when creating auction order");
            }
            if (wallet.AvailableBalance < request.ShippingPrice) {
                throw new Exception("401: Your wallet does not have enough balance to create order");
            }

            var now = DateTime.Now;
            var highestBid = _bidDAO.GetBidsByAuctionId(request.AuctionId).OrderByDescending(b => b.BidAmount).FirstOrDefault();

            var order = new Order {
                BuyerId = userId,
                SellerId = auction.Product.SellerId,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                RecipientAddress = address.Street + ", " + address.Ward + ", " + address.District + ", " + address.Province,
                OrderDate = now,
                TotalAmount = highestBid.BidAmount,
                ShippingCost = request.ShippingPrice,
                Status = (int) OrderStatus.ReadyForPickup,
                OrderItems = new List<OrderItem>()
            };

            order.OrderItems.Add(new OrderItem {
                ProductId = auction.ProductId.Value,
                Price = highestBid.BidAmount,
                Quantity = 1,
                ImageUrl = _productImageDAO.GetByProductId((int)auction.ProductId).FirstOrDefault().ImageUrl
            });
            _orderDAO.Create(order);
            var orderList = _orderDAO.GetAllByBuyerIdAfterCheckout(userId, now).ToList();
            foreach(var item in orderList) {
                //_walletService.CheckoutWallet(userId, item.Id, (int) OrderStatus.ReadyForPickup);
                _orderService.CreateShippingOrder(item.Id);
            }
        }
        public bool CheckWinner(int bidderId, int auctionId)
        {
            Auction auction = _auctionDAO.GetAuctionById(auctionId);
            Product product = auction.Product;
            var checkOrderExist = _orderDAO.GetAllOrder().Where(o => o.OrderItems.Any(i => i.ProductId == product.Id)).ToList();
            if(!CheckRegistration(bidderId, auctionId))
            {
                return false;
            }
            List<Bid> activeBids = _bidDAO.GetBidsByAuctionId(auctionId)
                .Where(b => b.Status == (int)BidStatus.Active).ToList();
            if (activeBids.Count == 0)
            {
                return false;
            }
            if (checkOrderExist.Count > 0)
            {
                return false;
            }
            Bid winnerBid = activeBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
            if(winnerBid.BidderId == bidderId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void TestSchedule() {
        //    var auction = _auctionDAO.GetAuctionById(3);

        //    auction.StartedAt = DateTime.Now.AddMinutes(1);
        //    auction.EndedAt = DateTime.Now.AddMinutes(2);
        //    _auctionDAO.UpdateAuction(auction);
        //_backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Opened), auction.StartedAt.Value);
        //    _backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Ended), auction.EndedAt.Value);
        //}

        //public void OpenAuction(int id)
        //{
        //    Auction auction = GetAuctionById(id);
        //    var status = auction.Status;
        //    switch (status)
        //    {
        //        case (int) AuctionStatus.Approved:
        //            if (auction.StartedAt == DateTime.Now)
        //            {
        //                auction.Status = (int)AuctionStatus.Opened;
        //                auction.UpdatedAt = DateTime.Now;
        //                _auctionDAO.UpdateAuction(auction);
        //            }                    
        //            break;

        //        case (int)AuctionStatus.Pending:
        //            throw new Exception("400: This auction is unapproved.");

        //        case (int)AuctionStatus.Unassigned:
        //            throw new Exception("400: This auction is unassigned.");

        //        case (int)AuctionStatus.Rejected:
        //            throw new Exception("400: This auction is rejected.");

        //        case (int)AuctionStatus.Opened:
        //            throw new Exception("400: This auction is opening.");

        //        default: throw new Exception("400: This auction is ended.");
        //    }
        //}

        //public void EndAuction(int id)
        //{
        //    Auction auction = GetAuctionById(id);
        //    var status = auction.Status;

        //    if (auction.Status == (int)AuctionStatus.Opened)
        //    {
        //        throw new Exception("400: This auction is opening.");
        //    }
        //    else if (auction.Status == (int)AuctionStatus.Ended
        //        || auction.Status == (int)AuctionStatus.Sold
        //        || auction.Status == (int)AuctionStatus.Expired)
        //    {
        //        throw new Exception("400: This auction is ended.");
        //    }
        //    else
        //    {
        //        throw new Exception("400: This auction cannot be rejected.");
        //    }
        //}
    }
}
