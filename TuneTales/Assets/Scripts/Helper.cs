using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Helper 
{
    public static string ConvertUtf8ToUtf16(string utf8_string)
    {
        byte[] utf8_bytes = Encoding.UTF8.GetBytes(utf8_string);
        return Encoding.UTF8.GetString(utf8_bytes);
    }
}
