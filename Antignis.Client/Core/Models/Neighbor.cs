using System.Collections.Generic;

namespace Antignis.Client.Core.Models
{
    internal class Neighbor
    {

        public string IPAddress { get; set; }
        public List<int> PortsOpen { get; set; }



    }
}
