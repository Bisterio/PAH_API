using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement {
    public class AddressDAO : DataAccessBase<Address>, IAddressDAO {
        public AddressDAO(PlatformAntiquesHandicraftsContext context) : base(context) {
        }

        public Address Get(int id) {
            return GetAll().FirstOrDefault(p => p.Id == id);
        }

        public IQueryable<Address> GetByCustomerId(int id) {
            return GetAll().Where(p => p.CustomerId == id);
        }
    }
}
