using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Ketchup.EntityFramework.App_Start {
    public static class EntityFramework_SqlServerCompact {
        public static void Start() {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }
    }
}
