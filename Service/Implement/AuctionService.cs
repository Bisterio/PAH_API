using DataAccess;
using DataAccess.Models;
using Hangfire;
using Hangfire.Storage;
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
        private readonly IBidDAO _bidDAO;
        private readonly IUserDAO _userDAO;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuctionService (IAuctionDAO auctionDAO, IBackgroundJobClient backgroundJobClient, IUserDAO userDAO, IBidDAO bidDAO)
        {
            _auctionDAO = auctionDAO;
            _userDAO = userDAO;
            _bidDAO = bidDAO;
            _backgroundJobClient = backgroundJobClient;
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
            return _auctionDAO.GetAuctionJoined(bidderId).Where(a => a.Status == status).ToList();
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

        public void StaffSetAuctionTime(int id, DateTime registrationStart, DateTime registrationEnd, DateTime startedAt, DateTime endedAt)
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
                _auctionDAO.UpdateAuction(auction);
            }
        }

        public void HostAuction(int auctionId, int status) {
            var auction = GetAuctionById(auctionId);

            if (auction == null) {
                return;
            }

            auction.Status = status;
            _auctionDAO.UpdateAuction(auction);
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
