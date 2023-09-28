using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public enum Status {
        Available = 1,
        Unavailable = 0
    }

    public enum Role {
        Buyer = 1,
        Seller = 2,
        Administrator = 3,
        Manager = 4,
        Staff = 5
    }
}
