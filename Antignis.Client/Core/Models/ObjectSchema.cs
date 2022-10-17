using System.Collections.Generic;

namespace Antignis.Client.Core.Models
{
    public class Host
    {
        public int Id { get; set; }

        public int WindowsFirewallSettingId { get; set; }

        public bool IsServerOS { get; set; }

        public string OperatingSystem { get; set; }

        public string DNSHostname { get; set; }

        public string IPAddress { get; set; }

        public string NetworkMask { get; set; }

        public virtual List<FileShare> FileShare { get; set; }

        public virtual List<TCPConnection> TCPConnection { get; set; }

        public virtual List<WindowsFirewallRule> WindowsFirewallRule { get; set; }

        public virtual WindowsFirewallSetting WindowsFirewallSetting { get; set; }

        public virtual List<Port> Port { get; set; }

        public virtual List<Program> Program { get; set; }

        public virtual List<Role> Role { get; set; }

        //public List<IP> IPAddress { get; set; }
    }

    public class IP
    {
        public string Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Address { get; set; }

        public string NetworkMask { get; set; }
    }

    public class WindowsFirewallRule
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Name { get; set; }

        public string LocalPorts { get; set; }

        public string RemoteAddresses { get; set; }

        public string Action { get; set; }

        public bool RuleEnabled { get; set; }

        public string Interfaces { get; set; }

        public string Profiles { get; set; }
    }

    public class WindowsFirewallSetting
    {
        public int Id { get; set; }


        // Booleans to indicate if profile is enabled or not
        public bool PrivateProfileEnabled { get; set; }

        public bool PublicProfileEnabled { get; set; }

        public bool DomainProfileEnabled { get; set; }

        // strings containing the default action
        public string PrivateProfileDefaultBlockAction { get; set; }

        public string PublicProfileDefaultBlockAction { get; set; }

        public string DomainProfileDefaultBlockAction { get; set; }
    }

    public class TCPConnection
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Direction { get; set; }
        public string LocalIPAddress { get; set; }
        public string RemoteIPAddress { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
    }

    public class FileShare
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Name { get; set; }

    }

    public class Port
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public int PortNumber { get; set; }

    }

    public class Program
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Name { get; set; }

    }

    public class Role
    {
        public int Id { get; set; }

        public virtual Host Host { get; set; }

        public int HostId { get; set; }

        public string Name { get; set; }

    }
}
