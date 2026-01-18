using AutoMapper;
using Microsoft.Data.Sqlite;
using SmartServe.Domain.Services;
using SmartServePOS.Models;

namespace SmartServePOS.Services
{
	public class DataService : IDataService
	{
		private readonly IMapper _mapper;
		private readonly ISqliteConnectionFactory _connectionFactory;
		private readonly IProductService _productService;

		public DataService(IMapper mapper, ISqliteConnectionFactory connectionFactory, IProductService productService)
		{
			_mapper = mapper;
			_connectionFactory = connectionFactory;
			_productService = productService;
		}

		#region public methods
		public async Task SyncAsync()
		{
			var categories = _mapper.Map<List<CategoryDto>>(await _productService.GetActiveCategoriesAsync());
			using var connection = _connectionFactory.CreateConnection();
			using var tx = connection.BeginTransaction();
			//SyncRoles(data.Roles);
			//SyncUsers(data.Users);
			//SyncRestaurantTables(data.RestaurantTables);
			SyncCategories(connection,categories);
			//SyncBrands(data.Brands);
			//SyncProducts(data.Products);
			//SyncProductVariants(data.ProductVariants);

			tx.Commit();
		}

		#endregion

		#region private methods

		//		private void SyncRoles(IEnumerable<RoleDto> roles)
		//		{
		//			foreach (var r in roles)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//				INSERT INTO roles (ServerId, role_name, UpdatedOn)
		//				VALUES (@ServerId, @Name, @UpdatedOn)
		//				ON CONFLICT(ServerId) DO UPDATE SET
		//					role_name = excluded.role_name,
		//					UpdatedOn = excluded.UpdatedOn;
		//				";
		//				cmd.Parameters.AddWithValue("@ServerId", r.Id);
		//				cmd.Parameters.AddWithValue("@Name", r.RoleName);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", r.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}
		//		private void SyncUsers(IEnumerable<UserDto> users)
		//		{
		//			foreach (var u in users)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//INSERT INTO users (ServerId, name, role_id, pin_hash, is_active, UpdatedOn)
		//VALUES (
		//    @ServerId,
		//    @Name,
		//    (SELECT Id FROM roles WHERE ServerId = @RoleServerId),
		//    @PinHash,
		//    @IsActive,
		//    @UpdatedOn
		//)
		//ON CONFLICT(ServerId) DO UPDATE SET
		//    name = excluded.name,
		//    role_id = excluded.role_id,
		//    pin_hash = excluded.pin_hash,
		//    is_active = excluded.is_active,
		//    UpdatedOn = excluded.UpdatedOn;
		//";
		//				cmd.Parameters.AddWithValue("@ServerId", u.Id);
		//				cmd.Parameters.AddWithValue("@Name", u.Name);
		//				cmd.Parameters.AddWithValue("@RoleServerId", u.RoleId);
		//				cmd.Parameters.AddWithValue("@PinHash", u.PinHash);
		//				cmd.Parameters.AddWithValue("@IsActive", u.IsActive ? 1 : 0);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", u.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}
		//		private void SyncRestaurantTables(IEnumerable<RestaurantTableDto> tables)
		//		{
		//			foreach (var t in tables)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//INSERT INTO restaurant_tables (ServerId, display_name, is_active, UpdatedOn)
		//VALUES (@ServerId, @Name, @IsActive, @UpdatedOn)
		//ON CONFLICT(ServerId) DO UPDATE SET
		//    display_name = excluded.display_name,
		//    is_active = excluded.is_active,
		//    UpdatedOn = excluded.UpdatedOn;
		//";
		//				cmd.Parameters.AddWithValue("@ServerId", t.Id);
		//				cmd.Parameters.AddWithValue("@Name", t.DisplayName);
		//				cmd.Parameters.AddWithValue("@IsActive", t.IsActive ? 1 : 0);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", t.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}
		private void SyncCategories(SqliteConnection connection, IEnumerable<CategoryDto> categories)
		{
			foreach (var c in categories)
			{
				using var cmd = connection.CreateCommand();;
				cmd.CommandText = @"
				INSERT INTO categories (ServerId, name, display_order, is_active, UpdatedOn)
				VALUES (@ServerId, @Name, @Order, @Active, @UpdatedOn)
				ON CONFLICT(ServerId) DO UPDATE SET
					name = excluded.name,
					display_order = excluded.display_order,
					is_active = excluded.is_active,
					UpdatedOn = excluded.UpdatedOn;
				";
				cmd.Parameters.AddWithValue("@ServerId", c.Id);
				cmd.Parameters.AddWithValue("@Name", c.Name);
				cmd.Parameters.AddWithValue("@Order", c.DisplayOrder);
				cmd.Parameters.AddWithValue("@Active", c.IsActive ? 1 : 0);
				cmd.Parameters.AddWithValue("@UpdatedOn", c.UpdatedOn);
				cmd.ExecuteNonQuery();
			}
		}
		//		private void SyncBrands(IEnumerable<BrandDto> brands)
		//		{
		//			foreach (var b in brands)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//INSERT INTO brands (ServerId, name, is_active, UpdatedOn)
		//VALUES (@ServerId, @Name, @Active, @UpdatedOn)
		//ON CONFLICT(ServerId) DO UPDATE SET
		//    name = excluded.name,
		//    is_active = excluded.is_active,
		//    UpdatedOn = excluded.UpdatedOn;
		//";
		//				cmd.Parameters.AddWithValue("@ServerId", b.Id);
		//				cmd.Parameters.AddWithValue("@Name", b.Name);
		//				cmd.Parameters.AddWithValue("@Active", b.IsActive ? 1 : 0);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", b.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}
		//		private void SyncProducts(IEnumerable<ProductDto> products)
		//		{
		//			foreach (var p in products)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//INSERT INTO products (ServerId, name, category_id, food_type, is_active, UpdatedOn)
		//VALUES (
		//    @ServerId,
		//    @Name,
		//    (SELECT Id FROM categories WHERE ServerId = @CategoryServerId),
		//    @FoodType,
		//    @Active,
		//    @UpdatedOn
		//)
		//ON CONFLICT(ServerId) DO UPDATE SET
		//    name = excluded.name,
		//    category_id = excluded.category_id,
		//    food_type = excluded.food_type,
		//    is_active = excluded.is_active,
		//    UpdatedOn = excluded.UpdatedOn;
		//";
		//				cmd.Parameters.AddWithValue("@ServerId", p.Id);
		//				cmd.Parameters.AddWithValue("@Name", p.Name);
		//				cmd.Parameters.AddWithValue("@CategoryServerId", p.CategoryId);
		//				cmd.Parameters.AddWithValue("@FoodType", p.FoodType);
		//				cmd.Parameters.AddWithValue("@Active", p.IsActive ? 1 : 0);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", p.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}

