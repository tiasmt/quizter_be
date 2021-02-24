
namespace quizter_be.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public bool LastAnswerIsCorrect { get; set; }

    }
}