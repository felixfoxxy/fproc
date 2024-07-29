using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace fproc
{
    public static class Utils
    {
        static Random rnd = new Random();
        public static string RandomString(int len)
        {
            string ret = "";
            for(int i = 0; i < len; i++)
            {
                char c = (char)rnd.Next(char.MinValue, char.MaxValue);
                while(!char.IsLetterOrDigit(c))
                    c = (char)rnd.Next(char.MinValue, char.MaxValue);
                ret += c;
            }
            return ret;
        }

        public static string Sha256(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public static void PrintColor(string txt, bool newLine = true, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black)
        {
            ConsoleColor of = Console.ForegroundColor;
            ConsoleColor ob = Console.BackgroundColor;
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
            if(newLine)
                Console.WriteLine(txt);
            else
                Console.Write(txt);
            Console.ForegroundColor = of;
            Console.BackgroundColor = ob;
        }
    }
}
