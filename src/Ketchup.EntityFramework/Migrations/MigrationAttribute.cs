
using System;
using System.ComponentModel.Composition;

namespace Ketchup.EntityFramework.Migrations
{
    /// <summary>
    /// Describe a migration
    /// </summary>
		[MetadataAttribute]
		[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class MigrationAttribute : ExportAttribute
    {
        private long _version;
        private bool _ignore = false;

    	
    	/// <summary>
        /// Describe the migration
        /// </summary>
        /// <param name="version">The unique version of the migration.</param>	
        public MigrationAttribute(long version) : base(typeof(IMigration))
        {
            Version = version;
        }

        /// <summary>
        /// The version reflected by the migration
        /// </summary>
        public long Version
        {
            get { return _version; }
            private set { _version = value; }
        }

        /// <summary>
        /// Set to <c>true</c> to ignore this migration.
        /// </summary>
        public bool Ignore
        {
            get { return _ignore; }
            set { _ignore = value; }
        }
    }

    public interface IMigrationCapabilities
    {
        long Version { get; }
        bool Ignore { get; }
    }
}
