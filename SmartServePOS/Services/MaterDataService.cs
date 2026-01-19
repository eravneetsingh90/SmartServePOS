using AutoMapper;
using Microsoft.Data.Sqlite;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public class MasterDataService : IMasterDataService
	{
		private readonly IMapper _mapper;
		private readonly ISqliteConnectionFactory _connectionFactory;
		private readonly IProductService _productService;
		private readonly IRestaurantTableStore _tableStore;
		private readonly ITableStatusStore _tableStatusStore;
		public MasterDataService(IMapper mapper,
			ISqliteConnectionFactory connectionFactory,
			IProductService productService,
			IRestaurantTableStore tableStore,
			ITableStatusStore tableStatusStore)
		{
			_mapper = mapper;
			_connectionFactory = connectionFactory;
			_productService = productService;
			_tableStore = tableStore;
			_tableStatusStore = tableStatusStore;
		}

		#region public methods
		public async Task SyncAsync()
		{
			using var connection = _connectionFactory.CreateConnection();
			using var tx = connection.BeginTransaction();
			await SyncRestaurantTables(connection);
			await SyncTableStatus(connection);
			await SyncCategories(connection);
			await SyncProducts(connection);
			await SyncProductVariants(connection);
			tx.Commit();
		}
		public async Task<List<CategoryDto>> GetCategoriesAsync()
		{
			var result = new List<CategoryDto>();

			using var connection = _connectionFactory.CreateConnection();
			using var command = connection.CreateCommand();

			command.CommandText = @"
								SELECT
									Id,
									LocalId,
									name,
									is_active,
									display_order
								FROM categories
								WHERE is_active = 1
								ORDER BY display_order, name;
								";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(new CategoryDto
				{
					Id = reader.GetInt32(0),
					LocalId = reader.GetInt32(1),
					Name = reader.GetString(2),
					IsActive = reader.GetInt32(3) == 1,
					DisplayOrder = reader.GetInt32(4)
				});
			}

			return result;
		}
		public async Task<List<ProductDto>> GetProductsAsync()
		{
			var result = new List<ProductDto>();

			using var connection = _connectionFactory.CreateConnection();
			using var command = connection.CreateCommand();

			command.CommandText = @"
								SELECT
									Id,
									LocalId,
									name,
									category_id,
									is_active,
									display_order,
									food_type
								FROM products
								WHERE is_active = 1
								ORDER BY display_order, name;
								";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(new ProductDto
				{
					Id = reader.GetInt32(0),
					LocalId = reader.GetInt32(1),
					Name = reader.GetString(2),
					CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
					IsActive = reader.GetInt32(4) == 1,
					DisplayOrder = reader.GetInt32(5),
					FoodType = reader.IsDBNull(6) ? null : reader.GetString(6)
				});
			}

			return result;
		}
		public async Task<List<ProductVariantDto>> GetProductVariantsAsync()
		{
			var result = new List<ProductVariantDto>();

			using var connection = _connectionFactory.CreateConnection();
			using var command = connection.CreateCommand();

			command.CommandText = @"
								SELECT
									Id,
									LocalId,
									product_id,
									brand_id,
									variant_name,
									price,
									is_active,
									display_order
								FROM product_variants
								WHERE is_active = 1
								ORDER BY display_order, variant_name;
								";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(new ProductVariantDto
				{
					Id = reader.GetInt32(0),
					LocalId = reader.GetInt32(1),
					ProductId = reader.GetInt32(2),
					BrandId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
					VariantName = reader.GetString(4),
					Price = reader.GetDecimal(5),
					IsActive = reader.GetInt32(6) == 1,
					DisplayOrder = reader.GetInt32(7)
				});
			}

			return result;
		}

		public async Task<List<TableStatusDto>> GetTableStatusAsync()
		{
			var result = new List<TableStatusDto>();

			using var connection = _connectionFactory.CreateConnection();
			using var command = connection.CreateCommand();

			command.CommandText = @"
								SELECT
									Id,
									LocalId,
									status_code,
									status_name,
									color_hex
								FROM table_status
								ORDER BY status_code;
								";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(new TableStatusDto
				{
					Id = reader.GetInt32(0),
					LocalId = reader.GetInt32(1),   
					StatusCode = reader.GetString(2),
					StatusName = reader.IsDBNull(3) ? null : reader.GetString(3),
					ColorHex = reader.IsDBNull(4) ? null : reader.GetString(4)
				});
			}

			return result;
		}

		#endregion

		#region private methods
		private async Task SyncRestaurantTables(SqliteConnection connection)
		{
			var tables = _mapper.Map<List<RestaurantTableDto>>(await _tableStore.GetActiveRestaurantTablesAsync());

			foreach (var t in tables)
			{
				using var cmd = connection.CreateCommand();
				cmd.CommandText = @"
				INSERT INTO restaurant_tables (Id, display_name, is_active, UpdatedOn)
				VALUES (@Id, @Name, @IsActive, @UpdatedOn)
				ON CONFLICT(Id) DO UPDATE SET
					display_name = excluded.display_name,
					is_active = excluded.is_active,
					UpdatedOn = excluded.UpdatedOn;
				";
				cmd.Parameters.AddWithValue("@Id", t.Id);
				cmd.Parameters.AddWithValue("@Name", t.DisplayName);
				cmd.Parameters.AddWithValue("@IsActive", 1);
				cmd.Parameters.Add(new SqliteParameter(
				"@UpdatedOn",
				t.UpdatedOn == default
					? DBNull.Value
					: t.UpdatedOn));
				cmd.ExecuteNonQuery();
			}
		}
		private async Task SyncTableStatus(SqliteConnection connection)
		{
			var statuses = _mapper.Map<List<TableStatusDto>>(
				await _tableStatusStore.GetAllAsync());
			try
			{
				foreach (var s in statuses)
				{
					using var cmd = connection.CreateCommand();

					cmd.CommandText = @"
				INSERT INTO table_status
					(Id, status_code, status_name, color_hex)
				VALUES
					(@Id, @Code, @Name, @Color)
				ON CONFLICT(Id) DO UPDATE SET
					status_code = excluded.status_code,
					status_name = excluded.status_name,
					color_hex   = excluded.color_hex;
				";

					cmd.Parameters.Add(new SqliteParameter("@Id", s.Id));
					cmd.Parameters.Add(new SqliteParameter("@Code", s.StatusCode));
					cmd.Parameters.Add(new SqliteParameter(
						"@Name",
						string.IsNullOrWhiteSpace(s.StatusName)
							? DBNull.Value
							: s.StatusName));
					cmd.Parameters.Add(new SqliteParameter(
						"@Color",
						string.IsNullOrWhiteSpace(s.ColorHex)
							? DBNull.Value
							: s.ColorHex));
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				var dd = ex;
			}
		}
		private async Task SyncCategories(SqliteConnection connection)
		{
			var categories = _mapper.Map<List<CategoryDto>>(await _productService.GetActiveCategoriesAsync());
			foreach (var c in categories)
			{
				using var cmd = connection.CreateCommand(); ;
				cmd.CommandText = @"
				INSERT INTO categories (Id, name, display_order, is_active, UpdatedOn)
				VALUES (@Id, @Name, @Order, @Active, @UpdatedOn)
				ON CONFLICT(Id) DO UPDATE SET
					name = excluded.name,
					display_order = excluded.display_order,
					is_active = excluded.is_active,
					UpdatedOn = excluded.UpdatedOn;
				";
				cmd.Parameters.AddWithValue("@Id", c.Id);
				cmd.Parameters.AddWithValue("@Name", c.Name);
				cmd.Parameters.AddWithValue("@Order", c.DisplayOrder);
				cmd.Parameters.AddWithValue("@Active", c.IsActive ? 1 : 0);
				cmd.Parameters.Add(new SqliteParameter(
				"@UpdatedOn",
				c.UpdatedOn == default
					? DBNull.Value
					: c.UpdatedOn));
				cmd.ExecuteNonQuery();
			}
		}
		private async Task SyncProducts(SqliteConnection connection)
		{
			try
			{
				var products = _mapper.Map<List<ProductDto>>(await _productService.GetActiveProductsAsync());
				foreach (var p in products)
				{
					using var cmd = connection.CreateCommand();
					cmd.CommandText = @"
					INSERT INTO products (Id, name, category_id, food_type,display_order, is_active, UpdatedOn)
					VALUES (
						@Id,
						@Name,
						@CategoryServerId,
						@FoodType,
						@Order,
						@Active,
						@UpdatedOn
					)
					ON CONFLICT(Id) DO UPDATE SET
						name = excluded.name,
						category_id = excluded.category_id,
						food_type = excluded.food_type,
						display_order = excluded.display_order,
						is_active = excluded.is_active,
						UpdatedOn = excluded.UpdatedOn;
					";
					cmd.Parameters.AddWithValue("@Id", p.Id);
					cmd.Parameters.AddWithValue("@Name", p.Name);
					cmd.Parameters.Add(new SqliteParameter(
				"@CategoryServerId",
				p.CategoryId > 0 ? p.CategoryId : (object)DBNull.Value));

					cmd.Parameters.Add(new SqliteParameter(
				"@FoodType",
				string.IsNullOrWhiteSpace(p.FoodType)
					? DBNull.Value
					: p.FoodType));
					cmd.Parameters.AddWithValue("@Order", p.DisplayOrder);
					cmd.Parameters.AddWithValue("@Active", p.IsActive ? 1 : 0);
					cmd.Parameters.Add(new SqliteParameter(
				"@UpdatedOn",
				p.UpdatedOn == default
					? DBNull.Value
					: p.UpdatedOn));
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				var exc = ex;
			}
		}
		private async Task SyncProductVariants(SqliteConnection connection)
		{
			try
			{
				var variants = _mapper.Map<List<ProductVariantDto>>(await _productService.GetActiveProductVariantsAsync());

				foreach (var v in variants)
				{
					using var cmd = connection.CreateCommand();
					cmd.CommandText = @"
								INSERT INTO product_variants
								(Id, product_id, brand_id, variant_name, price,display_order, is_active, UpdatedOn)
								VALUES (
									@Id,
									@ProductServerId,
									@BrandServerId,
									@Name,
									@Price,
									@Order,
									@Active,
									@UpdatedOn
								)
								ON CONFLICT(Id) DO UPDATE SET
									product_id = excluded.product_id,
									brand_id = excluded.brand_id,
									variant_name = excluded.variant_name,
									price = excluded.price,
									display_order = excluded.display_order,
									is_active = excluded.is_active,
									UpdatedOn = excluded.UpdatedOn;
								";
					cmd.Parameters.AddWithValue("@Id", v.Id);
					cmd.Parameters.AddWithValue("@ProductServerId", v.ProductId);
					cmd.Parameters.AddWithValue("@BrandServerId", (object?)v.BrandId ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@Name", v.VariantName);
					cmd.Parameters.AddWithValue("@Price", v.Price);
					cmd.Parameters.AddWithValue("@Order", v.DisplayOrder);
					cmd.Parameters.AddWithValue("@Active", v.IsActive ? 1 : 0);
					cmd.Parameters.AddWithValue("@UpdatedOn", v.UpdatedOn);
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				var hh = ex;
			}
		}
		#endregion
	}
}
