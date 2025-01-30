namespace uSignIn.CommonSettings.DTOs
{
	[Obsolete("User History<string> instead.")]
	public sealed class StringHistory
	{
		public string Value { get; set; }
		public DateTimeOffset TimeStamp { get; set; }
	}
}