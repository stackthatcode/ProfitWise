using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;

namespace Push.Foundation.Web.Shopify
{
    public class OwinUserService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IPushLogger _logger;

        public OwinUserService(ApplicationUserManager userManager, IPushLogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> CreateNewUser(ApplicationUser user, UserLoginInfo loginInfo)
        {
            var createUserResult = await _userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                _logger.Error(
                    $"Unable to create new User for {user.Email}/{user.UserName} - " +
                    $"{createUserResult.Errors.StringJoin(";")}");
                return false;
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user.Id, SecurityConfig.UserRole);
            if (!addToRoleResult.Succeeded)
            {
                _logger.Error(
                    $"Unable to add User {user.Email}/{user.UserName} to {SecurityConfig.UserRole} - " +
                    $"{createUserResult.Errors.StringJoin(";")}");
                return false;
            }

            var addLoginResult = await _userManager.AddLoginAsync(user.Id, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                _logger.Error(
                    $"Unable to add Login for User {user.Email}/{user.UserName} - " +
                    $"{addLoginResult.Errors.StringJoin(";")}");
                return false;
            }

            return true;
        }
    }
}
