using System;
using System.Linq.Expressions;

namespace Ketchup.EntityFramework.Testing {
	public interface ISetupEntityForTests<ENTITY> {
		PersistenceCheck<ENTITY> Property<VALUE>(Expression<Func<ENTITY, VALUE>> accessor, VALUE value);
	}
}