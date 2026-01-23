using AutoMapper;
using Microsoft.Data.Sqlite;
using SmartServe.Domain.Constants;
using SmartServe.Domain.Models;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Models;
using System.Data;

namespace SmartServePOS.Services
{
    public class POSBillingService : IPOSBillingService
	{
		private readonly IMapper _mapper;
		private readonly ISqliteConnectionFactory _connectionFactory;
		public POSBillingService(IMapper mapper,
			ISqliteConnectionFactory connectionFactory,
			IProductService productService,
			IRestaurantTableStore tableStore,
			ITableStatusStore tableStatusStore)
		{
			_mapper = mapper;
			_connectionFactory = connectionFactory;
		}

		public async Task<List<GetTableViewDto>> GetTablesForViewAsync()
		{
			var result = new List<GetTableViewDto>();

			using var connection = _connectionFactory.CreateConnection();
			using var command = connection.CreateCommand();

			command.CommandText = @"
								SELECT
									t.Id                AS TableId,
									t.display_name      AS DisplayName,

									o.Id                AS OrderId,
									COALESCE(o.total_amount, 0) AS Amount,

									COALESCE(s.status_code, 'BLANK')       AS StatusCode,
									COALESCE(s.status_name, 'Blank Table') AS StatusName,
									COALESCE(s.color_hex, '#E0E0E0')        AS ColorHex

								FROM restaurant_tables t

								LEFT JOIN orders o
									ON o.Id = (
										SELECT o2.Id
										FROM orders o2
										WHERE
											o2.table_id = t.Id
											AND o2.closed_at IS NULL
											AND o2.order_type = 'DINE_IN'
										ORDER BY o2.created_at DESC
										LIMIT 1
									)

								LEFT JOIN table_status s
									ON s.Id = o.status_id

								WHERE t.is_active = 1
								ORDER BY t.display_name;
								";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(new GetTableViewDto
				{
					TableId = reader.GetInt32(0),
					DisplayName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),

					OrderId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
					Amount = reader.GetDecimal(3),

					StatusCode = reader.GetString(4),
					StatusName = reader.GetString(5),
					ColorHex = reader.GetString(6)
				});
			}

			return result;
		}
		
		public async Task<int> CreateOrderAsync(OrderDto order)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
                INSERT INTO orders
                (order_number, order_type, table_id, status_id,
                 total_amount, discount_type, discount_value, discount_reason,
                 created_at, IsSynced)
                VALUES
                (@OrderNumber, @OrderType, @TableId, @StatusId,
                 @TotalAmount, @DiscountType, @DiscountValue, @DiscountReason,
                 CURRENT_TIMESTAMP, 0);
                SELECT last_insert_rowid();
            ";

			cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
			cmd.Parameters.AddWithValue("@OrderType", order.OrderType);
			cmd.Parameters.AddWithValue("@TableId", (object?)order.TableId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@StatusId", order.StatusId);
			cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
			cmd.Parameters.AddWithValue("@DiscountType", (object?)order.DiscountType ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@DiscountValue", order.DiscountValue);
			cmd.Parameters.AddWithValue("@DiscountReason", (object?)order.DiscountReason ?? DBNull.Value);

			var id = (long)await cmd.ExecuteScalarAsync();
			return (int)id;
		}

		public async Task<OrderDto> GetOrderAsync(int orderId)
		{
			using var connection = _connectionFactory.CreateConnection();

			var order = await GetOrderInternalAsync(connection, orderId);
			order.OrderItems = await GetOrderItemsAsync(connection, orderId);
			order.Payments = await GetPaymentsAsync(connection, orderId);

			return order;
		}

		public async Task UpdateOrderAsync(OrderDto order)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var cmd = connection.CreateCommand();

			cmd.CommandText = @"
                UPDATE orders
                SET
                    total_amount = @TotalAmount,
					status_id = @StatusId,
                    discount_type = @DiscountType,
                    discount_value = @DiscountValue,
                    discount_reason = @DiscountReason,
                    IsSynced = 0
                WHERE Id = @Id;
            ";

			cmd.Parameters.AddWithValue("@Id", order.Id);
			cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
			cmd.Parameters.AddWithValue("@StatusId", order.StatusId);
			cmd.Parameters.AddWithValue("@DiscountType", (object?)order.DiscountType ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@DiscountValue", order.DiscountValue);
			cmd.Parameters.AddWithValue("@DiscountReason", (object?)order.DiscountReason ?? DBNull.Value);

			await cmd.ExecuteNonQueryAsync();
		}

		public async Task CreateOrderItemsAsync(List<OrderItemDto> items)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var tx = connection.BeginTransaction();

			foreach (var item in items)
			{
				using var cmd = connection.CreateCommand();
				cmd.Transaction = tx;

				cmd.CommandText = @"
                    INSERT INTO order_items
                    (order_id, variant_id, quantity, price_snapshot, discount_amount)
                    VALUES
                    (@OrderId, @VariantId, @Quantity, @Price, @Discount);
                ";

				cmd.Parameters.AddWithValue("@OrderId", item.OrderId);
				cmd.Parameters.AddWithValue("@VariantId", item.VariantId);
				cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
				cmd.Parameters.AddWithValue("@Price", item.PriceSnapshot);
				cmd.Parameters.AddWithValue("@Discount", item.DiscountAmount);

				cmd.ExecuteNonQuery();
			}

			tx.Commit();
		}
		public async Task UpdateOrderItemsAsync(int orderId, List<OrderItemDto> items)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var tx = connection.BeginTransaction();

			using (var deleteCmd = connection.CreateCommand())
			{
				deleteCmd.Transaction = tx;
				deleteCmd.CommandText = "DELETE FROM order_items WHERE order_id = @OrderId;";
				deleteCmd.Parameters.AddWithValue("@OrderId", orderId);
				deleteCmd.ExecuteNonQuery();
			}

			foreach (var item in items)
			{
				using var insertCmd = connection.CreateCommand();
				insertCmd.Transaction = tx;

				insertCmd.CommandText = @"
                    INSERT INTO order_items
                    (order_id, variant_id, quantity, price_snapshot, discount_amount)
                    VALUES
                    (@OrderId, @VariantId, @Quantity, @Price, @Discount);
                ";

				insertCmd.Parameters.AddWithValue("@OrderId", orderId);
				insertCmd.Parameters.AddWithValue("@VariantId", item.VariantId);
				insertCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
				insertCmd.Parameters.AddWithValue("@Price", item.PriceSnapshot);
				insertCmd.Parameters.AddWithValue("@Discount", item.DiscountAmount);

				insertCmd.ExecuteNonQuery();
			}

			tx.Commit();
		}

		public async Task CloseOrderAsync(int orderId, PaymentDto payment)
		{
			using var connection = _connectionFactory.CreateConnection();
			using var tx = connection.BeginTransaction();

			try
			{
				// ---------------- PAYMENTS ----------------
				if (payment.Mode != PaymentMode.PART)
				{
					InsertPayment(connection, tx, orderId, payment.Mode.ToString(), payment.Amount);
				}
				else
				{
					InsertPayment(connection, tx, orderId, "CASH", payment.PartPaymentCash);
					InsertPayment(connection, tx, orderId, "UPI",
						payment.Amount - payment.PartPaymentCash);
				}

				// ---------------- CLOSE ORDER ----------------
				using var cmd = connection.CreateCommand();
				cmd.Transaction = tx;

				cmd.CommandText = @"
                    UPDATE orders
                    SET
                        closed_at = CURRENT_TIMESTAMP,
                        status_id = (
                            SELECT Id FROM table_status
                            WHERE status_code = @Blank
                        ),
                        IsSynced = 0
                    WHERE Id = @OrderId;
                ";

				cmd.Parameters.AddWithValue("@OrderId", orderId);
				cmd.Parameters.AddWithValue("@Blank", TableStatusCodes.BLANK);

				cmd.ExecuteNonQuery();

				tx.Commit();
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}


		#region private methods
		private async Task<OrderDto> GetOrderInternalAsync(SqliteConnection conn, int orderId)
		{
			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM orders WHERE Id = @Id;";
			cmd.Parameters.AddWithValue("@Id", orderId);

			using var reader = await cmd.ExecuteReaderAsync();
			await reader.ReadAsync();

			return new OrderDto
			{
				Id = reader.GetInt32(reader.GetOrdinal("Id")),
				OrderNumber = reader.GetString(reader.GetOrdinal("order_number")),
				OrderType = reader.GetString(reader.GetOrdinal("order_type")),
				TableId = reader.IsDBNull("table_id") ? null : reader.GetInt32("table_id"),
				StatusId = reader.GetInt32("status_id"),
				TotalAmount = reader.GetDecimal("total_amount"),
				DiscountType = reader.IsDBNull("discount_type") ? null : reader.GetString("discount_type"),
				DiscountValue = reader.GetDecimal("discount_value"),
				DiscountReason = reader.IsDBNull("discount_reason") ? null : reader.GetString("discount_reason"),
				CreatedAt = DateTime.Parse(reader.GetString("created_at")),
				ClosedAt = reader.IsDBNull("closed_at")
					? null
					: DateTime.Parse(reader.GetString("closed_at"))
			};
		}

		private async Task<List<OrderItemDto>> GetOrderItemsAsync(SqliteConnection conn, int orderId)
		{
			var result = new List<OrderItemDto>();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
			SELECT 
			-- Order Item
			oi.Id               AS OrderItemId,
			oi.order_id         AS OrderId,
			oi.variant_id       AS VariantId,
			oi.quantity         AS Quantity,
			oi.price_snapshot   AS PriceSnapshot,
			oi.discount_amount  AS DiscountAmount,

			-- Variant
			v.Id                AS Variant_Id,
			v.product_id        AS Variant_ProductId,
			v.brand_id          AS Variant_BrandId,
			v.variant_name      AS Variant_Name,
			v.price             AS Variant_Price,
			v.is_active         AS Variant_IsActive,
			v.display_order     AS Variant_DisplayOrder,

			-- Product
			p.Id                AS Product_Id,
			p.name              AS Product_Name,
			p.food_type         AS Product_FoodType,
			p.is_active         AS Product_IsActive

			FROM order_items oi
			INNER JOIN product_variants v 
				ON oi.variant_id = v.Id
			INNER JOIN products p
				ON v.product_id = p.Id
			WHERE oi.order_id = @Id";

			cmd.Parameters.AddWithValue("@Id", orderId);

			using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				result.Add(new OrderItemDto
				{
					Id = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
					OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
					VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
					Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
					PriceSnapshot = reader.GetDecimal(reader.GetOrdinal("PriceSnapshot")),
					DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),

					Variant = new ProductVariantDto
					{
						Id = reader.GetInt32(reader.GetOrdinal("Variant_Id")),
						ProductId = reader.GetInt32(reader.GetOrdinal("Variant_ProductId")),
						BrandId = reader.IsDBNull(reader.GetOrdinal("Variant_BrandId"))
									? null
									: reader.GetInt32(reader.GetOrdinal("Variant_BrandId")),
						VariantName = reader.GetString(reader.GetOrdinal("Variant_Name")),
						Price = reader.GetDecimal(reader.GetOrdinal("Variant_Price")),
						IsActive = reader.GetBoolean(reader.GetOrdinal("Variant_IsActive")),
						DisplayOrder = reader.GetInt32(reader.GetOrdinal("Variant_DisplayOrder")),

						Product = new ProductDto
						{
							Id = reader.GetInt32(reader.GetOrdinal("Product_Id")),
							Name = reader.GetString(reader.GetOrdinal("Product_Name")),
							FoodType = reader.IsDBNull(reader.GetOrdinal("Product_FoodType"))
									? string.Empty
									: reader.GetString(reader.GetOrdinal("Product_FoodType")),
							IsActive = reader.GetBoolean(reader.GetOrdinal("Product_IsActive"))
						}
					}
				});
			}

			return result;
		}



		private async Task<List<PaymentDto>> GetPaymentsAsync(SqliteConnection conn, int orderId)
		{
			var result = new List<PaymentDto>();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM payments WHERE order_id = @Id;";
			cmd.Parameters.AddWithValue("@Id", orderId);

			using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				result.Add(new PaymentDto
				{
					Id = reader.GetInt32("Id"),
					OrderId = reader.GetInt32("order_id"),
					Mode = reader.GetString("mode"),
					Amount = reader.GetDecimal("amount"),
					CreatedAt = DateTime.Parse(reader.GetString("created_at"))
				});
			}

			return result;
		}

		private void InsertPayment(
			SqliteConnection conn,
			SqliteTransaction tx,
			int orderId,
			string mode,
			decimal amount)
		{
			using var cmd = conn.CreateCommand();
			cmd.Transaction = tx;

			cmd.CommandText = @"
                INSERT INTO payments
                (order_id, mode, amount, status, IsSynced)
                VALUES
                (@OrderId, @Mode, @Amount, 'PAID', 0);
            ";

			cmd.Parameters.AddWithValue("@OrderId", orderId);
			cmd.Parameters.AddWithValue("@Mode", mode);
			cmd.Parameters.AddWithValue("@Amount", amount);

			cmd.ExecuteNonQuery();
		}
		#endregion
	}
}
