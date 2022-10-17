using System;
using System.Collections.Generic;
using System.Globalization;

namespace Antignis.Client.Core.Net
{
    public static class IpHelpers
    {
        public static string ToIpString(this UInt32 value)
        {

            uint bitmask = 0xff000000;
            string[] parts = new string[4];
            for (int i = 0; i < 4; i++)
            {
                uint masked = (value & bitmask) >> ((3 - i) * 8);
                bitmask >>= 8;
                parts[i] = masked.ToString(CultureInfo.InvariantCulture);
            }
            return String.Join(".", parts);
        }

        public static UInt32 ParseIp(this string ipAddress)
        {
            string[] splitted = ipAddress.Split('.');
            UInt32 ip = 0;
            for (int i = 0; i < 4; i++)
            {
                ip = (ip << 8) + UInt32.Parse(splitted[i]);
            }
            return ip;
        }
    }

    public class IPSegment
    {

        private readonly UInt32 _ip;
        private readonly UInt32 _mask;

        public IPSegment(string ip, string mask)
        {
            _ip = ip.ParseIp();
            _mask = mask.ParseIp();
        }

        public UInt32 NumberOfHosts => ~_mask + 1;

        public UInt32 NetworkAddress => _ip & _mask;

        public UInt32 BroadcastAddress => NetworkAddress + ~_mask;

        public IEnumerable<UInt32> Hosts()
        {
            for (uint host = NetworkAddress + 1; host < BroadcastAddress; host++)
            {
                yield return host;
            }
        }

    }
}