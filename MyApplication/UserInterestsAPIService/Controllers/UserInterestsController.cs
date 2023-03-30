using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UserProfileService.Interfaces;

namespace UserInterestsAPIService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserInterestsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public UserInterestsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:8000");
        }

        [HttpGet("{guid}/interests")]
        public async Task<IActionResult> GetInterests(string guid)
        {
            try
            {
                IUserProfileService userProfileService = ActorProxy.Create<IUserProfileService>(new ActorId(guid), new Uri("fabric:/MyApplication/UserProfileActorService"));
                var interests = await userProfileService.GetInterestsAsync();

                return Ok(interests);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{guid}/interests")]
        public async Task<IActionResult> AddInterests(string guid, [FromBody] string[] interests)
        {
            try
            {
                IUserProfileService userProfileService = ActorProxy.Create<IUserProfileService>(new ActorId(guid), new Uri("fabric:/MyApplication/UserProfileActorService"));
                await userProfileService.AddInterestsAsync(interests);
                return Ok();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
