using System;
using Ketchup.EntityFramework.Migrations.Builders;

namespace Ketchup.EntityFramework.Migrations
{
    public abstract class AwesomeMigration : Migration
    {
        protected void RemoveTable(params string[] tables)
        {
            foreach(var t in tables)
            {
                Database.RemoveTable(t);
            }
        }

        public void CreateTable(string tableName, Action<TableBuilder> table_setup)
        {
            var builder = new TableBuilder(tableName);
            table_setup(builder);
            builder.Execute(this.Database);
        }

        public void AlterTable(string tableName, Action<ChangeTableBuilder> table_setup)
        {
            var builder = new ChangeTableBuilder(tableName);
            table_setup(builder);
            builder.Execute(this.Database);
        }
    }

    public class ExampleMigration : AwesomeMigration
    {
        public override void Up()
        {
            CreateTable("SiteSettings",t =>
            {
                t.Id();
                t.String("CompanyTitle", "HomePageUrl").Null().Length(255);
                t.Boolean("EnableDirectory", "EnableHttps").NotNull().Default(false);
                t.Binary("CompanyLogo");
            });
        }

        public override void Down()
        {
            
        }
    }
}