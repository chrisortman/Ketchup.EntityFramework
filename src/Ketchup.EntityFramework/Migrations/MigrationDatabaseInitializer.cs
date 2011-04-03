namespace Ketchup.EntityFramework.Migrations {
	using System;
	using System.Data.Entity;
	using Ketchup.EntityFramework.Migrations.Provider.SqlCe40;
	using Ketchup.EntityFramework.Migrations.Runner;

	public class MigrationDatabaseInitializer<CONTEXT> : IDatabaseInitializer<CONTEXT> where CONTEXT : DbContext {

		private bool _alwaysRecreateTheDatabase;

		public MigrationDatabaseInitializer(bool alwaysRecreateTheDatabase = false) {
			_alwaysRecreateTheDatabase = alwaysRecreateTheDatabase;
		}

		public void InitializeDatabase(CONTEXT context) {
			var emptyContext = new DbContextWithoutTables(context.Database.Connection.ConnectionString);

			if(_alwaysRecreateTheDatabase && emptyContext.Database.Exists()) {
				emptyContext.Database.Delete();
			}

			emptyContext.Database.CreateIfNotExists();

			var migrator = new Migrator(
				provider:new SqlCe4TransformationProvider(context.Database.Connection.ConnectionString),
				migrationAssembly: Migrator.DiscoverMigrationAssembly(),
				trace:true);

			migrator.MigrateToLastVersion();
		}

		//Use this guy so that we can get the database
		//created but not have any tables or anything
		//get created when we do
		private class DbContextWithoutTables : DbContext {
			public DbContextWithoutTables(string nameOrConnectionString) : base(nameOrConnectionString) {}
		}
	}
}