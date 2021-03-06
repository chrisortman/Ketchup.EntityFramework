﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Ketchup.EntityFramework.Migrations;
using Ketchup.EntityFramework.Migrations.Loggers;
using Ketchup.EntityFramework.Migrations.Provider.SqlCe40;
using Ketchup.EntityFramework.Migrations.Provider.SqlServer;
using Ketchup.EntityFramework.Migrations.Runner;

namespace MigratorGui {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, ILogWriter {
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
			command.CommandText = "select max(version) from SchemaInfo";

			int version = 0;
			try {
				var versionColumnValue = command.ExecuteScalar();
				if(Convert.IsDBNull(versionColumnValue)) {
					version = 0;
				} else {
					version = Convert.ToInt32(versionColumnValue);
				}
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
			_migrationAssembly = Migrator.DiscoverMigrationAssembly();
		}

		private Ketchup.EntityFramework.Migrations.Runner.Migrator GetMigrator() {
			if (_connection is SqlConnection) {
				return new Ketchup.EntityFramework.Migrations.Runner.Migrator(
					new SqlServerTransformationProvider(new SqlServer2005Dialect(), _connection.ConnectionString),
					_migrationAssembly,
					true,
					new Logger(true,this));
			}
			else {
				return new Ketchup.EntityFramework.Migrations.Runner.Migrator(
					new SqlCe4TransformationProvider(_connection.ConnectionString),
					migrationAssembly: _migrationAssembly,
					trace: true,
					logger: new Logger(true,this)
					);
			}
		}

		private void MigrateToLatest(object sender, RoutedEventArgs e) {
			Ketchup.EntityFramework.Migrations.Runner.Migrator migrator = GetMigrator();


			migrator.MigrateToLastVersion();
			SetVersionToLastAppliedMigration(migrator);
		}

		private void MigrateDownOne(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();
			long previousMigrationNumber = 0;
			var previous = from migration in migrator.AppliedMigrations
			               orderby migration descending
			               select migration;
			if(previous.Count() <= 1) {
				previousMigrationNumber = 0;
			} else {
				previousMigrationNumber = previous.Skip(1).First();
			}


			migrator.MigrateTo(previousMigrationNumber);
			SetVersionToLastAppliedMigration(migrator);
		}

		private void MigrateUpOne(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();
			var current = (from migration in migrator.AppliedMigrations
			                orderby migration descending
			                select migration).First();

			migrator.MigrateTo(current+1);
			SetVersionToLastAppliedMigration(migrator);
		}

		private void SetVersionToLastAppliedMigration(Migrator migrator) {
			if(migrator.AppliedMigrations.Count > 0) {
				VersionField.Text = Convert.ToInt32(migrator.AppliedMigrations.Max()).ToString();
			} else {
				VersionField.Text = "0";
			}
		}

		private void MigrateTo(object sender, RoutedEventArgs e) {
			var migrator = GetMigrator();

			migrator.MigrateTo(Convert.ToInt64(MigrateToField.Text));
			SetVersionToLastAppliedMigration(migrator);
		}

		#endregion

		public void Write(string message, params object[] args) {
			ConsoleMessagesField.Text += String.Format(message, args);
		}

		public void WriteLine(string message, params object[] args) {
			AddMessage(String.Format(message,args));
		}
	}
}