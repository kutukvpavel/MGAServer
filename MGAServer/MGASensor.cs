using System;

namespace MGA
{
    public class MGASensor
    {
        public float TCR { get; set; }
        public float T0 { get; set; }
        public float R0 { get; set; }
        public float RShort { get; set; }
        public float? ExplicitTargetResistance { get; set; }
        public float GetTargetResistance(float targetTemp)
        {
            return ExplicitTargetResistance ?? (R0 * (TCR * (targetTemp - T0) + 1) + RShort);
        }
    }
}
