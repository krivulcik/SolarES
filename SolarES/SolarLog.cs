using System;

namespace SolarES
{
    public class SolarLog
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public int DurationMs { get; set; }
        public double ProductionW { get; set; }
        public double ProductionWh { get; set; }
        public double LoadW { get; set; }
        public double LoadWh { get; set; }
    }
}
