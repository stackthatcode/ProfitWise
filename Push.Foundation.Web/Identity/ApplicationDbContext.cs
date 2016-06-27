using System.Data.Common;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Push.Foundation.Web.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbConnection connection) 
                : base(connection, contextOwnsConnection: false)
        {
        }
    }
}
