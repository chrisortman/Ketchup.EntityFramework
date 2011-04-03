
using System;

namespace Ketchup.EntityFramework.Migrations.Runner
{
	/// <summary>
	/// Exception thrown in a migration <c>Down()</c> method
	/// when changes can't be undone.
	/// </summary>
	public class IrreversibleMigrationException : Exception
	{
		public IrreversibleMigrationException() : base("Irreversible migration")
		{
		}
	}
}
