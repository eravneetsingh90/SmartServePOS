namespace SmartServePOS.Models
{
	public class StockItemLookupModel
	{
		public int StockItemId { get; set; }

		public string ItemType { get; set; }   // VARIANT / INGREDIENT
		public int ReferenceId { get; set; }

		public string DisplayName { get; set; } // "Vanilla Brick", "Ice Cream Base"
		public string Unit { get; set; }
	}
}
