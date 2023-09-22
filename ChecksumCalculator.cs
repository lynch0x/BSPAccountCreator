using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BSPCreaor
{
    class ChecksumCalculator
    {
        private static HMACSHA256 sha = new HMACSHA256(Encoding.ASCII.GetBytes("7jA7^kAZSHtjxDAa"));
        public static string createChecksum(string gameId, string regionUpper, string password, string username, bool isGuest = false)
        {
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(gameId + regionUpper + password + username + isGuest.ToString().ToLower()))).Replace("-", "").ToLower();
        }
    }
}
