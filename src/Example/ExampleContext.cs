using System.Data.Entity;

namespace Example {
	public class ExampleContext : DbContext{

		public DbSet<Customer> Customers { get; set; }
	}
}