namespace Ketchup.EntityFramework.Testing {
	using System;
	using System.Data;
	using System.Data.Entity;
	using System.Data.Entity.Infrastructure;
	using System.Data.Entity.Validation;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Reflection;
	using Ketchup.EntityFramework.Migrations;

	public abstract class PersistenceTest<CONTEXT> where CONTEXT : DbContext {
		public const string TESTDB_CONNECTION = "TestDb";

		private CONTEXT _efContext;

		/// <summary>
		///   Allows you to access the current
		///   Entity Framework context.
		/// </summary>
		protected CONTEXT EfContext {
			get { return _efContext; }
			set { _efContext = value; }
		}

		//Have this so I can do begin trans / commit trans
		//stuff if i need to
		/// <summary>
		///   Wraps all changes withing an EF context and automatically
		///   pushes changes to the database afterward.
		/// </summary>
		/// <param name = "stuff"></param>
		/// <example>
		///   <code>
		///     UnitOfWork(context => 
		///     {
		///     var entity = new Customer();
		///     entity.Name = "chris";
		///     context.Customers.Add(entity);
		///     });
		///   </code>
		/// </example>
		/// <seealso cref = "EfContext" />
		protected void UnitOfWork(Action<CONTEXT> stuff) {
			stuff(EfContext);
			try {
				EfContext.SaveChanges();
			}
			catch (DbEntityValidationException validationEx) {
				var message = "Could not save entity\n";
				foreach (var err in validationEx.EntityValidationErrors) {
					foreach (var verr in err.ValidationErrors) {
						message += verr.ErrorMessage + Environment.NewLine;
					}
				}

				TestFrameworkAgnosticFail(message);
			}
			catch (UpdateException updateEx) {
				if (updateEx.InnerException is SqlException) {
					const string knownMessage =
						@"The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value.
The statement has been terminated.";
					if (updateEx.InnerException.Message == knownMessage) {
						throw new ApplicationException(
							"It looks like you forget to initialize one of the date fields in your entity class",
							updateEx);
					}
				}
				throw;
			}
		}

		/// <summary>
		///   Causes any changes in the current Entity Framework 
		///   context to be sent to the database.
		/// </summary>
		/// <seealso cref = "DbContext.SaveChanges" />
		/// <seealso cref = "CreateContext" />
		/// <seealso cref = "EfContext" />
		protected void FlushChanges() {
			EfContext.SaveChanges();
		}

		/// <summary>
		///   Helper function for doing common persistence checking.
		///   Will assign values to properties, send to database fetch back
		///   and check that everything matches.
		/// </summary>
		/// <typeparam name = "ENTITY">Type of the entity object</typeparam>
		/// <param name = "configureEntity">Function that will setup properties on the entity</param>
		/// <example>
		///   <code>
		///     <![CDATA[
		///  Check<WebServer>(c =>
		/// {
		///     c.Property(x => x.HostName, "Localhost")
		///         .Property(x => x.Username, "Admin")
		///         .Property(x => x.Password, "welcome")
		///         .Property(x => x.IisVersion,7);
		/// });
		/// ]]>
		///   </code>
		/// </example>
		protected void Check<ENTITY>(Action<ISetupEntityForTests<ENTITY>> configureEntity) where ENTITY : class {
			var checkerInstance = new PersistenceCheck<ENTITY>();
			configureEntity(checkerInstance);

			UnitOfWork(r => {
				var entity = Activator.CreateInstance<ENTITY>();
				checkerInstance.Update(r, entity);

				r.Set<ENTITY>().Add(entity);
			});

			UnitOfWork(r => {
				var fromDb = r.Set<ENTITY>().First();

				checkerInstance.Compare(fromDb, TestFrameworkAgnosticFail);
			});
		}

		private static void TestFrameworkAgnosticFail(string message) {
			var mstestAssertType =
				Type.GetType(
					"Microsoft.VisualStudio.TestTools.UnitTesting.Assert,Microsoft.VisualStudio.QualityTools.UnitTestFramework, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL",
					false);
			if (mstestAssertType != null) {
				var failMethod = mstestAssertType.GetMethod("Fail", new[] {typeof (string)});
				try {
					failMethod.Invoke(null, new[] {message});
				}
				catch (TargetInvocationException tex) {
					throw tex.InnerException;
				}
			}
		}

		public void SetupEntityFrameworkContext(bool useMigrations = false) {
			
      Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

			if(useMigrations) {
				Database.SetInitializer(new MigrationDatabaseInitializer<CONTEXT>(alwaysRecreateTheDatabase:true));
			}

			_efContext = CreateContext();
			_efContext.Database.Initialize(true);
		}

		/// <summary>
		///   Allows you to create an Entity Framework context
		///   for use in your tests.
		/// </summary>
		/// <returns></returns>
		/// <example>
		///   <code>
		///     using(var ctx = CreateContext())
		///     {
		///     var siteUrl = SiteUrl.CreateDefault("ebill",ctx.WebServers);
		///     ctx.SiteUrls.Add(siteUrl);
		///     ctx.SaveChanges();
		///     }
		///   </code>
		/// </example>
		protected CONTEXT CreateContext() {
			return Activator.CreateInstance<CONTEXT>();
		}

		public void CleanupEntityFrameworkContext() {
			EfContext.Dispose();
		}
	}
}