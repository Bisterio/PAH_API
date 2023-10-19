using DataAccess;
using DataAccess.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service;

namespace API.Hubs
{

    public class AuctionHub : Hub
    {
        private readonly IAuctionService _auctionService;

        public AuctionHub(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        [Authorize]
        public async override Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            List<Auction> auctionList = _auctionService.GetAuctionJoined(userId);
            // Add user to room
            foreach (var auction in auctionList)
            {
                if (auction.Status == (int)AuctionStatus.RegistrationOpen || auction.Status == (int)AuctionStatus.Opened)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "AUCTION_" + auction.Id);
                    await Clients.Group("AUCTION_" + auction.Id).SendAsync("ReceiveMessage", userId, "You have been added to group " + "AUCTION_" + auction.Id);
                }
            }
            await base.OnConnectedAsync();
        }

        [Authorize]
        public async Task SendMessage(string user, string message)
        {
            var userId = int.Parse(Context.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }

        public async Task JoinGroup(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AUCTION_" + auctionId);
            await Clients.Group("AUCTION_" + auctionId).SendAsync("ReceiveMessage", "SYSTEM", "You have been added to group " + "AUCTION_" + auctionId);
        }
    }

}