using CantinaIBJ.Integration.Cognito;
using CantinaIBJ.Integration.Cognito.Model.User;
using CantinaIBJ.Model;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CantinaIBJ.WebApi.Common.Constants;

namespace Risk.WebApi.Controllers.Auth;

[ServiceFilter(typeof(ValidateModelAttribute))]
public class UserController : CoreController
{
    readonly ILogger<UserController> _logger;
    readonly HttpUserContext _userContext;
    readonly ICognitoCommunication _cognitoClient;

    public UserController(
        ILogger<UserController> logger,
        HttpUserContext userContext,
        ICognitoCommunication cognitoClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
    }

    /// <summary>
    /// Obtém uma lista de usuários
    /// </summary>
    /// <param name="paginationToken">Token de paginação</param>
    /// <param name="page">Número da página</param>
    /// <param name="size">Total de itens retornados</param>
    /// <param name="filter"><br/><b>Atributos permitidos</b>:<br/>username; <br/>email; <br/>phone_number; <br/>name; <br/>given_name; <br/>family_name; <br/>preferred_username; <br/>cognito:user_status; <br/>status; <br/>sub.<br/><br/> <code>Exemplo de uso: username ^= "user"</code></param>
    /// <returns></returns>
    [HttpGet()]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(typeof(ListDataPagination<UserGetResponseModel>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUsersAsync(int page = 0, int size = 5, string? filter = null, string? paginationToken = null)
    {
        if (paginationToken is not null) 
            paginationToken = FixPaginationToken(paginationToken);

        try
        {
            var contextUser = _userContext.GetContextUser();
            var result = await _cognitoClient.GetUsersAsync(paginationToken, page, size, GetPoolId(), filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    public static string FixPaginationToken(string value) => value.Replace(" ", "+");

    /// <summary>
    /// Obtém dados de um usuário
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(typeof(UserGetResponseModel), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUserAsync([FromRoute] string id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();
            var result = await _cognitoClient.GetUserAsync(id, GetPoolId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Obtém dados de um usuário
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}/WithoutPermission")]
    [ProducesResponseType(typeof(UserGetResponseModel), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUserByIdWithouPermissionAsync([FromRoute] string id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();
            var result = await _cognitoClient.GetUserAsync(id, GetPoolId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Obtém grupos de um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="page">Número da página</param>
    /// <param name="size">Total de itens retornados</param>
    /// <returns></returns>
    [HttpGet("{id}/Groups")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(typeof(ListDataPagination<UserGroupResponseModel>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetUserGroupsAsync(string id, [FromQuery] int page = 0, [FromQuery] int size = 5)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            return Ok(await _cognitoClient.GetUserGroupsAsync(id, GetPoolId(), page, size));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Cria um usuário
    /// </summary>
    [HttpPost()]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(typeof(UserPostResponseModel), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes("application/json")]
    public async Task<IActionResult> PostUserAsync([FromBody] UserPostRequestModel inputModel)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            if (!string.IsNullOrEmpty(inputModel.PhoneNumber))
            {
                inputModel.PhoneNumber = inputModel.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }

            if (!inputModel.PhoneNumber.StartsWith("+55"))
                inputModel.PhoneNumber = "+55" + inputModel.PhoneNumber;

            var result = await _cognitoClient.CreateUserAsync(inputModel, GetPoolId(), "SUPPRESS");

            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Edita um usuário
    /// </summary>
    [HttpPut("{userId}")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes("application/json")]
    public async Task<IActionResult> PutUserAsync([FromRoute] string userId, [FromBody] UserPutRequestModel inputModel)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            if (!string.IsNullOrEmpty(inputModel.PhoneNumber))
            {
                inputModel.PhoneNumber = inputModel.PhoneNumber.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }

            if (inputModel.PhoneNumber is not null && !inputModel.PhoneNumber.StartsWith("+55"))
                inputModel.PhoneNumber = "+55" + inputModel.PhoneNumber;
             
            await _cognitoClient.UpdateUserAsync(userId, inputModel, GetPoolId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Edita um usuário
    /// </summary>
    [HttpPut("{userId}/AddUserToGroup")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes("application/json")]
    public async Task<IActionResult> AddUserToGroupAsync([FromRoute] string userId, [FromBody] GroupRequestModel request)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            await _cognitoClient.AddtUserToAnExistingGroupAsync(GetPoolId(), request.GroupName, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Edita um usuário
    /// </summary>
    [HttpPut("{userId}/RemoveUserToGroup")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes("application/json")]
    public async Task<IActionResult> RemoveUserToGroupAsync([FromRoute] string userId, [FromBody] GroupRequestModel request)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            await _cognitoClient.RemoveUserFromAnExistingGroupAsync(GetPoolId(), request.GroupName, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Exclui um usuário
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteUserAsync([FromRoute] string id)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            await _cognitoClient.RemoveUserAsync(id, GetPoolId());

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Redefine a senha do usuário
    /// </summary>
    [HttpPut("{id}/ResetPassword")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PostResetPasswordAsync([FromRoute] string id, string? password = null)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            //recuperar os dados do operador
            var userGetResponseModel = await _cognitoClient.GetUserAsync(id, GetPoolId());

            if (userGetResponseModel.UserStatus == "FORCE_CHANGE_PASSWORD")
            {
                UserPostRequestModel userPost = new()
                {
                    Email = userGetResponseModel.Email,
                    PhoneNumber = userGetResponseModel.PhoneNumber,
                    Name = userGetResponseModel.Name,
                    Password = !String.IsNullOrEmpty(password) ? password : "Mudar@123",
                    EmailVerified = userGetResponseModel.EmailVerified
                };

                if (!userPost.PhoneNumber.StartsWith("+55"))
                    userPost.PhoneNumber = "+55" + userPost.PhoneNumber;
                await _cognitoClient.CreateUserAsync(userPost, GetPoolId(), "RESEND");
            }
            else if (userGetResponseModel.UserStatus == "CONFIRMED" && !String.IsNullOrEmpty(password))
            {
                await _cognitoClient.AdminSetUserPasswordAsync(id, GetPoolId(), password, false);
            }
            else
            {
                await _cognitoClient.ResetPasswordUserAsync(id, GetPoolId());
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    [HttpPut("{id}/SetPassword")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SetPassword(string id, [FromBody] string newPassword, bool permanent)
    {
        try
        {
            var contextUser = _userContext.GetContextUser();

            await _cognitoClient.AdminSetUserPasswordAsync(id, GetPoolId(), newPassword, permanent);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    /// <summary>
    /// Suas informações
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("whoAmI")]
    [Authorize(Policy.MASTERADMIN)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public IActionResult GetWhoAmIAsync()
    {
        try
        {
            var contextUser = _userContext.GetContextUser();
            return Ok(contextUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return InvalidBusinessRules(ex.GetFriendlyException());
        }
    }

    private string GetPoolId() => _userContext.GetContextUser().GetCurrentUserPoolId();
}