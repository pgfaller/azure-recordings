namespace Recordings {
    public class Recording
    {
        public uint timestamp { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
    };
    public class Document : Recording
    {
        public string month { get; set; }
    };
}