using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using DataAccess;
using DataAccess.Models;
using System.Net;

namespace Service.Implement {
    public class AddressService : IAddressService {
        private readonly IAddressDAO _addressDAO;

        public AddressService(IAddressDAO addressDAO) {
            _addressDAO = addressDAO;
        }

        public void Create(Address address) {
            address.Id = 0;
            address.IsDefault = false;
            _addressDAO.Create(address);
        }

        public void Delete(int addressId, int customerId) {
            var db = _addressDAO.Get(addressId);

            if (db == null) {
                throw new Exception("404: Address not found");
            }

            if (db.CustomerId != customerId) {
                throw new Exception("401: You are not allowed to delete this address");
            }

            if (db.Type == (int) AddressType.Pickup) {
                var listAddress = _addressDAO.GetByCustomerId(db.CustomerId.Value).Select(p => p.Type == (int) AddressType.Pickup);
                if (listAddress.Count() <= 1) { 
                    throw new Exception("400: You are not allowed to delete address: You do not have sufficient address"); 
                }
            }
            _addressDAO.Delete(db);
        }

        public Address Get(int addressId) {
            return _addressDAO.Get(addressId);
        }

        public List<Address> GetByCustomerId(int customerId) {
            return _addressDAO.GetByCustomerId(customerId).ToList();
        }

        public void Update(Address address, int customerId) {
            var db = _addressDAO.Get(address.Id);

            if (db == null) {
                throw new Exception("404: Address not found");
            }

            if (db.CustomerId != customerId) {
                throw new Exception("401: You are not allowed to update this address");
            }

            db.RecipientName = address.RecipientName;
            db.RecipientPhone = address.RecipientPhone;
            db.Province = address.Province;
            db.District = address.District;
            db.DistrictId = address.DistrictId;
            db.Ward = address.Ward;
            db.WardCode = address.WardCode;
            db.Street = address.Street;
            db.UpdatedAt = DateTime.Now;

            _addressDAO.Update(db);
        }
    }
}
