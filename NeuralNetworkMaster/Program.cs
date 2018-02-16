using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace NeuralNetworkMaster
{
    class Program
    {

        static void Main(string[] args)
        {

            NeuralNetworkMaster master = new NeuralNetworkMaster
            {
                NumberOfSlaves = 2,
                InputLayerSize = 400,
                HiddenLayerSize = 25,
                HiddenLayerLength = 1,
                OutputLayerSize = 10,
                Lambda = 3,
                Epoch = 50
                
            };

            master.Master();

        }
    }
}
