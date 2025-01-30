namespace uSignIn.CommonSettings.DTOs
{
	public static class HistoryExtensions
	{
		public static T? LatestValue<T>(this IEnumerable<History<T>> histories)
		{
			var latest = histories?
			.OrderByDescending(h => h.TimeStamp)
			.FirstOrDefault();

			if (latest == null)
			{
				return default;
			}

			return latest.Value; // Guaranteed to be non-null
		}

	}

}
