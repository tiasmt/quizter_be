
using System.Collections.Generic;
using mtquiz_be.Models;

namespace quizter_be.Models
{
    public class Question
    {
        public string Body { get; set; }
        public List<Answer> Answers { get; set; }
        public int QuestionId { get; set; }
    }
}