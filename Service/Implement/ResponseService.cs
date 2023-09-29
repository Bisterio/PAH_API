using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class ResponseService : IResponseService
    {
        private readonly IResponseDAO _responseDAO;

        public ResponseService(IResponseDAO responseDAO)
        {
            _responseDAO = responseDAO;
        }

        public Response GetByFeedbackId(int feedbackId)
        {
            return _responseDAO.GetByFeedbackId(feedbackId);
        }

        public void Reply(Response response)
        {
            response.Timestamp = DateTime.Now;
            _responseDAO.CreateResponse(response);
        }
    }
}
