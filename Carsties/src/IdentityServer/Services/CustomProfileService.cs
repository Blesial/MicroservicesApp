using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Services;

public class CustomProfileService : IProfileService
{
	private readonly UserManager<ApplicationUser> _userManager;

	public CustomProfileService(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}
	public async Task GetProfileDataAsync(ProfileDataRequestContext context)
	{
		var user = await _userManager.GetUserAsync(context.Subject);
		var existingClaims = await _userManager.GetClaimsAsync(user);

		var claims = new List<Claim>
		{
			new Claim("username", user.UserName),
		};

		context.IssuedClaims.AddRange(claims);

		context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));

		// Basically we are adding two claims
		// 1. username
		// 2. fullName
	}

	public Task IsActiveAsync(IsActiveContext context)
	{
		return Task.CompletedTask;
	}
}
