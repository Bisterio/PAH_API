﻿using System;
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
            List<Address> addresses = new List<Address>();
            if (address.Type == (int)AddressType.Pickup)
            {
                addresses = _addressDAO.GetPickupByCustomerId((int)address.CustomerId).ToList();
                if (addresses.Count > 0)
                {
                    throw new Exception("400: You can only have 1 pickup address");
                }
                address.Id = 0;
                address.IsDefault = true;
                address.CreatedAt = DateTime.Now;
                address.UpdatedAt = DateTime.Now;
                _addressDAO.Create(address);
            }
            addresses = _addressDAO.GetDeliveryByCustomerId((int)address.CustomerId).ToList();
            foreach (Address existedAddress in addresses)
            {
                if (existedAddress.IsDefault == true)
                {
                    existedAddress.IsDefault = false;
                }
            }
            address.Id = 0;
            address.IsDefault = true;
            address.CreatedAt = DateTime.Now;
            address.UpdatedAt = DateTime.Now;
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

        public Address GetPickupBySellerId(int sellerId)
        {
            return _addressDAO.GetPickupByCustomerId(sellerId).FirstOrDefault();
        }

        public Address GetDeliveryByCurrentUser(int id)
        {
            return _addressDAO.GetByCustomerId(id)
                .Where(a => a.Type == (int)AddressType.Delivery && a.IsDefault == true)
                .FirstOrDefault();
        }

        public void Update(Address address, int customerId) {
            var db = _addressDAO.Get(address.Id);

            if (db == null) {
                throw new Exception("404: Address not found");
            }

            if (db.CustomerId != customerId) {
                throw new Exception("401: You are not allowed to update this address");
            }

            List<Address> addresses = new List<Address>();
            if (db.Type == (int)AddressType.Pickup)
            {
                db.RecipientName = address.RecipientName;
                db.RecipientPhone = address.RecipientPhone;
                db.Province = address.Province;
                db.ProvinceId = address.ProvinceId;
                db.District = address.District;
                db.DistrictId = address.DistrictId;
                db.Ward = address.Ward;
                db.WardCode = address.WardCode;
                db.Street = address.Street;
                db.UpdatedAt = DateTime.Now;
            }

            if(address.IsDefault == true)
            {
                addresses = _addressDAO.GetDeliveryByCustomerId(customerId).ToList();
                foreach (Address existedAddress in addresses)
                {
                    if (existedAddress.IsDefault == true)
                    {
                        existedAddress.IsDefault = false;
                    }
                }
            }

            db.RecipientName = address.RecipientName;
            db.RecipientPhone = address.RecipientPhone;
            db.Province = address.Province;
            db.ProvinceId = address.ProvinceId;
            db.District = address.District;
            db.DistrictId = address.DistrictId;
            db.Ward = address.Ward;
            db.WardCode = address.WardCode;
            db.Street = address.Street;
            db.IsDefault = address.IsDefault;
            db.UpdatedAt = DateTime.Now;

            _addressDAO.Update(db);
        }
    }
}