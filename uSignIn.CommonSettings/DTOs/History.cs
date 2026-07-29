namespace uSignIn.CommonSettings.DTOs
{
	/// <summary>
	/// A single point-in-time record of a value.
	/// </summary>
	/// <typeparam name="T">Type of the recorded value.</typeparam>
	public sealed class History<T>
	{
		/// <summary>The recorded value.</summary>
		public required T Value { get; set; }

		/// <summary>
		/// When the value was recorded. Defaults to the current UTC time.
		/// </summary>
		public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
	}
}
