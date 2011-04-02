using System;

namespace EntityFramework.PersistenceTest {

	/// <summary>
	/// Methods needed to be implemented by consumer of library.
	/// </summary>
	public static class ExternalLibraryBoundary {

		/// <summary>
		/// Used to determine if an object is an Entity object that should be saved
		/// independently or if it is a value object that should be saved as part of
		/// another entity.
		/// </summary>
		public static Func<object, bool> IsThisAnEntityObject = o => { return false; };

		/// <summary>
		/// Determine if 2 entities are the same using their ID's
		/// </summary>
		public static Func<object, object, bool> AreTheseTheSameEntity = (e1, e2) => { return false; };

		/// <summary>
		/// Get the ID of the entity.
		/// </summary>
		public static Func<object, string> GetValueOfId = entity => { return ""; };
	}
}