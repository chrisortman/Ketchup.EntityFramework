using System;
using Migrator.Framework;
using WebDBU;

namespace ExampleMigrations {
	[Migration(001)]
	public class AddCustomerTable : AwesomeMigration{
		public override void Up() {
			CreateTable("Customers",t => {
				t.String("Name");
				t.DateTime("Birthday");
			});
		}

		public override void Down() {
			RemoveTable("Customers");
		}
	}
}