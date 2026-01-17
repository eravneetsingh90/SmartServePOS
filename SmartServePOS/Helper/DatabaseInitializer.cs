using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection;

namespace SmartServePOS.Helper
{
	
	public class DatabaseInitializer
	{
		private readonly string _dbPath;

		public DatabaseInitializer(string dbPath)
		{
			_dbPath = dbPath;
		}

		public void Initialize()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

			using var connection = new SqliteConnection($"Data Source={_dbPath}");
			connection.Open();

			var sql = ReadEmbeddedSql("SmartServePOS.Migrations.001_initial_schema.sql");

			using var command = connection.CreateCommand();
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}

		private string ReadEmbeddedSql(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using var stream = assembly.GetManifestResourceStream(resourceName)
				?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
	}

}



