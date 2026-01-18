using Microsoft.Data.Sqlite;

namespace SmartServePOS.Data
{
		public interface ISqliteConnectionFactory
		{
			SqliteConnection CreateConnection();
		}

		public class SqliteConnectionFactory : ISqliteConnectionFactory
		{
			private readonly string _dbPath;

			public SqliteConnectionFactory(string dbPath)
			{
				if (string.IsNullOrWhiteSpace(dbPath))
					throw new ArgumentException("SQLite DB path cannot be null", nameof(dbPath));

				_dbPath = dbPath;
			}

			public SqliteConnection CreateConnection()
			{
				var connection = new SqliteConnection($"Data Source={_dbPath}");
				connection.Open();
				return connection;
			}
		}
}