		//		private void SyncProductVariants(IEnumerable<ProductVariantDto> variants)
		//		{
		//			foreach (var v in variants)
		//			{
		//				using var cmd = _connection.CreateCommand();
		//				cmd.CommandText = @"
		//INSERT INTO product_variants
		//(ServerId, product_id, brand_id, variant_name, price, is_active, UpdatedOn)
		//VALUES (
		//    @ServerId,
		//    (SELECT Id FROM products WHERE ServerId = @ProductServerId),
		//    (SELECT Id FROM brands WHERE ServerId = @BrandServerId),
		//    @Name,
		//    @Price,
		//    @Active,
		//    @UpdatedOn
		//)
		//ON CONFLICT(ServerId) DO UPDATE SET
		//    product_id = excluded.product_id,
		//    brand_id = excluded.brand_id,
		//    variant_name = excluded.variant_name,
		//    price = excluded.price,
		//    is_active = excluded.is_active,
		//    UpdatedOn = excluded.UpdatedOn;
		//";
		//				cmd.Parameters.AddWithValue("@ServerId", v.Id);
		//				cmd.Parameters.AddWithValue("@ProductServerId", v.ProductId);
		//				cmd.Parameters.AddWithValue("@BrandServerId", (object?)v.BrandId ?? DBNull.Value);
		//				cmd.Parameters.AddWithValue("@Name", v.VariantName);
		//				cmd.Parameters.AddWithValue("@Price", v.Price);
		//				cmd.Parameters.AddWithValue("@Active", v.IsActive ? 1 : 0);
		//				cmd.Parameters.AddWithValue("@UpdatedOn", v.UpdatedOn);
		//				cmd.ExecuteNonQuery();
		//			}
		//		}
#endregion

	}
}
