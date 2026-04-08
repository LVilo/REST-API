using System.Text.Json;

namespace RestAPI
{
    public class DeviceConfig
    {
        public string Id { get; set; }

        public string Arm { get; set; }
        public string DeviceFamily { get; set; }
        public string DeviceType { get; set; }
        public uint SerialNumber { get; set; }
        public string OrderNumber { get; set; }
        public JsonContent Settings { get; set; }
        public bool IsActual {  get; set; }
    }
}
