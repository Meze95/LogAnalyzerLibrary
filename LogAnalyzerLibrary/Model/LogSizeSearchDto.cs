namespace LogAnalyzerLibrary.Model
{
    public class LogSizeSearchDto
    {
        public string[] Directories { get; set; }
        public long MiniSizeKb { get; set; }
        public long MaxSizeKb { get; set; }
    }
}
