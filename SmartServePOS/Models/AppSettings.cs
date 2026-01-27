namespace SmartServePOS.Models
{
	public class POSSettings
	{
		public string ShopName { get; set; } = string.Empty;
		public string ShopAddress { get; set; } = string.Empty;
		public string TerminalId { get; set; } = string.Empty;
		public bool PrintPreview { get; set; } = false;
	}
	public class ThemeSettings
	{
		public string PrimaryColor { get; set; } = "#000000";
		public string SecondaryColor { get; set; } = "#000000";
		public string AccentColor { get; set; } = "#000000";
	}

}
