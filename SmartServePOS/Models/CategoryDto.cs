namespace SmartServePOS.Models
{
	public class CategoryDto
	{
		public int LocalId { get; set; }
		public int Id { get; set; } //serverId
		public string Name { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
