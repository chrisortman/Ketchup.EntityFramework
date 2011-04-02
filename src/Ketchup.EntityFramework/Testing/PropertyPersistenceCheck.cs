using System;
using System.Data.Entity;
using System.Reflection;

namespace Ketchup.EntityFramework.Testing {
	internal class PropertyPersistenceCheck {
		private object _valueToCompare;

		public PropertyInfo Property { get; set; }
		public Object ValueToSet { get; set; }

		public Object ValueToCompare {
			get {
				if (_valueToCompare == null) {
					return ValueToSet;
				}
				return _valueToCompare;
			}
			set { _valueToCompare = value; }
		}

		public Action<DbContext> SaveFunc { get; set; }

		public virtual void Compare(object entity, Action<string> onFail) {
			var entityValue = Property.GetValue(entity, null);

			if (ExternalLibraryBoundary.IsThisAnEntityObject(ValueToCompare)) {
				if (ExternalLibraryBoundary.AreTheseTheSameEntity(ValueToCompare, entityValue)) {
					onFail(String.Format("References for property {0} do not match.\n Expected ID {1}\n got {2}",
					                     Property.Name,
					                     ExternalLibraryBoundary.GetValueOfId(ValueToCompare),
					                     ExternalLibraryBoundary.GetValueOfId(entityValue)));
				}
			}
			else if (!ValueToCompare.Equals(entityValue)) {
				onFail(String.Format("Values for property {0} did not match. {1} != {2}", Property.Name,
				                     ValueToCompare, entityValue));
			}
		}
	}
}