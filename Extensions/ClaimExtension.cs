﻿using System.Security.Claims;
using SongApi.Models;

namespace SongApi.Extensions;

public static class ClaimExtension
{
    public static IEnumerable<Claim> GetClaims(this User user)
    {
        var result = new List<Claim>
        {
            new (ClaimTypes.Name,user.Email)
        };
        result.AddRange(user.Roles.Select(role=> new Claim(ClaimTypes.Role, role.Slug)));
        return result;
    }

    public static string GetUserEmail(this ClaimsPrincipal? user)
    {
        return user?.FindFirst(ClaimTypes.Name)?.Value!;
    }
}