using gatherist.web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gatherist.web.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserApi(IUserRepository userRepository)
        : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        [HttpGet("{id:int}")]
        public async Task<IResult> GetAsync(int id)
        {
            var user = await _userRepository.GetUserAsync(id);

            return user == null ? Results.NotFound() : Results.Ok(user);
        }
    }
}
