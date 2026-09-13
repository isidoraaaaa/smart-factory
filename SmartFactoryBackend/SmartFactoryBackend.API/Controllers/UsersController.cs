using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Application.Services;
using System.Security.Claims;

namespace SmartFactoryBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserAsync(id);
            return user == null ? NotFound($"User with id {id} does not exist!") : Ok(user);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var requestingUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(requestingUserIdClaim, out var requestingUserId))
            {
                return Unauthorized();
            }

            try
            {
                var deleted = await _userService.DeleteUserAsync(id, requestingUserId);
                return deleted ? Ok("Successfully deleted!") : NotFound($"User with id {id} does not exist!");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
