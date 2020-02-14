using dotnetcore.oauth2Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace webApp_urlshortener.Controllers
{


    [Route("Redirector")]
    public class RedirectorController : ControllerBase
    {
        private IConfiguration _configuration;
        private IClientCredentialsManager _clientCredentialsManager;
        private ILogger<RedirectorController> _logger;

        public RedirectorController(
            IConfiguration configuration,
            IClientCredentialsManager clientCredentialsManager,
            ILogger<RedirectorController> logger)
        {
            _configuration = configuration;
            _clientCredentialsManager = clientCredentialsManager;
            _logger = logger;
        }
        public async Task<IActionResult> Get()
        {
            var tokenResponse = await _clientCredentialsManager.GetAccessTokenAsync("marketing");
            return new JsonResult(tokenResponse.AccessToken);
        }
    }
}
