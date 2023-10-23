using BCrypt.Net;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Request;
using Service.EmailService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class UserService : IUserService {
        private readonly IUserDAO _userDAO;
        private readonly ITokenDAO _tokenDAO;
        private readonly IBuyerDAO _buyerDAO;
        private readonly IWalletDAO _walletDAO;
        private readonly ISellerDAO _sellerDAO;
        private readonly ITokenService _tokenService;
        private readonly int WORK_FACTOR = 13;
        private static readonly string DEFAULT_AVT = "https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg";

        public UserService(IUserDAO userDAO, ITokenDAO tokenDAO, 
            ITokenService tokenService, IBuyerDAO buyerDAO, 
            IWalletDAO walletDAO, ISellerDAO sellerDAO) {
            _userDAO = userDAO;
            _tokenDAO = tokenDAO;
            _tokenService = tokenService;
            _buyerDAO = buyerDAO;
            _walletDAO = walletDAO;
            _sellerDAO = sellerDAO;
        }

        public User Get(int id) {
            return _userDAO.Get(id);
        }

        public List<User> GetAll() {
            return _userDAO.GetAll().ToList();
        }

        public User GetByEmail(string email) {
            return _userDAO.GetByEmail(email);
        }

        public List<User> GetAllStaffs()
        {
            return _userDAO.GetAll().Where(u => u.Status == (int)Status.Available && u.Role == (int)Role.Staff).ToList();
        }

        public List<User> GetAllBuyersSellers()
        {
            return _userDAO.GetAll().Where(u => u.Status == (int)Status.Available
            && (u.Role == (int)Role.Buyer || u.Role == (int)Role.Seller)).ToList();
        }

        public void Deactivate(User user)
        {
            if(user.Status == (int)Status.Unavailable)
            {
                throw new Exception("400: This user has already deactivated");
            }
            user.Status = (int)Status.Unavailable;
            _userDAO.Deactivate(user);
        }
        public void Reactivate(User user)
        {
            if (user.Status == (int)Status.Available)
            {
                throw new Exception("400: This user hasn't been deactivated");
            }
            user.Status = (int)Status.Available;
            _userDAO.Deactivate(user);
        }

        public void AcceptSeller(Seller seller)
        {
            if(seller.Status != (int)SellerStatus.Pending)
            {
                throw new Exception("400: This seller request has been approved");
            }
            seller.Status = (int)SellerStatus.Available;
            _sellerDAO.UpdateSeller(seller);
        }

        public void RejectSeller(Seller seller)
        {
            if (seller.Status != (int)SellerStatus.Pending)
            {
                throw new Exception("400: This seller request has been approved");
            }
            seller.Status = (int)SellerStatus.Unavailable;
            _sellerDAO.UpdateSeller(seller);
        }

        public List<User> GetAvailableStaffs()
        {
            var availableStaffs = _userDAO.GetAll()
                .Where(u => u.Role == (int)Role.Staff 
                && u.Auctions.All(a => a.Status != (int)AuctionStatus.Assigned 
                && a.Status != (int)AuctionStatus.RegistrationOpen 
                && a.Status != (int)AuctionStatus.Opened))
                .ToList();
            return availableStaffs;
        }

        public List<User> GetReactivateRequestList()
        {
            return _userDAO.GetAll().Where(u => u.Status == (int)Status.Unavailable).ToList();
        }

        public User Login(string email, string password) {
            var user = _userDAO.GetByEmail(email);
            if (user != null) {
                var verifyPassword = BC.EnhancedVerify(password, user.Password);
                if (!verifyPassword) return null;

                //Create buyer
                var buyer = _buyerDAO.Get(user.Id);
                if (buyer == null) {
                    _buyerDAO.Create(new Buyer { Id = user.Id, JoinedAt = DateTime.Now });
                }

                //Create wallet
                var wallet = _walletDAO.GetWithoutStatus(user.Id);
                if (wallet == null) {
                    _walletDAO.Create(new Wallet { Id = user.Id, AvailableBalance = 0, LockedBalance = 0, Status = (int) Status.Available });
                }
            }
            return user;
        }

        public void Register(User user) {
            var dbUser = _userDAO.GetByEmail(user.Email);
            if (dbUser != null) {
                throw new Exception("409: Email already exists");
            }

            user.Password = BC.EnhancedHashPassword(user.Password, WORK_FACTOR);
            user.Role = (int) Role.Buyer;
            user.Status = (int) Status.Available;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            user.ProfilePicture = DEFAULT_AVT;
            _userDAO.Register(user);
            var newUser = _userDAO.GetByEmail(user.Email);

            if (newUser == null) throw new Exception("500: Cannot insert new user");

            //Create buyer
            CreateBuyer(newUser.Id);

            //Create Wallet
            CreateWallet(newUser.Id);
        }

        private void CreateBuyer(int userId) {
            var buyer = _buyerDAO.Get(userId);
            if (buyer == null) {
                _buyerDAO.Create(new Buyer { Id = userId, JoinedAt = DateTime.Now });
            } else {
                buyer.JoinedAt = DateTime.Now;
                _buyerDAO.Update(buyer);
            }
        }

        private void CreateWallet(int userId) {
            var wallet = _walletDAO.GetWithoutStatus(userId);
            if (wallet == null) {
                _walletDAO.Create(new Wallet { Id = userId, AvailableBalance = 0, LockedBalance = 0, Status = (int) Status.Available });
            } else {
                wallet.AvailableBalance = 0;
                wallet.LockedBalance = 0;
                wallet.Status = (int) Status.Available;
                _walletDAO.Update(wallet);
            }
        }

        public Tokens AddRefreshToken(int id) {
            var token = new Tokens { AccessToken = _tokenService.GenerateAccessToken(id), RefreshToken = _tokenService.GenerateRefreshToken() };
            var dbToken = _tokenDAO.Get(id);
            if (dbToken != null) {
                dbToken.RefreshToken = token.RefreshToken;
                dbToken.ExpiryTime = DateTime.Now.AddDays(7);
                _tokenDAO.UpdateToken(dbToken);
            } else {
                _tokenDAO.Add(new Token { Id = id, RefreshToken = token.RefreshToken, ExpiryTime = DateTime.Now.AddDays(7) });
            }
            return token;
        }

        public Token GetSavedRefreshToken(int id, string refreshToken) {
            return _tokenDAO.GetSavedRefreshToken(id, refreshToken);
        }

        public void RemoveRefreshToken(int id) {
            var token = _tokenDAO.Get(id);
            if (token != null) {
                token.RefreshToken = null;
                token.ExpiryTime = null;
                _tokenDAO.UpdateToken(token);
            }
        }

        public void AddResetToken(int id, string token) {
            var dbToken = _tokenDAO.Get(id);
            if (dbToken != null) {
                dbToken.RefreshToken = token;
                dbToken.ExpiryTime = DateTime.Now.AddMinutes(30);
                _tokenDAO.UpdateToken(dbToken);
            } else {
                _tokenDAO.Add(new Token { Id = id, RefreshToken = token, ExpiryTime = DateTime.Now.AddMinutes(30) });
            }
        }

        public void ResetPassword(ResetPasswordRequest request) {
            var user = _userDAO.GetByEmail(request.Email);
            if (user == null) {
                throw new Exception("404: User not found when reset password");
            }

            var token = _tokenDAO.GetResetToken(user.Id, request.Token, DateTime.Now);
            if (token == null) {
                throw new Exception("401: Token not correct");
            }

            user.Password = BC.EnhancedHashPassword(request.Password, WORK_FACTOR);
            user.UpdatedAt = DateTime.Now;
            _userDAO.Update(user);

            token.RefreshToken = null;
            _tokenDAO.UpdateToken(token);
        }

        public void UpdateProfile(int id, UpdateProfileRequest request) {
            var user = _userDAO.Get(id);
            if (user == null) {
                throw new Exception("404: User not found");
            }

            user.Name = request.Name;
            user.Password = BC.EnhancedHashPassword(request.Password, WORK_FACTOR);
            user.Phone = request.Phone;
            user.ProfilePicture = request.ProfilePicture;
            user.Gender = request.Gender;
            user.Dob = request.Dob;
            user.UpdatedAt = DateTime.Now;
            _userDAO.Update(user);
        }
    }
}
