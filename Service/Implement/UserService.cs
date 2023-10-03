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
        private ITokenService _tokenService;
        private readonly int WORK_FACTOR = 13;

        public UserService(IUserDAO userDAO, ITokenDAO tokenDAO, ITokenService tokenService, IBuyerDAO buyerDAO) {
            _userDAO = userDAO;
            _tokenDAO = tokenDAO;
            _tokenService = tokenService;
            _buyerDAO = buyerDAO;
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

        public User Login(string email, string password) {
            var user = _userDAO.GetByEmail(email);
            if (user != null) {
                var verifyPassword = BC.EnhancedVerify(password, user.Password);
                if (!verifyPassword) return null;
                var buyer = _buyerDAO.Get(user.Id);
                if (buyer == null) {
                    _buyerDAO.Create(new Buyer { Id = user.Id, JoinedAt = DateTime.Now });
                }
            }
            return user;
        }

        public void Register(User user) {
            user.Password = BC.EnhancedHashPassword(user.Password, WORK_FACTOR);
            user.Role = (int) Role.Buyer;
            user.ProfilePicture = "To be Implemented";
            user.Status = (int) Status.Available;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            _userDAO.Register(user);
            var newUser = _userDAO.GetByEmail(user.Email);

            if (newUser == null) throw new Exception("500: Cannot insert new user");
            _buyerDAO.Create(new Buyer { Id = newUser.Id, JoinedAt = DateTime.Now });
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
    }
}
