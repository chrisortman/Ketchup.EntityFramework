using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using Migrator.Providers.SqlCe40;
using Migrator.Providers.SqlServer;

namespace MigratorGui {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private IDbConnection _connection;
		private Assembly _migrationAssembly;

		public MainWindow() {
			InitializeComponent();
		}

		private void ConnectToDatabase(object sender, RoutedEventArgs e) {
			try {
				var connectionString = DatabaseConnectionStringField.Text;

				CreateConnection(connectionString);

				_connection.Open();

				SetConnectedTitle(connectionString);

				DisplaySchemaVersion();
			}
			catch (Exception ex) {
				Title = "Migrator";
				AddMessage("Unable to connect to database");
				AddMessage(ex.ToString());
			}
			finally {
				_connection.Close();
			}
		}

		private void DisplaySchemaVersion() {
			var command = _connection.CreateCommand();
			command.CommandText = "select max(version) from dbo.SchemaInfo";

			int version = 0;
			try {
				version = Convert.ToInt32(command.ExecuteScalar());
			}
			catch (SqlException noSchemaInfoTable) {
				version = 0;
			}
			catch (SqlCeException noSchemaInfotable) {
				version = 0;
			}

			VersionField.Text = version.ToString();
		}

		private void SetConnectedTitle(string connectionString) {
			if (_connection is SqlConnection) {
				var bldr = new SqlConnectionStringBuilder(connectionString);
				Title = String.Format("Connected - {0} on {1}", bldr.InitialCatalog, bldr.DataSource);
			}
			else {
				var bldr = new SqlCeConnectionStringBuilder(connectionString);
				Title = String.Format("Connected - SQLCE {0}", bldr.DataSource);
			}
		}

		private void CreateConnection(string connectionString) {
			if (connectionString.IndexOf("Initial Catalog", StringComparison.InvariantCultureIgnoreCase) > -1) {
				//probably SqlServer or express then
				_connection = new SqlConnection(connectionString);
			}
			else {
				_connection = new SqlCeConnection(connectionString);
			}
		}

		private void AddMessage(string message) {
			ConsoleMessagesField.Text += message + Environment.NewLine;
			ConsoleScroller.ScrollToEnd();
		}


		#region Migrations

		protected void DiscoverMigrations(object sender, RoutedEventArgs e) {
			var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory);
			var container = new CompositionContainer(catalog);
			var migrations = container.GetExports<Migration, IMigrationCapabilities>();
			var assembliesWithMigrations = (from m in migrations
			                                select m.Value.GetType().Assembly).Distinct();

			if (assembliesWithMigrations.Count() > 1) {
				MessageBox.Show("Multiple assembly migrations are not supported.");
			}
			else {
				_migrationAssembly = assembliesWithMigrations.First();
			}
		}

		private Migrator.Migrator GetMigrator() {
			if (_connection is SqlConnection) {
				return new Migrator.Migrator(
					new SqlServerTransformationProvider(new SqlServer2005Dialect(), _connection.ConnectionString),
					_migrationAssembly,
					true,
					new Logger(true));
			}
			else {
				return new Migrator.Migrator(
					new SqlCe4TransformationProvider(() => _connection, _connection.ConnectionString),
					migrationAssembly: _migrationAssembly,
					trace: true,
					logger: new Logger(true)
					);
			}
		}

		private void MigrateToLatest(object sender, RoutedEventArgs e) {
			Migrator.Migrator migrator = GetMigrator();


			migrator.MigrateToLastVersion();
			VersionField.Text = Convert.ToInt32(migrator.AppliedMigrations.Max()).ToString();
		}

		private void MigrateDownOne(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();
			var previous = (from migration in migrator.AppliedMigrations
			                orderby migration descending
			                select migration).Skip(1).First();

			migrator.MigrateTo(previous);
			VersionField.Text = Convert.ToInt32(migrator.AppliedMigrations.Max()).ToString();
		}

		private void MigrateUpOne(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();
			var current = (from migration in migrator.AppliedMigrations
			                orderby migration descending
			                select migration).First();

			migrator.MigrateTo(current+1);
			VersionField.Text = Convert.ToInt32(migrator.AppliedMigrations.Max()).ToString();
		}

		private void MigrateTo(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();

			migrator.MigrateTo(Convert.ToInt64(MigrateToField.Text));
			VersionField.Text = Convert.ToInt32(migrator.AppliedMigrations.Max()).ToString();
		}

		#endregion

	}
}