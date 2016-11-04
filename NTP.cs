using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Net.NetworkInformation
{
    public struct NTPInformation
    {
        public DateTime RemoteTimeUTC;
        public DateTime LocalTimeUTC;
        /// <summary>
        /// Add the correction to the local time to get the remote time.  Negative numbers mean the local time is ahead of the remote.
        /// </summary>
        public TimeSpan Correction;
        public bool Success;
        public Exception Error;
    }

    //adapted from http://stackoverflow.com/questions/650849/change-system-date-programatically
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    //adapted from http://stackoverflow.com/questions/683491/how-to-declarate-large-integer-in-c-sharp
    //for use in http://stackoverflow.com/questions/650849/change-system-date-programatically CLI/C++ translated example
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        public Int64 QuadPart;
        [FieldOffset(0)]
        public UInt32 LowPart;
        [FieldOffset(4)]
        public Int32 HighPart;
    }

    public class NTP
    {
        static private object LockObject = new object();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME st);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool GetSystemTime(ref SYSTEMTIME st);

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724280%28v=vs.85%29.aspx
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FileTimeToSystemTime(ref System.Runtime.InteropServices.ComTypes.FILETIME ft, ref SYSTEMTIME st);

        //adapted from http://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c
        static public void InterrogateRemote(string RemoteUrl, ref NTPInformation info)
        {
            info.Success = false;
            try
            {
                //default Windows time server
                //const string ntpServer = "time.windows.com";

                // NTP message size - 16 bytes of the digest (RFC 2030)
                var ntpData = new byte[48];

                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                var addresses = Dns.GetHostEntry(RemoteUrl).AddressList;
                IPAddress RemoteIP = addresses.FirstOrDefault<IPAddress>();
                //The UDP port number assigned to NTP is 123
                var ipEndPoint = new IPEndPoint(RemoteIP, 123);
                //NTP uses UDP
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 5000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
                info.LocalTimeUTC = DateTime.UtcNow;

                //Offset to get to the "Transmit Timestamp" field (time at which the reply 
                //departed the server for the client, in 64-bit timestamp format."
                const byte serverReplyTime = 40;

                //Get the seconds part
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                //Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                //Convert From big-endian to little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                //**UTC** time
                info.RemoteTimeUTC = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
                info.Correction = info.RemoteTimeUTC - info.LocalTimeUTC;

                //return networkDateTime.ToLocalTime();
                info.Success = true;
            }
            catch (Exception ex)
            {
                info.Error = ex;
                return;
            }
        }

        //adapted from http://stackoverflow.com/questions/650849/change-system-date-programatically
        static public bool AdjustMachineTime(double Seconds)
        {
            lock (LockObject)
            {
                DateTime newDate = DateTime.UtcNow.AddSeconds(Seconds);
                SYSTEMTIME st = new SYSTEMTIME();
                LARGE_INTEGER largeInt = new LARGE_INTEGER();
                largeInt.QuadPart = newDate.ToFileTime();

                System.Runtime.InteropServices.ComTypes.FILETIME fileTime;
                fileTime.dwHighDateTime = largeInt.HighPart;
                fileTime.dwLowDateTime = (int)largeInt.LowPart;
                if (!FileTimeToSystemTime(ref fileTime, ref st))
                    return false;
                return SetSystemTime(ref st);
            }
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
