using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuthSandbox.Controllers
{
    public class _InterestingCodeDump
    {
        // Save this flow for the auto generation of Admin User

        // POST: /AdminAuth/Register
        // var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        // var result = await UserManager.CreateAsync(user, model.Password);
        // if (result.Succeeded)
        // {
        //    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        // Send an email with this link
        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");



        //// GET: /Manage/ManageLogins
        //public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        //{
        //    ViewBag.StatusMessage =
        //        message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //        : message == ManageMessageId.Error ? "An error has occurred."
        //        : "";
        //    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());

        //    var otherLogins =
        //        AuthenticationManager
        //            .GetExternalAuthenticationTypes()
        //            .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider))
        //            .ToList();

        //    ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
        //    return View(new ManageLoginsViewModel
        //    {
        //        CurrentLogins = userLogins,
        //        OtherLogins = otherLogins
        //    });
        //}


    }
}