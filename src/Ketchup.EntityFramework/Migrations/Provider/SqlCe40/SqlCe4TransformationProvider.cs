using System;
using System.Data;
using Ketchup.EntityFramework.Migrations.Provider.SqlCe40;
using Migrator.Framework;
using Migrator.Providers.SqlServer;

namespace Migrator.Providers.SqlCe40 {
	public class SqlCe4TransformationProvider : SqlServerTransformationProvider {
		private Func<IDbConnection> _connectionFactory;

		public SqlCe4TransformationProvider(Func<IDbConnection> connectionFactory, string connectionString)
			: base(new SqlCe4Dialect(), connectionString) {
			_connectionFactory = connectionFactory;
			CreateConnection();
			CloseConnectionOnCommit = true;
		}

		protected override void CreateConnection() {
			//this gets called form the superclass ctor
			//and will fail until we've had a chance to initialize
			//_connectionFactory so we just exit and then
			//this will get recalled from our ctor

			if (_connectionFactory == null) {
				return;
			}
			_connection = _connectionFactory();
			_connection.ConnectionString = _connectionString;
			_connection.Open();
		}


		// FIXME: We should look into implementing this with INFORMATION_SCHEMA if possible
		// so that it would be usable by all the SQL Server implementations
		public override bool ConstraintExists(string table, string name) {
			using (IDataReader reader =
				ExecuteQuery(
					string.Format(
						"SELECT TOP 1 * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = '{0}'",
						name))) {
				return reader.Read();
			}
		}

		public override bool TableExists(string table) {
			string tableWithoutBrackets = this.RemoveBrackets(table);

			string tableName = this.GetTableName(tableWithoutBrackets);
			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{0}'",
				                           tableName))) {
				return reader.Read();
			}
		}


		public virtual void ChangeColumn(string table, string sqlColumn) {
			//have to strip the [dbo] crap that we put on in our AwesomeMigration...wish i could remember why it is there in the first place
			table = CleanTableName(table);
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ALTER COLUMN {1}", table, sqlColumn));
		}

		private string CleanTableName(string table) {
			if (table.StartsWith("[dbo].")) {
				table = table.Substring(6);
			}

			return this.Dialect.Quote(table);
		}


		public override void RenameTable(string oldName, string newName) {
			oldName = CleanTableName(oldName);
			newName = CleanTableName(newName);

			if (TableExists(newName)) {
				throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));
			}

			if (TableExists(oldName)) {
				ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME TO {1}", oldName, newName));
			}
		}

		public override void RenameColumn(string tableName, string oldColumnName, string newColumnName) {
			tableName = CleanTableName(tableName);
			if (ColumnExists(tableName, newColumnName)) {
				throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName,
				                                           newColumnName));
			}

			if (ColumnExists(tableName, oldColumnName)) {
				var c = GetColumnByName(RemoveBrackets(tableName), RemoveBrackets(oldColumnName));
				AddColumn(tableName, "[" + newColumnName + "] " + c.TypeString);
				ExecuteNonQuery(String.Format("UPDATE {0} set [{1}] = [{2}]", tableName, newColumnName,
				                              oldColumnName));


				DeleteColumnConstraints(tableName, oldColumnName);
				RemoveColumn(tableName, oldColumnName);
			}
		}

		// FIXME: We should look into implementing this with INFORMATION_SCHEMA if possible
		// so that it would be usable by all the SQL Server implementations
		protected override string FindConstraints(string table, string column) {
			return String.Format(
				"select CONSTRAINT_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}'",
				RemoveBrackets(table), column);
		}

		public override void AddColumn(string table, string sqlColumn) {
			table = CleanTableName(table);
			ExecuteNonQuery(String.Format("ALTER TABLE {0} ADD COLUMN {1}", table, sqlColumn));
		}

		public override void RemoveColumn(string table, string column) {
			table = CleanTableName(table);
			if (ColumnExists(table, column)) {
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN {1} ", table, column));
			}
		}


		public override void RemoveConstraint(string table, string name) {
			table = CleanTableName(table);
			if (TableExists(table) && ConstraintExists(table, name)) {
				table = _dialect.TableNameNeedsQuote ? _dialect.Quote(table) : table;
				name = _dialect.ConstraintNameNeedsQuote ? _dialect.Quote(name) : name;
				ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", table, name));
			}
		}

		/// <summary>
		/// Add a new column to an existing table.
		/// </summary>
		/// <param name="table">Table to which to add the column</param>
		/// <param name="column">Column name</param>
		/// <param name="type">Date type of the column</param>
		/// <param name="size">Max length of the column</param>
		/// <param name="property">Properties of the column, see <see cref="ColumnProperty">ColumnProperty</see>,</param>
		/// <param name="defaultValue">Default value</param>
		public override void AddColumn(string table, string column, DbType type, int size, ColumnProperty property,
		                               object defaultValue) {
			table = CleanTableName(table);
			if (ColumnExists(table, column)) {
				Logger.Warn("Column {0}.{1} already exists", table, column);
				return;
			}

			ColumnPropertiesMapper mapper =
				_dialect.GetAndMapColumnProperties(new Column(column, type, size, property, defaultValue));

			AddColumn(table, mapper.ColumnSql);
		}

		public override bool ColumnExists(string table, string column) {
			if (!TableExists(table)) {
				return false;
			}

			string tableWithoutBrackets = this.RemoveBrackets(table);

			string tableName = this.GetTableName(tableWithoutBrackets);
			using (IDataReader reader =
				ExecuteQuery(String.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{0}' AND COLUMN_NAME='{1}'",
				                           tableName, column))) {
				return reader.Read();
			}
		}
	}
}