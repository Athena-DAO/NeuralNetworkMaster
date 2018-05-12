using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetworkMaster.Model
{
    class Pipeline
    {
        public string id { get; set; }
        public string algorithmId { get; set; }
        public string algorithmName { get; set; }
        public string algorithmDescription { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int numberOfContainers { get; set; }
        public string result { get; set; }
        public string dataSetId { get; set; }
        public List<PipelineParameters> parameters { get; set; }
    }
}
