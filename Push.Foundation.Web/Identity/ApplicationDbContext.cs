using System;
using System.Data.Common;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Push.Foundation.Web.Identity
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public Guid UniqueIdentifier { get; private set; }

        public ApplicationDbContext(DbConnection connection) 
                : base(connection, contextOwnsConnection: false)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
            UniqueIdentifier = Guid.NewGuid();
        }
    }
}
