namespace SmartServePOS.Models
{
	public class CategoryDto
	{
		public int Id { get; set; }              // ServerId
		public string Name { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
