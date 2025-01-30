namespace uSignIn.CommonSettings.DTOs
{
	[Obsolete("User History<DateOnly> instead.")]
	public sealed class DateOnlyHistory
	{
		public DateOnly Value { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
	}
}