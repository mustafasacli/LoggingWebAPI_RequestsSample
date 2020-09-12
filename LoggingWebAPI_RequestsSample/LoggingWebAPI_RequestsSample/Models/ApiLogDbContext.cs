using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace LoggingWebAPI_RequestsSample.Models
{
    public class ApiLogDbContext : DbContext
    {
        public ApiLogDbContext() :
            base("data source=.\\SQLEXPRESS;initial catalog=ApiLogDb; Integrated Security=SSPI; MultipleActiveResultSets=True;App=EntityFramework")
        {


        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //base.OnModelCreating(modelBuilder);
        }

        public DbSet<ApiLogEntry> ApiLogEntry { get; set; }
    }
}