using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackDAO _feedbackDAO;

        public FeedbackService(IFeedbackDAO feedbackDAO)
        {
            _feedbackDAO = feedbackDAO;
        }

        public Feedback GetById(int id)
        {
            return _feedbackDAO.GetById(id);
        }

        public List<Feedback> GetAll(int productId)
        {
            return _feedbackDAO.GetAll(productId).ToList();
        }

        public void CreateFeedback(Feedback feedback)
        {
            feedback.Status = (int)Status.Available;
            feedback.Timestamp = DateTime.Now;
            _feedbackDAO.CreateFeedback(feedback);
        }

        //public void Delete(Feedback feedback)
        //{
        //    feedback.Status = (int)Status.Unavailable;
        //    _feedbackDAO.Update(feedback);
        //}
    }
}
