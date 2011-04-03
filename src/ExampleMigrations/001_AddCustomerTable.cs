using Ketchup.EntityFramework.Migrations;

namespace ExampleMigrations {
	[Migration(001)]
	public class AddCustomerTable : AwesomeMigration{
		public override void Up() {
			CreateTable("Customers",t => {
				t.Id();
				t.String("Name");
				t.DateTime("Birthday");
			});
		}

		public override void Down() {
			RemoveTable("Customers");
		}
	}
}