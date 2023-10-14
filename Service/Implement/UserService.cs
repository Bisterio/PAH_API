using BCrypt.Net;
using DataAccess;
using DataAccess.Models;
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
        private ITokenService _tokenService;
        private readonly int WORK_FACTOR = 13;
        private static readonly string DEFAULT_AVT = "https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg";

        public UserService(IUserDAO userDAO, ITokenDAO tokenDAO, ITokenService tokenService, IBuyerDAO buyerDAO, IWalletDAO walletDAO) {
            _userDAO = userDAO;
            _tokenDAO = tokenDAO;
            _tokenService = tokenService;
            _buyerDAO = buyerDAO;
            _walletDAO = walletDAO;
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

        public void Deactivate(User user)
        {
            user.Status = (int)Status.Unavailable;
            _userDAO.Deactivate(user);
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
                dbToken.ExpiryTime = DateTime.Now.AddMinutes(1);
                _tokenDAO.UpdateToken(dbToken);
            } else {
                _tokenDAO.Add(new Token { Id = id, RefreshToken = token.RefreshToken, ExpiryTime = DateTime.Now.AddMinutes(1) });
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

        public List<User> GetAllStaffs()
        {
            return _userDAO.GetAll().Where(u => u.Status == (int)Status.Available && u.Role == (int)Role.Staff).ToList();
        }
    }
}
