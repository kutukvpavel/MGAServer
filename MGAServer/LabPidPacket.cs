using System.Collections.Generic;

namespace MGA.LabPid
{
    public class Packet
    {
        //public float Temperature { get; set; }
        public float Setpoint { get; set; }
        //public GpioDescriptor Gpio { get; set; }
    }

    /*public class GpioDescriptor
    {
        public bool[] Inputs { get; }
        public bool[] Outputs { get; }

        public Dictionary<int, string> InputLabels { get; set; }
        public Dictionary<int, string> OutputLabels { get; set; }
    }*/
}
