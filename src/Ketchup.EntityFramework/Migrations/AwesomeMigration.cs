namespace Ketchup.EntityFramework.Migrations {
	using System;
	using Ketchup.EntityFramework.Migrations.Builders;

	public abstract class AwesomeMigration : Migration {
		protected void RemoveTable(params string[] tables) {
			foreach (var t in tables) {
				Database.RemoveTable(t);
			}
		}

		public void CreateTable(string tableName, Action<TableBuilder> table_setup) {
			var builder = new TableBuilder(tableName);
			table_setup(builder);
			builder.Execute(this.Database);
		}

		public void AlterTable(string tableName, Action<ChangeTableBuilder> table_setup) {
			var builder = new ChangeTableBuilder(tableName);
			table_setup(builder);
			builder.Execute(this.Database);
		}
	}
}