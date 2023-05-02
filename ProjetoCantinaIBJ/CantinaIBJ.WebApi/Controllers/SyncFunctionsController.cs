using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using static CantinaIBJ.WebApi.Common.Constants;

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
        readonly HttpUserContext _userContext;

        public SyncFunctionsController(
            IOptions<CognitoSettings> cognitoSettings,
            HttpUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _cognitoSettings = cognitoSettings.Value;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("CreateGroupsForUsers")]
        public async Task<IActionResult> CreateGroupsForUsers([FromQuery] string groupName, string userPoolId)
        {
            try
            {
                var contextUser = _userContext.GetContextUser();

                var cognitoClient = new AmazonCognitoIdentityProviderClient(_cognitoSettings.AccessKey, _cognitoSettings.SecretKey, RegionEndpoint.USEast2);

                var createGroupRequest = new CreateGroupRequest
                {
                    GroupName = groupName,
                    UserPoolId = userPoolId
                };

                var createGroupResponse = await cognitoClient.CreateGroupAsync(createGroupRequest);

                return Ok("Group created with ID: " + createGroupResponse.Group.GroupName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddUserForGroups")]
        [Authorize]
        public async Task<IActionResult> AddUserForGroups([FromQuery] string groupName, string userPoolId, string username)
        {
            try
            {
                var contextUser = _userContext.GetContextUser();

                var cognitoClient = new AmazonCognitoIdentityProviderClient(_cognitoSettings.AccessKey, _cognitoSettings.SecretKey, RegionEndpoint.USEast2);

                var request = new AdminAddUserToGroupRequest
                {
                    GroupName = groupName,
                    UserPoolId = userPoolId,
                    Username = username
                };

                var response = await cognitoClient.AdminAddUserToGroupAsync(request);

                // Verificar se a resposta é bem-sucedida
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Atualizar o token com a informação do grupo adicionado
                    // por exemplo, adicionar uma claim com o nome do grupo
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserAdmin")]
        [Authorize(Policy.USER)]
        public async Task<IActionResult> GetTestUserAdmin([FromQuery] string username, string userPoolId)
        {
            var contextUser = _userContext.GetContextUser();

            //var cognitoClient = new AmazonCognitoIdentityProviderClient(_cognitoSettings.AccessKey, _cognitoSettings.SecretKey, RegionEndpoint.USEast2);

            //HttpContext context = _httpContextAccessor.HttpContext;

            //var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;

            //var user = await cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
            //{
            //    UserPoolId = userPoolId,
            //    Username = username
            //});

            //if (user != null)
            //{
            //    if (user.UserAttributes != null)
            //    {
            //        if (user.UserAttributes.Any(a => a.Name == "email"))
            //            claimsIdentity.AddClaim(new Claim("custom:email", user.UserAttributes.First(a => a.Name == "email").Value));
            //        if (user.UserAttributes.Any(a => a.Name == "name"))
            //            claimsIdentity.AddClaim(new Claim("custom:name", user.UserAttributes.First(a => a.Name == "name").Value));
            //        if (user.UserAttributes.Any(a => a.Name == "phone_number"))
            //            claimsIdentity.AddClaim(new Claim("custom:phone_number", user.UserAttributes.First(a => a.Name == "phone_number").Value));
            //    }
            //}

            return Ok();
        }

        //[HttpGet("GetTestGroups")]
        //[Authorize]
        //public IActionResult GetTestGroups()
        //{
        //    // Obter as informações do usuário a partir do token
        //    var groupClaim = User.Claims.FirstOrDefault(c => c.Type == "cognito:groups");

        //    if (groupClaim != null)
        //    {
        //        var groupName = groupClaim.Value;

        //        // Fazer algo com o nome do grupo
        //    }

        //    KeyValue keyValue = new(){
        //        Key = groupClaim.Type,
        //        Value = groupClaim.Value
        //    };

        //    return Ok(keyValue);
        //}
    }
}
