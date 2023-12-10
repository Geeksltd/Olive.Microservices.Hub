using System;
using System.Collections.Generic;
using System.Linq;
using Olive;

namespace Olive.Microservices.Hub
{
    internal class HubEncoder
    {
        public static Guid ConvertStringToGuid(string inputStr)
        {
            var result = Guid.NewGuid();
            if (inputStr.IsEmpty()) return result;

            try
            {
                var hashedString = HashString(inputStr);

                var hashed32 = Convert256To32(hashedString);

                var hashed32ToString = Convert32ToString(hashed32);

                var guidString = HexToGuid(hashed32ToString);

                result = guidString.To<Guid>();
            }
            catch
            {
                // ignored
            }

            return result;
        }

        static string HexToGuid(string hexString)
        {
            return $"{hexString.Substring(0, 8)}-{hexString.Substring(8, 4)}-{hexString.Substring(12, 4)}-{hexString.Substring(16, 4)}-{hexString.Substring(20, 12)}";
        }

        static string Convert32ToString(List<byte> input)
        {
            var result = string.Empty;

            input.ForEach(x => result += Byte2Hex(x));

            return result;
        }

        static char Byte2Hex(byte input)
        {
            var hexNum = input % 16;
            var result = (char)0;

            switch (hexNum)
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

        static List<byte> Convert256To32(List<int> input)
        {
            var result = new List<byte>();

            for (var i = 0; i < 256; i += 8)
            {
                var temp = (i % 2 == 0) ? input.GetRange(i, 8).Min() : input.GetRange(i, 8).Max();
                result.Add((byte)(temp % 16));
            }

            return result;
        }

        static List<int> HashString(string input)
        {
            var hashtable = new List<int>();
            for (var i = 0; i < 256; ++i) hashtable.Add(-1);

            var toByteArray = ConvertStr2Byte(input);

            for (var i = 0; i < toByteArray.Count; ++i) hashtable[toByteArray[i]] = 1 + i;

            while (hashtable.Contains(-1))
            {
                for (var i = 0; i < hashtable.Count; ++i)
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