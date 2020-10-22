using AutoMapper;
using DynamicsAdapter.Web.Register;
using Fams3Adapter.Dynamics;
using Fams3Adapter.Dynamics.DataProvider;
using Fams3Adapter.Dynamics.RfiService;
using Fams3Adapter.Dynamics.SearchApiRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using Serilog.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicsAdapter.Web.Rfi
{
	[Route("[controller]")]
    [ApiController]
    public class RequestForInformationController : ControllerBase
    {
        private readonly ILogger<RequestForInformationController> _logger;
        private readonly IRfiSubmittalService _rfiService;
        private readonly IDataPartnerService _dataPartnerService;
        private readonly IMapper _mapper;
        private readonly ISearchRequestRegister _register;

        public RequestForInformationController(IRfiSubmittalService rfiService,
            IDataPartnerService dataPartnerService,
            ILogger<RequestForInformationController> logger, 
            IMapper mapper,
            ISearchRequestRegister register)
        {
            _rfiService = rfiService;
            _dataPartnerService = dataPartnerService;
            _logger = logger;
            _mapper = mapper;
            _register = register;
        }

        //POST: Completed/id
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("Completed/{key}")]
        [OpenApiTag("Request For Information Events API")]
        public async Task<IActionResult> Completed(string key, [FromBody] Models.RequestForInformationStatus rfiCompletedEvent)
        {
            try
            {
                Guard.NotNull(rfiCompletedEvent, nameof(rfiCompletedEvent));
                using (LogContext.PushProperty("RFI Id", rfiCompletedEvent?.Id))
                {
                    _logger.LogInformation("Received Person search completed event");
                    var cts = new CancellationTokenSource();
                    SSG_SearchApiRequest request = await _register.GetSearchApiRequest(key);
                    //update completed event
                    var rfiApiEvent = _mapper.Map<SSG_RfiMessageEvents>(rfiCompletedEvent);
                    _logger.LogDebug($"Attempting to create a new event for SearchApiRequest");
                    await _rfiService.AddEventAsync(request.SearchApiRequestId, rfiApiEvent, cts.Token);
                    _logger.LogInformation($"Successfully created completed event for SearchApiRequest");

                    return Ok();
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}