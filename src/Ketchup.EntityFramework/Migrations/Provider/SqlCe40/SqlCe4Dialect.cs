using System.Data;
using Ketchup.EntityFramework.Migrations.Provider.SqlServer;

namespace Ketchup.EntityFramework.Migrations.Provider.SqlCe40 {
	public class SqlCe4Dialect : SqlServer2005Dialect
	{
		public SqlCe4Dialect()
		{
            
			RegisterColumnType(DbType.AnsiStringFixedLength, "NCHAR(255)");
			RegisterColumnType(DbType.AnsiStringFixedLength, 8000, "NCHAR($l)");
			RegisterColumnType(DbType.AnsiString, "NVARCHAR(255)");
			RegisterColumnType(DbType.AnsiString, 8000, "NVARCHAR($l)");
			RegisterColumnType(DbType.AnsiString, 2147483647, "NTEXT");
			RegisterColumnType(DbType.Binary, "VARBINARY(8000)");
			//RegisterColumnType(DbType.Binary, 8000, "VARBINARY($l)");
			RegisterColumnType(DbType.Binary, 2147483647, "VARBINARY(8000)");
			//RegisterColumnType(DbType.Boolean, "BIT");
			//RegisterColumnType(DbType.Byte, "TINYINT");
			//RegisterColumnType(DbType.Currency, "MONEY");
			//RegisterColumnType(DbType.Date, "DATETIME");
			//RegisterColumnType(DbType.DateTime, "DATETIME");
			//RegisterColumnType(DbType.Decimal, "DECIMAL(19,5)");
			//RegisterColumnType(DbType.Decimal, 19, "DECIMAL(19, $l)");
			RegisterColumnType(DbType.Double, "FLOAT(53)"); //synonym for FLOAT(53)
			//RegisterColumnType(DbType.Guid, "UNIQUEIDENTIFIER");
			//RegisterColumnType(DbType.Int16, "SMALLINT");
			//RegisterColumnType(DbType.Int32, "INT");
			//RegisterColumnType(DbType.Int64, "BIGINT");
			//RegisterColumnType(DbType.Single, "REAL"); //synonym for FLOAT(24) 
			//RegisterColumnType(DbType.StringFixedLength, "NCHAR(255)");
			//RegisterColumnType(DbType.StringFixedLength, 4000, "NCHAR($l)");
			//RegisterColumnType(DbType.String, "NVARCHAR(255)");
			//RegisterColumnType(DbType.String, 4000, "NVARCHAR($l)");
			//RegisterColumnType(DbType.String, 1073741823, "NTEXT");
			//RegisterColumnType(DbType.Time, "DATETIME");
            
		}

		public override bool TableNameNeedsQuote
		{
			get { return true; }
		}

		public override string Quote(string value)
		{
			if(value.StartsWith("[") == false)
			{
				return "[" + value + "]";
			}
			else if(value.StartsWith("[dbo].")) //schema qualified
			{
				//must strip schema
				var parts = value.Split('.');
				return "[" + parts[1] + "]";
			}
			else
			{
				return value;
			}
		}
	}
}