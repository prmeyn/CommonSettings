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
				throw new InvalidOperationException("No history records available.");
			}

			return latest.Value; // Guaranteed to be non-null
		}

	}

}
