namespace uSignIn.CommonSettings.DTOs
{
	[Obsolete("User History instead.")]
	public static class Extensions
	{
		public static DateOnly? GetLatest(this DateOnlyHistory[] dateOnlyHistories)
		=> dateOnlyHistories?.OrderByDescending(f => f.TimeStamp)?.FirstOrDefault()?.Value;

		public static string? GetLatest(this StringHistory[] stringWithHistory)
		=> stringWithHistory?.OrderByDescending(f => f.TimeStamp)?.FirstOrDefault()?.Value;


	}
}
