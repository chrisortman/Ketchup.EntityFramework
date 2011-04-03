namespace Ketchup.EntityFramework.Testing {
	using System;
	using System.Linq.Expressions;

	public interface ISetupEntityForTests<ENTITY> {
		PersistenceCheck<ENTITY> Property<VALUE>(Expression<Func<ENTITY, VALUE>> accessor, VALUE value);
	}
}