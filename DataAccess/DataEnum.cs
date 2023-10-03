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

    public enum OrderStatus {
        Pending = 1,
        ReadyForPickup = 2,
        Delivering = 3,
        Delivered = 4,
        CancelApprovalPending = 10,
        CancelledBySeller = 12,
        CancelledByBuyer = 11
    }

    public enum AddressType {
        Delivery = 1,
        Pickup = 2
    }
}
