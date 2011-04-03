using System;
using System.Collections.Generic;
using System.Data;

namespace Ketchup.EntityFramework.Migrations.Builders
{
    public class TableBuilder
    {
        private readonly string _tableName;
        private readonly bool _createTable;
        private List<Func<IEnumerable<Column>>> _columnBuilders;
        private List<Action<ITransformationProvider>> _foreignKeys;

        public TableBuilder(string tableName,bool createTable=true)
        {
            _tableName = tableName;
            _createTable = createTable;
            _columnBuilders = new List<Func<IEnumerable<Column>>>();
            _foreignKeys = new List<Action<ITransformationProvider>>();
        }

        public void Execute(ITransformationProvider database)
        {
            var columnDefinitions = new List<Column>();
            foreach(var builder in _columnBuilders)
            {
                columnDefinitions.AddRange(builder());
            }

            if(_createTable)
            {
                database.AddTable("[dbo]." + _tableName, columnDefinitions.ToArray());
            }
            else
            {
                string queryTableName = "[dbo]." + _tableName;

                BeforeColumnsAreAddedOrChanged(queryTableName,database);

                foreach(var column in columnDefinitions)
                {
                    if(database.ColumnExists(queryTableName,column.Name))
                    {
                        database.ChangeColumn(queryTableName,column);
                    }
                    else
                    {
                        database.AddColumn(queryTableName,column);
                    }
                }

            }

            foreach(var fk in _foreignKeys)
            {
                fk(database);
            }
        }

        /// <summary>
        /// Called when a table is being before columns are added or changed.
        /// </summary>
        /// <param name="queryTableName">the table name qaulified by schema </param>
        /// <param name="database"></param>
        protected virtual void BeforeColumnsAreAddedOrChanged(string queryTableName, ITransformationProvider database)
        {
        }

        public void Id()
        {
            Func<IEnumerable<Column>> builder = () =>
            {
                return new[]
                {
                    new Column("ID", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity)
                };
            };

            _columnBuilders.Add(builder);
        }

        public void GuidId(string columnName = "ID")
        {
            Func<IEnumerable<Column>> builder = () =>
            {
                return new[]
                {
                    new Column(columnName, DbType.Guid, ColumnProperty.PrimaryKey,"newid()")
                };
            };

            _columnBuilders.Add(builder);
        }

        private ColumnBuilder<T> Column<T>(string[] columnNames,DbType columnType,int? length = null)
        {
            var builder = new ColumnBuilder<T>(columnNames,columnType);
            if(length.HasValue)
            {
                builder.Length(length.Value);
            }
            _columnBuilders.Add(builder.GetColumnDefinitions);
            return builder;
        }

        /// <summary>
        /// Creates varchar columns.
        /// The default length is 255.
        /// By default the column will be nullable.
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public ColumnBuilder<string> String(params string[] columnNames)
        {
            return Column<string>(columnNames,DbType.AnsiString);
        }

        public ColumnBuilder<bool> Boolean(params string[] columnNames) 
        {
            return Column<bool>(columnNames,DbType.Boolean);
        }

        public ColumnBuilder<int> Int(params string[] columnNames) 
        {
            return Column<int>(columnNames,DbType.Int32);
        }

        public ColumnBuilder<Guid> Guid(params string[] columnNames)
        {
            return Column<Guid>(columnNames,DbType.Guid);
        }

        public ColumnBuilder<byte[]> Binary(params string[] columnNames)
        {
            return Column<byte[]>(columnNames,DbType.Binary,2147483647);
        }

        public ColumnBuilder<DateTime> DateTime(params string[] columnNames)
        {
            return Column<DateTime>(columnNames,DbType.DateTime);
        }

        /// <summary>
        /// DONT USE THIS ONE
        /// Creates a foreign key to the specified table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void ReferencesWithLegacyNamingSchemeDontUse(string tableName,string columnName = null)
        {
            if(columnName == null)
            {
                
                columnName = tableName + "_ID";
            }
            
            var c = Column<int>(new[]{columnName}, DbType.Int32);
            c.Null().IsForeignKey();
            
            Action<ITransformationProvider> createForeignKey = db =>
            {
                string fkName = string.Format("FK_{0}_{1}", tableName, _tableName);
                db.AddForeignKey(fkName,foreignTable:_tableName, foreignColumn:columnName,
                                        primaryTable:tableName, primaryColumn:"ID");

            };

            _foreignKeys.Add(createForeignKey);
        }

         /// <summary>
        /// DONT USE THIS ONE
        /// Creates a foreign key to the specified table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void References(string tableName,string columnName = null)
        {
            if(columnName == null)
            {
                
                columnName = tableName + "ID";
            }
            
            var c = Column<int>(new[]{columnName}, DbType.Int32);
            c.Null().IsForeignKey();
            
            Action<ITransformationProvider> createForeignKey = db =>
            {
                string fkName = string.Format("FK_{0}_{1}", tableName, _tableName);
                db.AddForeignKey(fkName,foreignTable:_tableName, foreignColumn:columnName,
                                        primaryTable:tableName, primaryColumn:"ID");

            };

            _foreignKeys.Add(createForeignKey);
        }
    }

    public class ChangeTableBuilder : TableBuilder
    {
        private readonly List<string> _columnsToRemove;
        private readonly List<Tuple<string, string>> _columnsToRename;

        public ChangeTableBuilder(string tableName) : base(tableName, false)
        {
            _columnsToRemove = new List<string>();
            _columnsToRename = new List<Tuple<string, string>>();
        }

        public void Remove(params string[] columns)
        {
            foreach (var column in columns)
            {
                _columnsToRemove.Add(column);
            }

            
        }

        protected override void BeforeColumnsAreAddedOrChanged(string queryTableName, ITransformationProvider database)
        {
            foreach(var col in _columnsToRemove)
            {
                database.RemoveColumn(queryTableName, col);
            }

            foreach(var col in _columnsToRename)
            {
                database.RenameColumn(queryTableName,col.Item1,col.Item2);
            }
        }

        public void RenameColumn(string oldColumnName, string newColumnName)
        {
            _columnsToRename.Add(new Tuple<string, string>(oldColumnName,newColumnName));   
        }
    }
}