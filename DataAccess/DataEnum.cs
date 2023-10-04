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

    public enum ProductType
    {
        ForSale = 1,
        Auction = 2,
    }

    public enum Condition
    {
        Mint = 1,
        NearMint = 2,
        VeryFine = 3,
        Good = 4,
        Poor = 5,
    }

    public enum AuctionStatus
    {
        Unavailable = 0,
        Unassigned = 1,
        Pending = 2,
        Rejected = 3,
        Approved = 4,
        Opened = 5,
        Ended = 6,
        Sold = 7,
        Expired = 8,
    }
}
