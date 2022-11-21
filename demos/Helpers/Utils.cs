using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demos.Helpers
{
    public  static class Utils
    {
        public static string GenerateRandomString(int length)
        {
            var res = new Random();

            var str = "abcdefghijklmnopqrstuvwxyz0123456789";

            var randomstring = string.Empty;

            for (int i = 0; i < length; i++)
            {

                int x = res.Next(str.Length);

                randomstring = randomstring + str[x];
            }

            Console.WriteLine("Random alphanumeric String:" + randomstring);
            return randomstring;
        }
    }
}
