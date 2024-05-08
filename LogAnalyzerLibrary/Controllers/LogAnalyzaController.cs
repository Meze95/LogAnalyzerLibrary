using LogAnalyzerLibrary.Logic;
using LogAnalyzerLibrary.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LogAnalyzerLibrary.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogAnalyzaController : ControllerBase
    {
        private readonly ILogAnalyzerHelper _logAnalyzerHelper;

        public LogAnalyzaController(ILogAnalyzerHelper logAnalyzerHelper)
        {
            _logAnalyzerHelper = logAnalyzerHelper;
        }

        [HttpPost("GetTotalCountOfAvailablelogs")]
        public IActionResult GetTotalCountOfAvailableLogs([FromBody] MiltiDirectoryParamDto param)
        {
           if(param != null)
            {
                var res = _logAnalyzerHelper.GetTotalCountOfAvailableLogsInAService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
           return BadRequest("Invalid Parameters");
        }
       
        [HttpPost("DeleteAvailablelogsByDateRange")]
        public IActionResult DeleteAvailableLogs([FromBody] MiltiDirectoryParamDto param)
        {
           if(param != null)
            {
                var res = _logAnalyzerHelper.DeleteAvailablelogsInAService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
           return BadRequest("Invalid Parameters");
        }
        
        [HttpPost("GetUniqueTotalCountOfAvailablelogs")]
        public async Task<IActionResult> GetUniqueTotalCountOfAvailableLogs([FromBody] MiltiDirectoryParamDto param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.GetUniqueTotalCountOfAvailablelogsService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("GetDuplicateTotalCountOfAvailablelogs")]
        public async Task<IActionResult> GetDuplicateTotalCountOfAvailableLogs([FromBody] MiltiDirectoryParamDto param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.GetDuplictaeTotalCountOfAvailablelogsService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("ArchiveLogsFromPeriod")]
        public async Task<IActionResult> ArchiveLogsFromPeriod([FromBody] ArchiveParamDto param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.ArchiveLogsFromPeriodService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("SearchLogsPerDirectory")]
        public async Task<IActionResult> SearchLogsPerDirectory([FromBody] string[] param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.SearchLogsPerDirectoryService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("SearchLogsBySize")]
        public async Task<IActionResult> SearchLogsBySize([FromBody] LogSizeSearchDto param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.SearchLogsBySizeService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("DeleteArchive")]
        public async Task<IActionResult> DeleteArchive([FromBody] ArchiveParamDto param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.DeleteArchiveForAPeriodService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }

        [HttpPost("UploadLogToRemoteServer")]
        public async Task<IActionResult> UploadLogToRemoteServer([FromBody] string[] param)
        {
            if (param != null)
            {
                var res = await _logAnalyzerHelper.UploadLogToRemoteServerService(param);
                switch (res.Code)
                {
                    case HttpStatusCode.OK:
                        return Ok(res.OutResponses);
                    default:
                        return BadRequest(res.OutResponses);
                }
            }
            return BadRequest("Invalid Parameters");
        }
    }
}
