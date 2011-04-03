namespace Ketchup.EntityFramework.Migrations {
	using System;
	using System.Collections.Generic;
	using System.Data;

	public interface ISchemaInfoProvider {
		void LoadMigrations(List<long> appliedMigrations);
		void RecordMigration(long version);
		void RecordMigrationUnApplied(long version);
		void CreateTableIfYouHaventAlready();
	}

	public class DefaultSchemaInfoProvider : ISchemaInfoProvider {
		public DefaultSchemaInfoProvider(ITransformationProvider transformationProvider) {
			Provider = transformationProvider;
		}

		private ITransformationProvider Provider { get; set; }

		public void LoadMigrations(List<long> appliedMigrations) {
			using (IDataReader reader = Provider.Select("version", "SchemaInfo")) {
				while (reader.Read()) {
					appliedMigrations.Add(Convert.ToInt64(reader.GetValue(0)));
				}
			}
		}

		public void RecordMigration(long version) {
			Provider.Insert("SchemaInfo", new string[] {"version"}, new string[] {version.ToString()});
		}

		public void RecordMigrationUnApplied(long version) {
			Provider.Delete("SchemaInfo", "version", version.ToString());
		}

		public void CreateTableIfYouHaventAlready() {
			if (!Provider.TableExists("SchemaInfo")) {
				Provider.AddTable("SchemaInfo", new Column("Version", DbType.Int64, ColumnProperty.PrimaryKey));
			}
		}
	}
}