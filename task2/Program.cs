using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace task2
{
    public class Program
    {
	    
        const int N = 10;
        const UInt32 F16 = 0xFFFF;

        static UInt64 msg = 0x123456789ABCDEF0;
        static UInt16 K = 0x96EA;
        
        static UInt64 IV = 0x18FD47203C7A23BC; 	// инициализационный вектор
        const int B = 4; // число блоков в исходном сообщении

        private static bool STEP = false;
        private static bool INPUT = true;
        
        static void log(String str, bool isLog)
        {
            if (isLog)
                Console.WriteLine(str + "\n");
        }

        static UInt16 right16(UInt16 x, int t)
        {
            return (UInt16)((x >> t) | (x << (16 - t)));
        }

        static UInt16 left16(UInt16 x, int t)
        {
            return (UInt16)((x << t) | (x >> (16 - t)));
        }

        static UInt16 Generate16Key(int i)
        {
            return right16(K, i * 4);
        }

        static UInt16 F(UInt16 sub_block1, UInt16 sub_block2, UInt16 K_i)
        {
            UInt16 b1 = (UInt16)(K_i | left16(sub_block1, 5));
            UInt16 b2 = (UInt16)(K_i | right16(sub_block2, 3));
            return (UInt16)(b1 ^ b2);
        }

        static UInt16 GetSubBlock16(UInt64 blok, int index)
        {
            return (UInt16)((blok >> 16 * (4 - index)) & F16);
        }

        static UInt64 shifr(UInt64 blok)
        {
            UInt16 x1 = GetSubBlock16(blok, 1);
            UInt16 x2 = GetSubBlock16(blok, 2);
            UInt16 x3 = GetSubBlock16(blok, 3);
            UInt16 x4 = GetSubBlock16(blok, 4);

            for (int i = 0; i < N; i++)
            {
                UInt16 K_i = Generate16Key(i);

                UInt16 F_i = F(x1, x2,  K_i);

                UInt16 x1_i = x1;
                UInt16 x2_i = x2;
                UInt16 x3_i = (UInt16)(x3 ^ F_i);
                UInt16 x4_i = (UInt16)(x4 ^ F_i);

                log(String.Format("in {0} x1 = {1:X}; x2 = {2:X}; x3 = {3:X}; x4 = {4:X};", i, x1, x2, x3, x4), STEP);
                if (i < N - 1)
                {
                    x1 = x2_i; x2 = x3_i; x3 = x4_i; x4 = x1_i;
                }
                else 
                {
                    x1 = x1_i; x2 = x2_i; x3 = x3_i; x4 = x4_i;
                }
                log(String.Format("out {0} x1 = {1:X}; x2 = {2:X}; x3 = {3:X}; x4 = {4:X};", i, x1, x2, x3, x4), STEP);

            }
            
            UInt64 shifroblok = x1; 
            shifroblok = (shifroblok << 16) | (x2 & F16);
            shifroblok = (shifroblok << 16) | (x3 & F16);
            shifroblok = (shifroblok << 16) | (x4 & F16);
            return shifroblok;
        }

        // Расшифровка 64 разрядного блока
        static UInt64 rasshifr(UInt64 blok)
        {
            UInt16 x1 = GetSubBlock16(blok, 1);
            UInt16 x2 = GetSubBlock16(blok, 2);
            UInt16 x3 = GetSubBlock16(blok, 3);
            UInt16 x4 = GetSubBlock16(blok, 4);
            
            for (int i = N - 1; i >= 0; i--)
            {
                UInt16 K_i = Generate16Key(i);
               
                UInt16 F_i = F(x1, x2,  K_i);

                UInt16 x1_i = x1;
                UInt16 x2_i = x2;
                UInt16 x3_i = (UInt16)(x3 ^ F_i);
                UInt16 x4_i = (UInt16)(x4 ^ F_i);
                
                log(String.Format("in {0} x1 = {1:X}; x2 = {2:X}; x3 = {3:X}; x4 = {4:X};", i, x1, x2, x3, x4), STEP);
                // Console.WriteLine("in {0} x1 = {1:X}; x2 = {2:X}; x3 = {3:X}; x4 = {4:X};", i, x1, x2, x3, x4);
                if (i > 0)
                {
                    x1 = x4_i; x2 = x1_i; x3 = x2_i; x4 = x3_i;
                }
                else
                {
                    x1 = x1_i; x2 = x2_i; x3 = x3_i; x4 = x4_i;
                }
                log(String.Format("out {0} x1 = {1:X}; x2 = {2:X}; x3 = {3:X}; x4 = {4:X};", i, x1, x2, x3, x4), STEP);

            }

            UInt64 shifroblok = x1; 
            shifroblok = (shifroblok << 16) | (x2 & F16);
            shifroblok = (shifroblok << 16) | (x3 & F16);
            shifroblok = (shifroblok << 16) | (x4 & F16);
            return shifroblok;
        }

        public static List<UInt64> ToList(byte[] bytes)
        {
            var list = new List<UInt64>();
            for (int i = 0; i < bytes.Length; i += sizeof(UInt64))
                list.Add(BitConverter.ToUInt64(bytes, i));

            return list;
        }

        public static byte[] ToBytes(List<UInt64> list)
        {
            var byteList = list.ConvertAll(new Converter<UInt64, byte[]>(Int64Converter));
            List<byte> resultList = new List<byte>();

            byteList.ForEach(x => { resultList.AddRange(x); });
            return resultList.ToArray();
        }

        public static byte[] Int64Converter(UInt64 x)
        {
            return BitConverter.GetBytes(x);
        }
        
        public static byte[] GetSafeBites(String str)
        {
            int strl = str.Length % 8;
            if (strl > 0)
            {
                for (int i = 0; i < 8 - strl - 1; i++)
                {
                    str += " ";
                }
            }
            return Encoding.Default.GetBytes(str);
            
        }

        public static void CBC(List<UInt64> ints)
        {
            List<UInt64> encoded = new List<UInt64>();
            UInt64 blok = ints[0] ^ IV;
            encoded.Add(shifr(blok));
            for (int b = 1; b < ints.Count; b++)
            {
                blok = ints[b] ^ encoded[b - 1];
                encoded.Add(shifr(blok));
            }
            log(Encoding.Default.GetString(ToBytes(encoded)), INPUT);
            
            List<UInt64> result = new List<UInt64>();
            UInt64 msg_b;
            msg_b = rasshifr(encoded[0]); 
            msg_b ^= IV;
            result.Add(msg_b);
            for (int b = 1; b < ints.Count; b++)
            {
                msg_b = rasshifr(encoded[b]);	
                msg_b ^= encoded[b - 1]; 		
                result.Add(msg_b);
            }
            log(Encoding.Default.GetString(ToBytes(result)), INPUT);
        }
        
        public static void CFB(List<UInt64> ints)
        {
            List<UInt64> encoded = new List<UInt64>();
            
            encoded.Add(ints[0] ^ shifr(IV));
            for (int b = 1; b < ints.Count; b++)
            {
                encoded.Add(ints[b] ^ shifr(encoded[b - 1]));
            }
            
            log(Encoding.Default.GetString(ToBytes(encoded)), INPUT);
            
            List<UInt64> result = new List<UInt64>();
            result.Add(encoded[0] ^ shifr(IV));
            for (int b = 1; b < ints.Count; b++)
            {
                result.Add(encoded[b]^shifr(encoded[b-1]));	
            }
            log(Encoding.Default.GetString(ToBytes(result)), INPUT);
        }

        public static void Main(string[] args)
        {
            String input = File.ReadAllText(@"/Users/andrejtarasov/Desktop/test.txt");

            log(input, INPUT);
            
            log(String.Format("Init Key {0:X}", K), INPUT);
            log(String.Format("Init Key {0:X}", IV), INPUT);
            
            
            List<UInt64> ints = ToList(GetSafeBites(input));
            CBC(ints);
            CFB(ints);
        }
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
	    
    }
}