using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Implement
{
    public class WalletDAO : DataAccessBase<Wallet>, IWalletDAO
    {
        public WalletDAO(PlatformAntiquesHandicraftsContext context) : base(context) { }

        public Wallet GetByCurrentUser(int id)
        {
            return GetAll().Where(w => w.Id == id && w.Status == (int)Status.Available).FirstOrDefault();
        }
    }
}
