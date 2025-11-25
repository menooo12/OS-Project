using System;
using System.Collections.Generic;

namespace OS_project
{
    public class Converter
    {
        public static byte[] StringToBytes(string str)
        {
            if (str == null)
                return new byte[0];

            return System.Text.Encoding.UTF8.GetBytes(str);
        }
        public static string BytesToString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "";

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}