
namespace quizter_be.Models
{
    public class Game
    {
        public string Name { get; set; }
        public Category GameCategory { get; set; }

        public Settings GameSettings { get; set; }
        public int Players { get; set; }

    }
}