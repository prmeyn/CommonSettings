namespace uSignIn.CommonSettings.DTOs
{
	public sealed class History<T>
	{
		public required T Value { get; set; }
		public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow; // Automatically set current time
	}
}
