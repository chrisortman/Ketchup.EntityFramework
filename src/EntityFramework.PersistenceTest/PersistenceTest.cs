using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.PersistenceTest {
	public interface ISetupEntityForTests<ENTITY> {
		PersistenceCheck<ENTITY> Property<VALUE>(Expression<Func<ENTITY, VALUE>> accessor, VALUE value);
	}

	[TestClass]
	public abstract class PersistenceTest<CONTEXT> where CONTEXT : DbContext {
		public const string TESTDB_CONNECTION = "TestDb";

		private CONTEXT _efContext;

		/// <summary>
		/// Allows you to access the current
		/// Entity Framework context.
		/// </summary>
		protected CONTEXT EfContext {
			get { return _efContext; }
			set { _efContext = value; }
		}

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		//Have this so I can do begin trans / commit trans
		//stuff if i need to
		/// <summary>
		/// Wraps all changes withing an EF context and automatically
		/// pushes changes to the database afterward.
		/// </summary>
		/// <param name="stuff"></param>
		/// <example>
		/// <code>
		/// UnitOfWork(context => 
		/// {
		///     var entity = new Customer();
		///     entity.Name = "chris";
		///     context.Customers.Add(entity);
		/// });
		/// </code>
		/// </example>
		/// <seealso cref="EfContext"/>
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

				Assert.Fail(message);
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
		/// Causes any changes in the current Entity Framework 
		/// context to be sent to the database.
		/// </summary>
		/// <seealso cref="DbContext.SaveChanges"/>
		/// <seealso cref="CreateContext"/>
		/// <seealso cref="EfContext" />
		protected void FlushChanges() {
			EfContext.SaveChanges();
		}

		/// <summary>
		/// Helper function for doing common persistence checking.
		/// Will assign values to properties, send to database fetch back
		/// and check that everything matches.
		/// </summary>
		/// <typeparam name="ENTITY">Type of the entity object</typeparam>
		/// <param name="configureEntity">Function that will setup properties on the entity</param>
		/// <example>
		/// <code>
		/// <![CDATA[
		///  Check<WebServer>(c =>
		/// {
		///     c.Property(x => x.HostName, "Localhost")
		///         .Property(x => x.Username, "Admin")
		///         .Property(x => x.Password, "welcome")
		///         .Property(x => x.IisVersion,7);
		/// });
		/// ]]>
		/// </code>
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

				checkerInstance.Compare(fromDb, Assert.Fail);
			});
		}

		[TestInitialize]
		public void Setup() {
			try {
				//DbDatabase.SetInitializer(new MigrationDatabaseInitializer<WebDatabase>()
				//{
				//    AlwaysDrop = true, 
				//    ConnectionStringName = TESTDB_CONNECTION,
				//    ConnectionFactory = () => new SqlCeConnection()
				//});
				//DbDatabase.SetInitializer<PaymentSessionContainer>(null); //dont need initializer for this db since it is the same as web, but it will error if we dont do this


				_efContext = CreateContext();
				_efContext.Database.Initialize(true);
			}
			catch (ReflectionTypeLoadException rflEx) {
				foreach (var lex in rflEx.LoaderExceptions) {
					TestContext.WriteLine(lex.ToString());
				}
				throw;
			}
		}

		/// <summary>
		/// Allows you to create an Entity Framework context
		/// for use in your tests.
		/// </summary>
		/// <returns></returns>
		/// <example>
		/// <code>
		/// using(var ctx = CreateContext())
		/// {
		///     var siteUrl = SiteUrl.CreateDefault("ebill",ctx.WebServers);
		///     ctx.SiteUrls.Add(siteUrl);
		///     ctx.SaveChanges();
		/// }
		/// </code>
		/// </example>
		protected CONTEXT CreateContext() {
			return Activator.CreateInstance<CONTEXT>();
		}

		[TestCleanup]
		public void Teardown() {
			EfContext.Dispose();
		}
	}
}