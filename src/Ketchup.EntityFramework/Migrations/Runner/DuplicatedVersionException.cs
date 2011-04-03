
using System;

namespace Ketchup.EntityFramework.Migrations.Runner
{
	/// <summary>
	/// Exception thrown when a migration number is not unique.
	/// </summary>
	public class DuplicatedVersionException : Exception
	{
		public DuplicatedVersionException(long version)
			: base(String.Format("Migration version #{0} is duplicated", version))
		{
		}
	}
}
