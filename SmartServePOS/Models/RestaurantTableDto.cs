namespace SmartServePOS.Models
{
	public class RestaurantTableDto
	{
		public int Id { get; set; }              // ServerId
		public string DisplayName { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
