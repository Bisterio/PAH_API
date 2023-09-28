using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service {
    public interface IUserService {
        public User Get(int id);
        public List<User> GetAll();
        public User GetByEmail(string email);
        public User Login(string email, string password);
        public void Register(User user);
    }
}
