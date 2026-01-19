namespace SmartServePOS.Models
{
    public class TableStatusDto
    {
		public int Id { get; set; }
		public int LocalId { get; set; }

		public string StatusCode { get; set; } = null!;

		public string? StatusName { get; set; }

		public string? ColorHex { get; set; }
	}
}
