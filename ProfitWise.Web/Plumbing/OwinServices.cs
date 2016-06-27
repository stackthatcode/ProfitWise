using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Push.Foundation.Web.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ProfitWise.Web.Plumbing
{
    public class OwinServices
    {
        private readonly Controller _controller;
        private readonly HttpContextBase _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public OwinServices(Controller controller)
        {
            _context = controller.HttpContext;
        }

        public OwinServices(HttpContextBase context)
        {
            _context = context;
        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? _context.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? _context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager => _context.GetOwinContext().Authentication;

        public void Cleanup()
        {
            if (_userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            if (_signInManager != null)
            {
                _signInManager.Dispose();
                _signInManager = null;
            }
        }
    }
}