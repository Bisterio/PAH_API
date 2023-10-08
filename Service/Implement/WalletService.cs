using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class WalletService : IWalletService
    {
        private readonly IWalletDAO _walletDAO;

        public WalletService(IWalletDAO walletDAO)
        {
            _walletDAO = walletDAO;
        }

        public Wallet GetByCurrentUser(int id)
        {
            return _walletDAO.GetByCurrentUser(id);
        }
    }
}
