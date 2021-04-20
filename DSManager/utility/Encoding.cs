#define UTF8
//#define UNICODE
//#define ASCII
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    public class Encoding
    {
#if UTF8
          static  UTF8Encoding utf8 = new UTF8Encoding();
#elif UNICODE
          static UnicodeEncoding Unicode = new UnicodeEncoding();
#elif ASCII
          static   ASCIIEncoding Ascii = new ASCIIEncoding();
#endif
        static Encoding()
        { 
        
        }
        public static  byte[] getbyte(string msg)
        {
#if UTF8
            return utf8.GetBytes(msg);

#elif UNICODE
            return Unicode.GetBytes(msg);
#elif ASCII
            return Ascii.GetBytes(msg);
#endif
        }
        public static string getstring(byte[] buffer)
        {
#if UTF8
            return utf8.GetString(buffer);

#elif UNICODE
            return Unicode.GetString(buffer);
#elif ASCII
            return Ascii.GetString(buffer);
#endif
        }
        public static string getstring(byte[] buffer,int index,int count)
        {
#if UTF8
            return utf8.GetString(buffer,index,count);

#elif UNICODE
            return Unicode.GetString(buffer,index,count);
#elif ASCII
            return Ascii.GetString(buffer,index,count);
#endif
        }
    }
}
