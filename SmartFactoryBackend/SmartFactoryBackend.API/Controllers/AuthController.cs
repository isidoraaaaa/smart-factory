using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using SmartFactoryBackend.Infrastructure.Persistence;
using SmartFactoryBackend.Infrastructure.Services;

namespace SmartFactoryBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;


        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var token = await _authService.RegisterAsync(request.Name, request.Lastname, request.Username, request.Email, request.Password);

            return token is null
                ? Conflict("Username is already taken.")
                : Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);

            return token is null
                ? Unauthorized("Invalid username or password.")
                : Ok(new { token });
        }
    }
    public record RegisterRequest(string Name, string Lastname, string Username, string Email, string Password);
    public record LoginRequest(string Username, string Password);
}
