using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AccountingProject.Contracts;
using AccountingProject.Infrastructure;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagement;

        public UsersController(IUserManagementService userManagement)
        {
            _userManagement = userManagement;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> List(CancellationToken cancellationToken)
        {
            var list = await _userManagement.ListAsync(cancellationToken);
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult<UserSummaryDto>> Create(
            [FromBody] AdminCreateUserRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            (UserSummaryDto? user, string? error) = await _userManagement.CreateAsync(request, cancellationToken);
            if (error != null)
                return BadRequest(new { message = error });

            return Created($"/api/users/{user!.Id}", user);
        }

        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> SetPassword(int id, [FromBody] SetPasswordRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Password is required." });

            (bool successPw, string? errPw) =
                await _userManagement.SetPasswordAsync(id, request.Password, cancellationToken);
            if (!successPw && errPw != null && errPw.Equals("User not found.", StringComparison.Ordinal))
                return NotFound(new { message = errPw });
            if (!successPw)
                return BadRequest(new { message = errPw });

            return NoContent();
        }

        [HttpPut("{id:int}/active")]
        public async Task<IActionResult> SetActive(
            int id,
            [FromBody] SetUserActiveRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var sub =
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var currentUserId))
                return Unauthorized(new { message = "Invalid token identity." });

            (bool success, string? error) =
                await _userManagement.SetActiveAsync(id, request.IsActive, currentUserId, cancellationToken);
            if (!success && error != null && error.Equals("User not found.", StringComparison.Ordinal))
                return NotFound(new { message = error });
            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }
    }
}
