using DataAccess;
using DataAccess.Models;
using Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IAuctionService
    {
        public List<Auction> GetAuctions(string? title, int status, int categoryId, int materialId, int orderBy);
        public List<Auction> GetAllAuctions(string? title, int status, int categoryId, int materialId, int orderBy);
        public Auction GetAuctionById(int id);
        public List<Auction> GetAuctionAssigned(int staffId);
        public List<Auction> GetAuctionsByProductId(int productId);
        public List<Auction> GetAuctionJoined(int bidderId);
        public List<Auction> GetAuctionJoinedByStatus(int status, int bidderId);
        public List<Auction> GetAuctionBySellerId(int sellerId, int status);
        public Bid EndAuction(int auctionId);
        public void CreateAuction(Auction auction);
        public void AssignStaff(int id, int staffId);
        public void ManagerApproveAuction(int id);
        public void ManagerRejectAuction(int id);
        public void StaffSetAuctionTime(int id, DateTime registrationStart, DateTime registrationEnd, DateTime startedAt, DateTime endedAt, decimal step);
        public bool HostAuction(int auctionId, int status);
        public bool CheckRegistration(int bidderId, int auctionId);
        public bool CheckWinner(int bidderId, int auctionId);
        //public void TestSchedule();
        //public void OpenAuction(int id);
        //public void EndAuction(int id);

        public void CreateAuctionOrder(int userId, AuctionOrderRequest request);
    }
}
