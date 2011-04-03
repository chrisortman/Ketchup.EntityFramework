namespace Ketchup.EntityFramework.Testing {
	using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Linq.Expressions;
	using System.Reflection;

	public class PersistenceCheck<ENTITY> : ISetupEntityForTests<ENTITY> {
		private readonly List<PropertyPersistenceCheck> _infos = new List<PropertyPersistenceCheck>();

		public PersistenceCheck<ENTITY> Property<VALUE>(Expression<Func<ENTITY, VALUE>> accessor, VALUE value) {
			return Property(accessor, value, value);
		}

		/// <summary>
		///   Set and verify a property round trip.
		///   This overload allows the expected property to be different
		///   than the inserted proeprty...this is useful for passwords.
		/// </summary>
		/// <typeparam name = "VALUE">The type of the ALUE.</typeparam>
		/// <param name = "accessor">The accessor.</param>
		/// <param name = "valueIn">The value in.</param>
		/// <param name = "valueOut">The value out.</param>
		/// <returns></returns>
		public PersistenceCheck<ENTITY> Property<VALUE>(Expression<Func<ENTITY, VALUE>> accessor, VALUE valueIn,
		                                                VALUE valueOut) {
			var memberExpression = accessor.Body as MemberExpression;
			if (memberExpression != null) {
				var prop = memberExpression.Member as PropertyInfo;
				if (prop != null) {
					_infos.Add(new PropertyPersistenceCheck {
						Property = prop,
						ValueToSet = valueIn,
						ValueToCompare = valueOut
					});
				}
			}

			return this;
		}

		public void Update(DbContext context, ENTITY entity) {
			foreach (var info in _infos) {
				if (ExternalLibraryBoundary.IsThisAnEntityObject(info.ValueToSet)) {
					if (info.SaveFunc != null) {
						info.SaveFunc(context);
					}
				}

				info.Property.SetValue(entity, info.ValueToSet, null);
			}
		}

		public void Compare(ENTITY entity, Action<string> onFail) {
			foreach (var info in _infos) {
				info.Compare(entity, onFail);
			}
		}

		public PersistenceCheck<ENTITY> References<OTHER_ENTITY>(Expression<Func<ENTITY, OTHER_ENTITY>> accessor,
		                                                         OTHER_ENTITY other) where OTHER_ENTITY : class {
			var memberExpression = accessor.Body as MemberExpression;
			if (memberExpression != null) {
				var prop = memberExpression.Member as PropertyInfo;
				if (prop != null) {
					var infos = new PropertyPersistenceCheck {
						Property = prop,
						ValueToSet = other,
						SaveFunc = context => context.Set<OTHER_ENTITY>().Add(other),
					};
					_infos.Add(infos);
				}
			}

			return this;
		}


		public PersistenceCheck<ENTITY> Has<OTHER>(Expression<Func<ENTITY, IEnumerable<OTHER>>> accessor,
		                                           IEnumerable<OTHER> others) {
			var memberExpression = accessor.Body as MemberExpression;
			if (memberExpression != null) {
				var prop = memberExpression.Member as PropertyInfo;
				if (prop != null) {
					var infos = new CollectionPropertyPersistenceCheck<OTHER>() {
						Property = prop,
						ValueToSet = others,
					};
					_infos.Add(infos);
				}
			}
			return this;
		}
	}
}