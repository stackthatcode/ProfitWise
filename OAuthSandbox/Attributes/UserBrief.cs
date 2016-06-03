using System;
using System.Collections.Generic;
using Push.Utilities.Web.Identity;

namespace OAuthSandbox.Attributes
{
    public class UserBrief
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }

        public string RolesFlattened => String.Join(",", Roles);
        public bool IsAdmin => Roles != null ? Roles.Contains(SecurityConfig.AdminRole) : false;
    }
}
