using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model.User;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Models.Create.User;
using CantinaIBJ.WebApi.Models.Read.User;
using CantinaIBJ.WebApi.Models.Update.User;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CantinaIBJ.WebApi.Controllers.Auth
{
    [Route("v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class UserController : CoreController
    {
        readonly IUserRepository _userRepository;
        readonly IMapper _mapper;
        readonly ILogger<UserController> _logger;
        readonly HttpUserContext _userContext;

        public UserController(
            IMapper mapper,
            ILogger<UserController> logger,
            IUserRepository userRepository,
            HttpUserContext userContext)
        {
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _userContext = userContext;
        }

        /// <summary>
        /// Lista todos os usuários
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy.Admin)]
        public async Task<ActionResult<IAsyncEnumerable<UserReadModel>>> UsertListAsync()
        {
            try
            {
                var contextUser = _userContext.GetContextUser();

                var users = await _userRepository.GetUsers(contextUser);

                return Ok(users);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Acessa um registro de usuário por Id(Código)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy.Admin)]
        [ProducesResponseType(typeof(UserReadModel), 200)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var contextUser = _userContext.GetContextUser();

                var user = await _userRepository.GetUserByIdAsync(contextUser, id);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                var readUser = _mapper.Map<UserReadModel>(user);

                return Ok(readUser);
            }
            catch (Exception e)
            {
                return LoggerBadRequest(e, _logger);
            }
        }

        /// <summary>
        /// Cria um novo registro de um usuário
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy.Admin)]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<IActionResult> Create([FromBody] UserCreateModel model)
        {
            if (!ModelState.IsValid)
                return NotFound("Modelo não é válido");

            try
            {
                var contextUser = _userContext.GetContextUser();

                var user = _mapper.Map<User>(model);
                await _userRepository.AddUserAsync(contextUser, user);

                return Ok(user.Id);
            }
            catch (Exception e)
            {
                return LoggerBadRequest(e, _logger);
            }
        }

        /// <summary>
        /// Atualiza um registro de um usuário
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Policy.Admin)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UserUpdateModel updateModel)
        {
            if (!ModelState.IsValid)
                return NotFound("Modelo não é válido");

            try
            {
                var contextUser = _userContext.GetContextUser();

                var user = await _userRepository.GetUserByIdAsync(contextUser, id);
                _mapper.Map(updateModel, user);

                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = contextUser.GetCurrentUser();

                await _userRepository.UpdateUserAsync(contextUser, user);
            }
            catch (Exception e)
            {
                return LoggerBadRequest(e, _logger);
            }

            return NoContent();
        }

        /// <summary>
        /// Exclui um registro de um usuário
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var contextUser = _userContext.GetContextUser();

                var User = await _userRepository.GetUserByIdAsync(contextUser, id);
                if (User == null)
                    return NotFound("Usuário não encontrado");

                User.IsDeleted = true;
                User.UpdatedAt = DateTime.Now;
                User.UpdatedBy = contextUser.GetCurrentUser();

                await _userRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return LoggerBadRequest(e, _logger);
            }
        }
    }
}
