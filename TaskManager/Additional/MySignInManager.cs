using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Additional
{
    public class MySignInManager : SignInManager<IdentityUser>
    {
        public MySignInManager(Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager, Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor, Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<Microsoft.AspNetCore.Identity.IdentityUser> claimsFactory, Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Identity.IdentityOptions> optionsAccessor, Microsoft.Extensions.Logging.ILogger<Microsoft.AspNetCore.Identity.SignInManager<Microsoft.AspNetCore.Identity.IdentityUser>> logger, Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes, IUserConfirmation<IdentityUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password,
            bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByEmailAsync(userName);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
    }
}
