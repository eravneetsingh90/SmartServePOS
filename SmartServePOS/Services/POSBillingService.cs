using AutoMapper;
using SmartServe.Domain.Services;
using SmartServe.Domain.Stores;
using SmartServePOS.Models;

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

	}
}
