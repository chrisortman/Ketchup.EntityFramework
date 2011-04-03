using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Ketchup.EntityFramework;
using Example;
using Ketchup.EntityFramework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExampleTest {
	[TestClass]
	public class ManualTests {

		[TestInitialize]
		public void Initialize() {
			
		}
		[TestMethod]
		public void Can_save_and_fetch_a_customer() {
			var context = new ExampleContext();
			var customer = new Customer() {
				Name = "Chris",
				Birthday = new DateTime(1979,12,6)
			};
			context.Customers.Add(customer);
			context.SaveChanges();
			context.Dispose();
		}
	}


	[TestClass]
	public class AutomacTest : PersistenceTest<ExampleContext> {
		
		public void ConfigureTests() {
			ExternalLibraryBoundary.AreTheseTheSameEntity = (e1, e2) => {
				return e1.Equals(e2);
			};

			ExternalLibraryBoundary.GetValueOfId = e => {
				if (e is Customer) {
					return ((Customer) e).ID.ToString();
				}
				else {
					return "";
				}
			};

			ExternalLibraryBoundary.IsThisAnEntityObject = e => {
				return e is Customer;
			};
		}

		[TestInitialize]
		public void Setup() {
			SetupEntityFrameworkContext(useMigrations:true);
		}

		[TestMethod]
		public void Can_save_and_fetch_a_customer() {
			Check<Customer>(c => {
				c.Property(x => x.Name, "Chris");
				c.Property(x => x.Birthday, new DateTime(1979, 12, 6));

			});
		}

		[TestCleanup]
		public void Teardown()
		{
			CleanupEntityFrameworkContext();
		}
	}
}