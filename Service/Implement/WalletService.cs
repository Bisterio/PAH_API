using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Request.ThirdParty.Zalopay;

namespace Service.Implement
{
    public class WalletService : IWalletService
    {
        private readonly IWalletDAO _walletDAO;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IOrderDAO _orderDAO;
        private readonly IHttpClientFactory _httpClientFactory;

        public WalletService(IWalletDAO walletDAO, ITransactionDAO transactionDAO, IHttpClientFactory httpClientFactory, IOrderDAO orderDAO) {
            _walletDAO = walletDAO;
            _transactionDAO = transactionDAO;
            _httpClientFactory = httpClientFactory;
            _orderDAO = orderDAO;
        }

        public async Task Topup(int userId, TopupRequest topupRequest) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Wallet not found");
            }

            //Check transaction from zalopay in committed in db
            if (!_transactionDAO.IsZalopayOrderValid(topupRequest.AppTransId, topupRequest.Mac)) {
                throw new Exception("409: Order from zalopay is invalid");
            }

            //Uncomment when zalopay fix their api

            ////Call to zalopay
            //var httpClient = _httpClientFactory.CreateClient("Zalopay");
            //var data = new QueryRequest {
            //    app_id = orderRequest.AppId,
            //    app_trans_id = orderRequest.AppTransId,
            //    mac = orderRequest.Mac
            //};
            //var httpResponseMessage = await httpClient.PostAsync("query", Utils.ConvertForPost<QueryRequest>(data));

            //if (!httpResponseMessage.IsSuccessStatusCode) {
            //    throw new Exception("400: No order in zalo pay yet");
            //}

            ////Validation
            //var responseData = await httpResponseMessage.Content.ReadAsAsync<QueryResponse>();
            //if (responseData.return_code != 1) {
            //    throw new Exception("400: " + responseData.return_message);
            //}
            //if (responseData.amount != orderRequest.Topup) {
            //    throw new Exception("400: Amount does not match with order from zalopay");
            //}

            //Topup and create transaction
            wallet.AvailableBalance += topupRequest.Topup;
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new DataAccess.Models.Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Zalopay,
                Amount = topupRequest.Topup,
                Type = (int) TransactionType.Deposit,
                Date = DateTime.Now,
                Description = $"app_id: {topupRequest.AppId}, " +
                    $"app_trans_id: {topupRequest.AppTransId}, " +
                    $"mac: {topupRequest.Mac}",
                Status = (int) Status.Available
            });
        }

        public void AddLockedBalance(int userId, decimal balance) {
            throw new NotImplementedException();
        }

        public void SubtractLockedBalance(int userId, decimal balance) {
            throw new NotImplementedException();
        }

        public void CheckoutWallet(int userId, int orderId, int orderStatus) {
            var wallet = _walletDAO.Get(userId);
            if (wallet == null) {
                throw new Exception("404: Wallet not found");
            }

            var order = _orderDAO.Get(orderId);
            if (order == null) {
                throw new Exception("404: Order not found when checkout");
            }

            if (wallet.AvailableBalance < order.TotalAmount + order.ShippingCost) {
                throw new Exception($"400: Not enough balance in wallet to confirm order: {orderId}");
            }

            //Subtract from wallet, create transaction, update order
            wallet.AvailableBalance -= (order.TotalAmount + order.ShippingCost);
            _walletDAO.Update(wallet);
            _transactionDAO.Create(new DataAccess.Models.Transaction {
                Id = 0,
                WalletId = wallet.Id,
                PaymentMethod = (int) PaymentType.Wallet,
                Amount = order.TotalAmount + order.ShippingCost,
                Type = (int) TransactionType.Payment,
                Date = DateTime.Now,
                Description = $"Payment for order: {orderId}",
                Status = (int) Status.Available
            });
            //Update order status to Waiting for Seller Confirm
            order.Status = orderStatus;
            _orderDAO.UpdateOrder(order);
        }

        public Wallet GetByCurrentUser(int id)
        {
            return _walletDAO.GetByCurrentUser(id);
        }

        //public void CheckoutZalopay(int userId, int orderId, OrderRequest orderRequest) {
        //    throw new NotImplementedException();
        //}
    }
}
