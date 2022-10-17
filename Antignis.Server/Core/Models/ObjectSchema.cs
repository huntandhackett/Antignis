using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Antignis.Server.Core.Models
{
    public class Host
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }

        [Index, ForeignKey("WindowsFirewallSetting")]
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
    }

    public class WindowsFirewallRule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
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
        [Key]
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
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
        public int HostId { get; set; }

        public string Direction { get; set; }

        public string LocalIPAddress { get; set; }

        public string RemoteIPAddress { get; set; }

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }
    }

    public class FileShare
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
        public int HostId { get; set; }

        public string Name { get; set; }

    }

    public class Port
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
        public int HostId { get; set; }

        public int PortNumber { get; set; }

    }

    public class Program
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
        public int HostId { get; set; }

        public string Name { get; set; }

    }

    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual Host Host { get; set; }

        [Index, ForeignKey("Host")]
        public int HostId { get; set; }

        public string Name { get; set; }

    }

    public class Query
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string QueryString { get; set; }
    }
}
