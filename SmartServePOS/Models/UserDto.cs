namespace SmartServePOS.Models
{
	public class UserDto
	{
		public int Id { get; set; }              // ServerId
		public string Name { get; set; }
		public int RoleId { get; set; }          // Server RoleId
		public string PinHash { get; set; }
		public bool IsActive { get; set; }
		public DateTime UpdatedOn { get; set; }
	}

}
