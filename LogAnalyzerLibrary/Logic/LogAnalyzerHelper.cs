using LogAnalyzerLibrary.Model;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace LogAnalyzerLibrary.Logic
{
    public interface ILogAnalyzerHelper
    {
        Task<ApiResponse> ArchiveLogsFromPeriodService(ArchiveParamDto param);
        Task<ApiResponse> DeleteArchiveForAPeriodService(ArchiveParamDto param);
        ApiResponse DeleteAvailablelogsInAService(MiltiDirectoryParamDto param);
        Task<ApiResponse> GetDuplictaeTotalCountOfAvailablelogsService(MiltiDirectoryParamDto param);
        ApiResponse GetTotalCountOfAvailableLogsInAService(MiltiDirectoryParamDto param);
        Task<ApiResponse> GetUniqueTotalCountOfAvailablelogsService(MiltiDirectoryParamDto param);
        Task<ApiResponse> SearchLogsBySizeService(LogSizeSearchDto param);
        Task<ApiResponse> SearchLogsPerDirectoryService(string[] param);
        Task<ApiResponse> UploadLogToRemoteServerService(string[] directories);
    }


    public class LogAnalyzerHelper : ILogAnalyzerHelper
    {
        private readonly IConfiguration _configuration;

        public LogAnalyzerHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<string> GetExistingFileLogNameService(string[] directories)
        {
            var resp = new List<string>();
            if (directories.Any())
            {
                foreach (var directory in directories)
                {
                    if (!string.IsNullOrEmpty(directory))
                    {
                        string[] files = Directory.GetFiles(directory, "*.log");
                        if (files.Any())
                        {
                            var x = files.ToList();
                            resp.AddRange(x);
                        }
                    }
                }
            }
            return resp;
        }
        public List<string> GetExistingFileLogNameService(string directory)
        {
            var resp = new List<string>();
            if (!string.IsNullOrEmpty(directory))
            {
                string[] files = Directory.GetFiles(directory, "*.log");
                if (files.Any())
                {
                    var x = files.ToList();
                    resp.AddRange(x);
                }
            }
            return resp;
        }

        public ApiResponse GetTotalCountOfAvailableLogsInAService(MiltiDirectoryParamDto param)
        {
            var response = new ApiResponse();
            var count = 0;
            if (!param.Directories.Any())
            {
                response.OutResponses.Message = "Directories cannot be empty"; return response;
            } 
            if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
            {
               response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
            }
            var files = GetExistingFileLogNameService(param.Directories);
            if (files.Any())
            {
                foreach (string x in files)
                {
                    var fileInfo = Path.GetFileNameWithoutExtension(x);
                    var datecreated = GetLogDateByFileName(fileInfo);

                    if (param.StartPeriod <= datecreated && datecreated <= param.EndPeriod)
                    {
                        count++;
                    }
                }
            }
            response.Code = HttpStatusCode.OK;
            response.OutResponses.Message = "success";
            response.OutResponses.IsSuccess = true;
            response.OutResponses.Data = count;
            return response;
        }

        public DateTime GetLogDateByFileName(string fName)
        {
            string patternx = @"[_a-zA-Z]";
            var matchx = Regex.Replace(fName, patternx, "");
            string pattern = @"\b\d{4}\.\d{2}\.\d{2}\b";
            Match match = Regex.Match(matchx, pattern);
            return DateTime.ParseExact(match.Value, "yyyy.MM.dd", null);
        }

        public ApiResponse DeleteAvailablelogsInAService(MiltiDirectoryParamDto param)
        {
            var response = new ApiResponse();
            var count = 0;
            if (!param.Directories.Any())
            {
                response.OutResponses.Message = "Directories cannot be empty"; return response;
            }
            if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
            {
                response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
            }
            var files = GetExistingFileLogNameService(param.Directories);
            if (files.Any())
            {
                foreach (string x in files)
                {
                    var fileInfo = Path.GetFileNameWithoutExtension(x);
                    var datecreated = GetLogDateByFileName(fileInfo);
                    if (param.StartPeriod <= datecreated && datecreated <= param.EndPeriod)
                    {
                        if (File.Exists(x))
                        {
                            File.Delete(x);
                            count++;
                        }
                    }
                }
            }
            response.Code = HttpStatusCode.OK;
            response.OutResponses.IsSuccess = true;
            response.OutResponses.Message = count > 0 ? $"{count} record(s) deleted" : "No Record Found";
            return response;
        }
        public async Task<ApiResponse> GetUniqueTotalCountOfAvailablelogsService(MiltiDirectoryParamDto param)
        {
            var response = new ApiResponse();
            try
            {
                if (!param.Directories.Any())
                {
                    response.OutResponses.Message = "Directories cannot be empty"; return response;
                }
                if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
                {
                    response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
                }
                var errorLogList = new List<AnaylsisVM>();
                string patternx = @"\b\d{4}\.\d{2}\.\d{2}\b";
                string pattern = @"err(?:or)?(?:-)*:?\s*";
                if (param.Directories.Any())
                {
                    var files = GetExistingFileLogNameService(param.Directories);
                    if (files.Any())
                    {
                        foreach (string x in files)
                        {
                            var errorLog = new AnaylsisVM();
                            errorLog.CurrentLogFileName = Path.GetFileNameWithoutExtension(x);
                            Match match = Regex.Match(x, patternx);
                            if (File.Exists(x))
                            {
                                // Read the file contents
                                string logContent = File.ReadAllText(x);
                                var logSplit = logContent.Split("__________________________");
                                var logSplitWithError = logSplit.Where(x => x.ToLower().Contains("error:") || x.ToLower().Contains("error------>")).ToList();
                                List<string> logErrDetails = new List<string>();
                                if (logSplitWithError.Any())
                                {
                                    string splitParam = "";
                                    if(logSplitWithError.Any(x => x.ToLower().Contains("error------>")))
                                    {
                                        splitParam = "error------>";
                                    }
                                    else if (logSplitWithError.Any(x => x.ToLower().Contains("error:")))
                                    {
                                        splitParam = "error:";
                                    }

                                    foreach (var err in logSplitWithError)
                                    {
                                        MatchCollection matches = Regex.Matches(err, pattern, RegexOptions.IgnoreCase);
                                        if (matches.Any())
                                        {
                                            var lower = err.ToLower().Split(splitParam);
                                            logErrDetails.Add(lower[1]);
                                        }
                                    }
                                    var elementCounts = logErrDetails.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
                                    // Count distinct elements
                                    errorLog.ErrorCount = elementCounts.Count;

                                }
                            }
                            errorLogList.Add(errorLog);
                        }
                    }
                    response.Code = HttpStatusCode.OK;
                    response.OutResponses.IsSuccess = true;
                    response.OutResponses.Message = files.Any() ? $"success" : "No Record Found";
                    response.OutResponses.Data = errorLogList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse> GetDuplictaeTotalCountOfAvailablelogsService(MiltiDirectoryParamDto param)
        {
            var response = new ApiResponse();
            try
            {
                if (!param.Directories.Any())
                {
                    response.OutResponses.Message = "Directories cannot be empty"; return response;
                }
                if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
                {
                    response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
                }
                var errorLogList = new List<AnaylsisVM>();
                string patternx = @"\b\d{4}\.\d{2}\.\d{2}\b";
                string pattern = @"err(?:or)?(?:-)*:?\s*";
                if (param.Directories.Any())
                {
                    var files = GetExistingFileLogNameService(param.Directories);
                    if (files.Any())
                    {
                        foreach (string x in files)
                        {
                            var errorLog = new AnaylsisVM();
                            errorLog.CurrentLogFileName = Path.GetFileNameWithoutExtension(x);
                            Match match = Regex.Match(x, patternx);
                            if (File.Exists(x))
                            {
                                // Read the file contents
                                string logContent = File.ReadAllText(x);
                                var logSplit = logContent.Split("__________________________");
                                var logSplitWithError = logSplit.Where(x => x.ToLower().Contains("error:") || x.ToLower().Contains("error------>")).ToList();
                                List<string> logErrDetails = new List<string>();
                                if (logSplitWithError.Any())
                                {
                                    string splitParam = "";
                                    if (logSplitWithError.Any(x => x.ToLower().Contains("error------>")))
                                    {
                                        splitParam = "error------>";
                                    }
                                    else if (logSplitWithError.Any(x => x.ToLower().Contains("error:")))
                                    {
                                        splitParam = "error:";
                                    }
                                    foreach (var err in logSplitWithError)
                                    {
                                        MatchCollection matches = Regex.Matches(err, pattern, RegexOptions.IgnoreCase);
                                        if (matches.Any())
                                        {
                                            var lower = err.ToLower().Split(splitParam);
                                            logErrDetails.Add(lower[1]);
                                        }
                                    }
                                    var elementCounts = logErrDetails.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
                                    
                                    int duplicateCount = elementCounts.Count(kv => kv.Value > 1);
                                    errorLog.ErrorCount = duplicateCount;

                                }
                            }
                            errorLogList.Add(errorLog);
                        }
                    }
                    response.Code = HttpStatusCode.OK;
                    response.OutResponses.IsSuccess = true;
                    response.OutResponses.Message = files.Any() ? $"success" : "No Record Found";
                    response.OutResponses.Data = errorLogList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse> ArchiveLogsFromPeriodService(ArchiveParamDto param)
        {
            var response = new ApiResponse();
            try
            {
                int count = 0;
                if (string.IsNullOrEmpty(param.Directory))
                {
                    response.OutResponses.Message = "Directory cannot be empty"; return response;
                }
                if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
                {
                    response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
                }
                if (param.StartPeriod > param.EndPeriod)
                {
                    response.OutResponses.Message = "Invalid EndPeriod must be greater then StartPeriod"; return response;
                }
                // Create the name of the ZIP file

                var logFiles = Directory.GetFiles(param.Directory, "*.log");
                if (!logFiles.Any())
                {
                    response.OutResponses.Message = "No Record Found"; return response;
                }

                string zipFileName = $"{param.StartPeriod:dd_MM_yyyy}-{param.EndPeriod:dd_MM_yyyy}.zip";
                string zipFilePath = Path.Combine(param.Directory, zipFileName);
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                // Create a new ZIP archive
                using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    // Add each log file to the archive and delete log
                    foreach (var logFile in logFiles)
                    {
                        var fileInfo = Path.GetFileNameWithoutExtension(logFile);
                        var datecreated = GetLogDateByFileName(fileInfo);
                        if (param.StartPeriod <= datecreated && datecreated <= param.EndPeriod)
                        {
                            zipArchive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                            File.Delete(logFile);
                            count++;
                        }
                    }
                }
                response.Code = HttpStatusCode.OK;
                response.OutResponses.IsSuccess = true;
                response.OutResponses.Message = count > 0 ? $"{count} record(s) Archived" : "No Record Found";
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse> SearchLogsPerDirectoryService(string[] param)
        {
            var response = new ApiResponse();
            try
            {
                var errorLogList = new List<LogSearchVM>();
                if (!param.Any())
                {
                    response.OutResponses.Message = "Directory cannot be empty"; return response;
                }
                foreach (string x in param)
                {
                    var files = GetExistingFileLogNameService(x);

                    if (files.Any())
                    {
                        var errorLog = new LogSearchVM()
                        {
                            Directory = x,
                            Files = files.Select(f => Path.GetFileName(f)).ToList(),
                            FileCount = files.Count()
                        };
                        errorLogList.Add(errorLog);
                    }
                    response.Code = HttpStatusCode.OK;
                    response.OutResponses.IsSuccess = true;
                    response.OutResponses.Message = files.Any() ? "success" : "No Record Found";
                    response.OutResponses.Data = errorLogList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse> SearchLogsBySizeService(LogSizeSearchDto param)
        {
            var response = new ApiResponse();
            try
            {
                var errorLogList = new List<LogSearchVM>();
                if (!param.Directories.Any())
                {
                    response.OutResponses.Message = "Directory cannot be empty"; return response;
                }
                if (param.MiniSizeKb > param.MaxSizeKb)
                {
                    response.OutResponses.Message = "MaxSizeKb must be greater than MiniSizeKb"; return response;
                }
                foreach (string x in param.Directories)
                {
                    var files = Directory.GetFiles(x, "*.log").Where(file => IsLogFileWithinSizeRange(file, param.MiniSizeKb, param.MaxSizeKb)).ToList();
                    var errorLog = new LogSearchVM()
                    {
                        Directory = x,
                        Files = new List<string>(),
                        FileCount = 0
                    };
                    if (files.Any())
                    {
                        errorLog.Files = files.Select(f => Path.GetFileName(f)).ToList();
                        errorLog.FileCount = files.Count();
                    }
                    errorLogList.Add(errorLog);
                }

                response.Code = HttpStatusCode.OK;
                response.OutResponses.IsSuccess = true;
                response.OutResponses.Message = "success";
                response.OutResponses.Data = errorLogList;
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }


        private bool IsLogFileWithinSizeRange(string filePath, long minSizeKB, long maxSizeKB)
        {
            var fileInfo = new FileInfo(filePath);
            long fileSizeKB = (long)Math.Ceiling((double)fileInfo.Length / 1024);

            return fileSizeKB >= minSizeKB && fileSizeKB <= maxSizeKB;
        }

        public async Task<ApiResponse> DeleteArchiveForAPeriodService(ArchiveParamDto param)
        {
            var response = new ApiResponse();
            try
            {
                int count = 0;
                if (string.IsNullOrEmpty(param.Directory))
                {
                    response.OutResponses.Message = "Directory cannot be empty"; return response;
                }
                if (param.StartPeriod == DateTime.MinValue || param.EndPeriod == DateTime.MinValue)
                {
                    response.OutResponses.Message = "Invalid StartPeriod or EndPeriod entered"; return response;
                }
                if (param.StartPeriod > param.EndPeriod)
                {
                    response.OutResponses.Message = "Invalid EndPeriod must be greater then StartPeriod"; return response;
                }
                if (!string.IsNullOrEmpty(param.Directory))
                {
                    // Create the name of the ZIP file
                    string zipFileName = $"{param.StartPeriod:dd_MM_yyyy}-{param.EndPeriod:dd_MM_yyyy}.zip";
                    string zipFilePath = Path.Combine(param.Directory, zipFileName);
                    if (File.Exists(zipFilePath))
                    {
                        File.Delete(zipFilePath);
                        count++;
                    }
                }
                response.Code = HttpStatusCode.OK;
                response.OutResponses.IsSuccess = true;
                response.OutResponses.Message = count > 0 ? $"Deleted" : "No Record Found";
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }
        
        
        //NB: No API provide and one need to see API documentations to know what it takes to consume it.
        //Possible API call to upload logs to remote server

        public async Task<ApiResponse> UploadLogToRemoteServerService(string[] directories)
        {
            var response = new ApiResponse();
            try
            {
                int failedCount = 0;
                int successCount = 0;
                string bearerToken = _configuration["BearerToken"];
                string remoteUrl = _configuration["RemoteUrl"];
                if (string.IsNullOrEmpty(bearerToken))
                {
                    response.OutResponses.Message = "Directory cannot be empty"; return response;
                }
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    var logFiles = GetExistingFileLogNameService(directories);

                    foreach (var logFile in logFiles)
                    {
                        byte[] fileBytes = await File.ReadAllBytesAsync(logFile);

                        ByteArrayContent fileContent = new ByteArrayContent(fileBytes);

                        fileContent.Headers.Add("FileName", Path.GetFileName(logFile));

                        HttpResponseMessage responses = await client.PostAsync(remoteUrl, fileContent);

                        if (responses.IsSuccessStatusCode)
                        { successCount++; }
                        else
                        { failedCount++; }
                    }
                }
                response.Code = HttpStatusCode.OK;
                response.OutResponses.IsSuccess = true;
                response.OutResponses.Message = $@"{successCount} uploaded successfully, {failedCount} Failed to upload";
                return response;
            }
            catch (Exception ex)
            {
                response.OutResponses.Message = ex.Message;
                return response;
            }
        }
    
    }

}