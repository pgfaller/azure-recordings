using Microsoft.WindowsAzure.Storage.Table;
namespace Recordings {
    public class RecordingTableEntity: TableEntity
    {
        public string Source { get; set; }
        public string Month { get; set; }
        public long Epoch { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }

    public class Recording
    {
        public string source { get; set; }
        public uint timestamp { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
    };
    public class Document : Recording
    {
        public string month { get; set; }
    };
}