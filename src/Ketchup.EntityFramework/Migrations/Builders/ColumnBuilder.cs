namespace Ketchup.EntityFramework.Migrations.Builders {
	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	///   Used to construct database columns
	/// </summary>
	/// <typeparam name = "T"></typeparam>
	public class ColumnBuilder<T> {
		private readonly string[] _columnNames;
		private ColumnProperty _properties;

		private int? _length;
		private DbType? _columnSqlType;
		private bool _hasDefaultValue = false;
		private T _defaultValue;
		private string _stringDefaultValue = null;

		public ColumnBuilder(string[] columnNames, DbType? columnSqlType = null) {
			_columnNames = columnNames;
			_properties = ColumnProperty.Null;
			_columnSqlType = columnSqlType;
		}

		/// <summary>
		///   Gets the column definitions.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Column> GetColumnDefinitions() {
			foreach (var columnName in _columnNames) {
				var column = new Column(columnName);

				if (_length.HasValue) {
					column.Size = _length.Value;
				}

				if (_columnSqlType.HasValue) {
					column.Type = _columnSqlType.Value;
				}

				if (_hasDefaultValue) {
					if (_stringDefaultValue == null) {
						column.DefaultValue = _defaultValue;
					}
					else {
						column.DefaultValue = _stringDefaultValue;
					}
				}

				column.ColumnProperty = _properties;

				yield return column;
			}
		}

		/// <summary>
		///   Sets the length for the column
		/// </summary>
		/// <param name = "length">The length.</param>
		/// <returns></returns>
		public ColumnBuilder<T> Length(int length) {
			_length = length;
			return this;
		}

		/// <summary>
		///   Sets the column to allow nulls
		/// </summary>
		/// <returns></returns>
		public ColumnBuilder<T> Null() {
			if (_properties.HasFlag(ColumnProperty.NotNull)) {
				_properties = _properties ^ ColumnProperty.NotNull;
			}

			_properties = _properties | ColumnProperty.Null;
			return this;
		}

		/// <summary>
		///   Sets the column to not allow nulls
		/// </summary>
		/// <returns></returns>
		public ColumnBuilder<T> NotNull() {
			if (_properties.HasFlag(ColumnProperty.Null)) {
				_properties = _properties ^ ColumnProperty.Null;
			}
			_properties = _properties | ColumnProperty.NotNull;

			return this;
		}

		/// <summary>
		///   Specifies that the column is a foreign key
		/// </summary>
		/// <returns></returns>
		public ColumnBuilder<T> IsForeignKey() {
			_properties = _properties | ColumnProperty.ForeignKey;
			return this;
		}

		public ColumnBuilder<T> Default(T defaultValue) {
			_hasDefaultValue = true;
			_defaultValue = defaultValue;
			return this;
		}

		public ColumnBuilder<T> Default(string defaultValue) {
			_hasDefaultValue = true;
			_stringDefaultValue = defaultValue;
			return this;
		}
	}
}