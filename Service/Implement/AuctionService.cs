using DataAccess;
using DataAccess.Models;
using Hangfire;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuctionService (IAuctionDAO auctionDAO, IBackgroundJobClient backgroundJobClient)
        {
            _auctionDAO = auctionDAO;
            _backgroundJobClient = backgroundJobClient;
        }

        public List<Auction> GetAuctions(string? title, int status, int categoryId, int materialId, int orderBy)
        {
            List<Auction> auctionList;
            try
            {
                var auctions = _auctionDAO.GetAuctions()
                    .Where(a => status == 0 || a.Status == status
                    //&& a.Product.SellerId. == (int)Status.Available
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                    && (materialId == 0 || a.Product.MaterialId == materialId)
                    && (categoryId == 0 || a.Product.CategoryId == categoryId));

                //default (0): old -> new, 1: started at asc, 2: unknown, 3: unknown
                switch (orderBy)
                {
                    case 1:
                        auctions = auctions.OrderBy(a => a.StartedAt);
                        break;
                    //case 2:
                    //    auctions = auctions.OrderByDescending(p => p.StartedAt);
                    //    break;
                    //case 3:
                    //    auctions = auctions.OrderBy(p => p.Price);
                    //    break;
                    default:
                        auctions = auctions.OrderByDescending(a => a.StartedAt);
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

        public List<Auction> GetAuctionBySellerId(int sellerId)
        {
            if (sellerId == null)
            {
                throw new Exception("404: Seller not found");
            }
            return _auctionDAO.GetAuctionBySellerId(sellerId).ToList();
        }

        public List<Auction> GetAuctionJoined(int bidderId)
        {
            if (bidderId == null)
            {
                throw new Exception("404: Bidder not found");
            }
            return _auctionDAO.GetAuctionJoined(bidderId).ToList();
        }

        public void CreateAuction(Auction auction)
        {
            auction.EntryFee = 0.1m * auction.StartingPrice;
            auction.StaffId = null;
            auction.Status = (int) AuctionStatus.Unassigned;
            auction.CreatedAt = DateTime.Now;
            auction.UpdatedAt = DateTime.Now;
            _auctionDAO.CreateAuction(auction);
        }

        public void StaffApproveAuction(int id)
        {
            Auction auction = GetAuctionById(id);

            if(auction.Status == (int) AuctionStatus.Unassigned)
            {
                throw new Exception("400: This auction is unassigned.");
            }
            else if (auction.Status == (int) AuctionStatus.Pending)
            {
                auction.Status = (int)AuctionStatus.Approved;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
                //Schedule task to open/end auction
                _backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Opened), auction.StartedAt.Value);
                _backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Ended), auction.EndedAt.Value);
            } 
            else
            {
                throw new Exception("400: This auction is already approved.");
            }
        }

        public void StaffRejectAuction(int id)
        {
            Auction auction = GetAuctionById(id);

            if (auction.Status == (int)AuctionStatus.Unassigned)
            {
                throw new Exception("400: This auction is unassigned.");
            }
            else if (auction.Status == (int)AuctionStatus.Pending)
            {
                auction.Status = (int)AuctionStatus.Rejected;
                auction.UpdatedAt = DateTime.Now;
                _auctionDAO.UpdateAuction(auction);
            }
            else
            {
                throw new Exception("400: This auction cannot be rejected.");
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

        //public void TestSchedule() {
        //    var auction = _auctionDAO.GetAuctionById(3);

        //    auction.StartedAt = DateTime.Now.AddMinutes(1);
        //    auction.EndedAt = DateTime.Now.AddMinutes(2);
        //    _auctionDAO.UpdateAuction(auction);
        //    _backgroundJobClient.Schedule(() => HostAuction(auction.Id, (int) AuctionStatus.Opened), auction.StartedAt.Value);
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
