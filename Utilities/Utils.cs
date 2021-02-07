using System;
using System.Linq;

namespace quizter_be.Utilities
{
    public static class Utils
    {
        public static string RandomString(int length = 4)
        {
            var random = new Random();
            string pool = "abcdefghijklmnopqrstuvwxyz".ToUpper();
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
    }
}