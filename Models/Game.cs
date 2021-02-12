
namespace quizter_be.Models
{
    public class Game
    {
        public string GameName { get; set; }
        public string GameCategory { get; set; }

        public Settings GameSettings { get; set; }
        public int Players { get; set; }

    }
}