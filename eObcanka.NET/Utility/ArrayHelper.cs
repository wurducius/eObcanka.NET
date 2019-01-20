using System;
using System.Collections.Generic;
using System.Text;

namespace eObcanka.NET.Utility
{
   public class ArrayHelper
    {
        public bool Equality(byte[] a1, byte[] b1)
        {
            if (a1 == null || b1 == null)
                return false;
            int length = a1.Length;
            if (b1.Length != length)
                return false;
            while (length > 0)
            {
                length--;
                if (a1[length] != b1[length])
                    return false;
            }
            return true;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

    }
}
