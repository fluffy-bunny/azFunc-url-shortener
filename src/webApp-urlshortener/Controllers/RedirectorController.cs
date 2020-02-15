﻿using dotnetcore.urlshortener.contracts;
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
        private ITenantStore _tenantStore;
        private ILogger<RedirectorController> _logger;

        public RedirectorController(
            IConfiguration configuration,
            ITenantStore tenantStore,
            ILogger<RedirectorController> logger)
        {
            _configuration = configuration;
            _tenantStore = tenantStore;
            _logger = logger;
        }
        public async Task<IActionResult> Get()
        {
            var tenantServices = await _tenantStore.GetTenantAsync("marketing");
            var tokenResponse = await tenantServices.GetAccessTokenAsync();
            return new JsonResult(tokenResponse.AccessToken);
        }
    }
}
