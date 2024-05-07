namespace LogAnalyzerLibrary.Model
{
    public class LogSearchVM
    {
        public string Directory { get; set; }
        public List<string> Files { get; set; }
        public int FileCount { get; set; }
    }
}
