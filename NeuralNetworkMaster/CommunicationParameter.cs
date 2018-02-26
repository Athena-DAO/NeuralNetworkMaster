using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster
{
    class CommunicationParameter
    {
        public string Id { get; set; }
        public int NumberOfSlaves { get; set; }
        public int Port { get; set; }
        public SlaveLocation[] SlaveLocations { get; set; }
    }
}
