using System;
using System.Text;

namespace RumbleParty.Networking
{
    public class SystemCodeGenerator:IRoomCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        private readonly int _length;
        private readonly Random _random = new Random();
        
        public SystemCodeGenerator(int length = 5)
        {
            _length = length;
        }
        public string GenerateRoomCode()
        {
            var sb = new StringBuilder(_length);
            for (int i = 0; i < _length; i++)
                sb.Append(Alphabet[_random.Next(Alphabet.Length)]);
            return sb.ToString();
        }
    }
}