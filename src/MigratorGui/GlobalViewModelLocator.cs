/*
  In App.xaml:
  <Application.Resources>
      <vm:GlobalViewModelLocator xmlns:vm="clr-namespace:MigratorGui"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  
  OR (WPF only):
  
  xmlns:vm="clr-namespace:MigratorGui"
  DataContext="{Binding Source={x:Static vm:GlobalViewModelLocator.ViewModelNameStatic}}"
*/

using GalaSoft.MvvmLight;
using MigratorGui.ViewModels;

namespace MigratorGui {
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// <para>
	/// Use the <strong>mvvmlocatorproperty</strong> snippet to add ViewModels
	/// to this locator.
	/// </para>
	/// <para>
	/// In Silverlight and WPF, place the GlobalViewModelLocator in the App.xaml resources:
	/// </para>
	/// <code>
	/// &lt;Application.Resources&gt;
	///     &lt;vm:GlobalViewModelLocator xmlns:vm="clr-namespace:MigratorGui"
	///                                  x:Key="Locator" /&gt;
	/// &lt;/Application.Resources&gt;
	/// </code>
	/// <para>
	/// Then use:
	/// </para>
	/// <code>
	/// DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
	/// </code>
	/// <para>
	/// You can also use Blend to do all this with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm/getstarted
	/// </para>
	/// <para>
	/// In <strong>*WPF only*</strong> (and if databinding in Blend is not relevant), you can delete
	/// the ViewModelName property and bind to the ViewModelNameStatic property instead:
	/// </para>
	/// <code>
	/// xmlns:vm="clr-namespace:MigratorGui"
	/// DataContext="{Binding Source={x:Static vm:GlobalViewModelLocator.ViewModelNameStatic}}"
	/// </code>
	/// </summary>
	public class GlobalViewModelLocator {
		/// <summary>
		/// Initializes a new instance of the GlobalViewModelLocator class.
		/// </summary>
		public GlobalViewModelLocator() {
			if (ViewModelBase.IsInDesignModeStatic)
			{
			    // Create design time view models
			}
			else
			{
			    // Create run time view models
			}
		}

		private static MainWindowViewModel _mainWindowViewModel;

		/// <summary>
		/// Gets the MainWindowViewModel property.
		/// </summary>
		public static MainWindowViewModel MainWindowViewModelStatic {
			get {
				if(_mainWindowViewModel == null) {
					CreateMainWindowViewModel();
				}

				return _mainWindowViewModel;
			}
		}

		/// <summary>
		/// Gets the MainWindowViewModel property.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
				"CA1822:MarkMembersAsStatic",
				Justification = "This non-static member is needed for data binding purposes.")]
		public MainWindowViewModel MainWindowViewModel {
			get {
				return MainWindowViewModelStatic;
			}
		}

		/// <summary>
		/// Provides a deterministic way to delete the MainWindowViewModel property.
		/// </summary>
		public static void ClearMainWindowViewModel() {
			_mainWindowViewModel.Cleanup();
			_mainWindowViewModel = null;
		}

		/// <summary>
		/// Provides a deterministic way to create the MainWindowViewModel property.
		/// </summary>
		public static void CreateMainWindowViewModel() {
			if(_mainWindowViewModel == null) {
				_mainWindowViewModel = new MainWindowViewModel();
			}
		}

		/// <summary>
		/// Cleans up all the resources.
		/// </summary>
		public static void Cleanup() {
			ClearMainWindowViewModel();
		}

	}
}