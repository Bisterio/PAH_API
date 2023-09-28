﻿using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class UserDAO : DataAccessBase<User>, IUserDAO {
        public UserDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public User Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id && p.Status == (int) Status.Available);
        }

        public User GetByEmail(string email) {
            return GetAll().FirstOrDefault(p => p.Email.Equals(email) && p.Status == (int) Status.Available);
        }

        public void Register(User user) {
            Create(user);
        }

        IQueryable<User> IUserDAO.GetAll() {
            return GetAll();
        }
    }
}