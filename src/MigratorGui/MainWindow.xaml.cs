using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
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

namespace MigratorGui {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private IDbConnection _connection;

		public MainWindow() {
			InitializeComponent();
		}

		private void ConnectToDatabase(object sender, RoutedEventArgs e) {
			try {
				var connectionString = DatabaseConnectionStringField.Text;
				if (connectionString.IndexOf("Initial Catalog", StringComparison.InvariantCultureIgnoreCase) > -1) {
					//probably SqlServer or express then
					_connection = new SqlConnection(connectionString);
				}
				else {
					_connection = new SqlCeConnection(connectionString);
				}
				_connection.Open();
				if (_connection is SqlConnection) {
					var bldr = new SqlConnectionStringBuilder(connectionString);
					Title = String.Format("Connected - {0} on {1}", bldr.InitialCatalog, bldr.DataSource);
				}
				else {
					var bldr = new SqlCeConnectionStringBuilder(connectionString);
					Title = String.Format("Connected - SQLCE {0}", bldr.DataSource);
				}
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

		private void AddMessage(string message) {
			ConsoleMessagesField.Text += message + Environment.NewLine;
			ConsoleScroller.ScrollToEnd();
		}
	}
}