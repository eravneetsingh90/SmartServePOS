namespace SmartServePOS.Models
{
	public enum PaymentModeType
	{
		Cash,
		UPI,
		Part
	}
	public enum StockItemType
	{
		VARIANT,
		INGREDIENT
	}
	public enum DateRangeType
	{
		Today,
		Yesterday,
		Last7Days,
		ThisMonth,
		LastMonth,
		Custom
	}
}
