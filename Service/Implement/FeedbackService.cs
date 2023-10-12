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
            if (id == null)
            {
                throw new Exception("404: Feedback not found");
            }
            return _feedbackDAO.GetById(id);
        }

        public List<Feedback> GetAll(int productId)
        {
            if(productId == null)
            {
                throw new Exception("404: Product not found");
            }
            return _feedbackDAO.GetAll(productId).ToList();
        }

        public List<Feedback> GetTop3Newest(int productId)
        {
            if (productId == null)
            {
                throw new Exception("404: Product not found");
            }
            return _feedbackDAO.GetAll(productId).Take(3).ToList();
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
