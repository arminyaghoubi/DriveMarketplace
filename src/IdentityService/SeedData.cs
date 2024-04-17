using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityService
{
    public class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (userMgr.Users.Any())
            {
                return;
            }

            var armin = userMgr.FindByNameAsync("ArminYaghoubi1").Result;
            if (armin is null)
            {
                armin = new ApplicationUser
                {
                    UserName = "ArminYaghoubi1",
                    Email = "ArminYaghoubi1@gmail.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(armin, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(armin, new Claim[]{
                                new Claim(JwtClaimTypes.Name, "Armin Yaghoubi"),
                            }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("armin created");
            }
            else
            {
                Log.Debug("armin already exists");
            }

            var arash = userMgr.FindByNameAsync("Arash").Result;
            if (arash == null)
            {
                arash = new ApplicationUser
                {
                    UserName = "Arash",
                    Email = "Arash@gmail.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(arash, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(arash, new Claim[]{
                                new Claim(JwtClaimTypes.Name, "Arash Yaghoubi"),
                            }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("arash created");
            }
            else
            {
                Log.Debug("arash already exists");
            }
        }
    }
}
