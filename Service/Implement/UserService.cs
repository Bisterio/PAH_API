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
        private readonly int WORK_FACTOR = 13;

        public UserService(IUserDAO userDAO) {
            _userDAO = userDAO;
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
            }
            return user;
        }

        public void Register(User user) {
            user.Password = BC.EnhancedHashPassword(user.Password, WORK_FACTOR);
            user.Role = (int) Role.Buyer;
            user.ProfilePicture = "To be Implemented";
            user.Status = (int) Status.Available;
            _userDAO.Register(user);
        }
    }
}
