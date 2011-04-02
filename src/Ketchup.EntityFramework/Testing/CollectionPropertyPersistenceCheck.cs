using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ketchup.EntityFramework.Testing {
	internal class CollectionPropertyPersistenceCheck<COLLECTION_TYPE> : PropertyPersistenceCheck {
		public override void Compare(object entity, Action<string> onFail) {
			var entityValue = Property.GetValue(entity, null);
			var expected = new ArrayList();
			foreach (var item in (IEnumerable) ValueToSet) {
				expected.Add(item);
			}

			if (ValueToSet != null && entityValue == null) {
				onFail("Entity collection was NULL but was not expected to be");
			}

			var actual = (ICollection<COLLECTION_TYPE>) entityValue;

			if (actual == null) {
				onFail(String.Format("Expected a non-null collection type for {0}", Property.Name));
				return;
			}
			if (expected.Count != actual.Count) {
				onFail(String.Format("Sizes of collections differed.\n Expected {0}\n Got {1}", expected.Count,
				                     actual.Count));
			}

			for (int i = 0; i < expected.Count; i++) {
				if (!actual.Contains((COLLECTION_TYPE) expected[i])) {
					onFail(String.Format("Collections differed at position {0}.\n Expected {1}\n Got: {2}.", i, expected[i],
					                     actual.ElementAt(i)));
				}
			}
		}
	}
}