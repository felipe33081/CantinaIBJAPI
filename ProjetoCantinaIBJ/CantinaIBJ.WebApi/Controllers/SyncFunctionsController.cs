using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.WebApi.Controllers.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CantinaIBJ.WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
#if RELEASE
    [ApiExplorerSettings(IgnoreApi = true)]
#endif
    public class SyncFunctionsController : CoreController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        readonly CognitoSettings _cognitoSettings;
        readonly IWhatsGWService _whatsGWService;

        public SyncFunctionsController(
            IOptions<CognitoSettings> cognitoSettings,
            IHttpContextAccessor httpContextAccessor,
            IWhatsGWService whatsGWService)
        {
            _cognitoSettings = cognitoSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _whatsGWService = whatsGWService;
        }

        [HttpPost("TesteWhatsMessage")]
        public async Task<IActionResult> TesteWhatsMessage([FromQuery] string toNumber, string message)
        {
            try
            {
                var result = await _whatsGWService.WhatsSendMessage(toNumber, message);


                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
