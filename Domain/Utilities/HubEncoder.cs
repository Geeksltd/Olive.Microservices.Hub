using System;
using System.Collections.Generic;
using System.Linq;
using Olive;

namespace Olive.Microservices.Hub
{
    internal class HubEncoder
    {
        public static Guid ConvertStringToGuid(string inputstr)
        {
            var result = Guid.NewGuid();
            if (inputstr.IsEmpty()) return result;

            try
            {
                var hashedstring = HashString(inputstr);

                var hashed32 = Conver256to32(hashedstring);

                var hashed32tostring = Convert32toString(hashed32);

                var guidstring = HexToGuid(hashed32tostring);

                result = guidstring.To<Guid>();
            }
            catch
            {

            }
            return result;
        }

        static string HexToGuid(string hexstring)
        {
            return $"{hexstring.Substring(0, 8)}-{hexstring.Substring(8, 4)}-{hexstring.Substring(12, 4)}-{hexstring.Substring(16, 4)}-{hexstring.Substring(20, 12)}";
        }

        static string Convert32toString(List<byte> input)
        {
            var result = String.Empty;

            input.ForEach(x => result += Byte2Hex(x));

            return result;
        }

        static char Byte2Hex(byte input)
        {
            var hexnum = input % 16;
            char result = (char)0;
            switch (hexnum)
            {
                case 1: result = '1'; break;
                case 2: result = '2'; break;
                case 3: result = '3'; break;
                case 4: result = '4'; break;
                case 5: result = '5'; break;
                case 6: result = '6'; break;
                case 7: result = '7'; break;
                case 8: result = '8'; break;
                case 9: result = '9'; break;
                case 10: result = 'A'; break;
                case 11: result = 'B'; break;
                case 12: result = 'C'; break;
                case 13: result = 'D'; break;
                case 14: result = 'E'; break;
                case 15: result = 'F'; break;
                default: result = '0'; break;
            }
            return result;
        }

        static List<byte> Conver256to32(List<int> input)
        {
            var result = new List<byte>();
            for (int i = 0; i < 256; i += 8)
            {
                var temp = (i % 2 == 0) ? input.GetRange(i, 8).Min() : input.GetRange(i, 8).Max();
                result.Add((byte)(temp % 16));
            }
            return result;
        }

        static List<int> HashString(string input)
        {
            var hashtable = new List<int>();
            for (int i = 0; i < 256; ++i) hashtable.Add(-1);

            var tobytearray = ConvertStr2Byte(input);

            for (int i = 0; i < tobytearray.Count; ++i) hashtable[tobytearray[i]] = 1 + i;

            while (hashtable.Contains(-1))
            {
                for (int i = 0; i < hashtable.Count; ++i)
                {
                    if (hashtable[i] == -1)
                    {
                        int temp;
                        if (i == 0)
                        {
                            temp = (hashtable[255] == -1 ? 0 : hashtable[255]) * 255 + (hashtable[254] == -1 ? 0 : hashtable[254]) * 254;
                        }
                        else if (i == 1)
                        {
                            temp = (hashtable[255] == -1 ? 0 : hashtable[255]) * 255;
                        }
                        else
                        {
                            temp = (hashtable[i - 1] == -1 ? 0 : hashtable[i - 1]) * (i - 1) + (hashtable[i - 2] == -1 ? 0 : hashtable[i - 2]) * (i - 2);
                        }
                        hashtable[i] = temp % 256;
                    }
                }
            }
            return hashtable;
        }

        static List<int> ConvertStr2Byte(string input)
        {
            var result = new List<int>();
            input.ToList().ForEach(chr => result.Add(Convert.ToByte(chr)));
            return result;
        }
    }
}
