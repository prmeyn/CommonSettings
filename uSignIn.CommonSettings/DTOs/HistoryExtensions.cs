
namespace uSignIn.CommonSettings.DTOs
{
	public static class HistoryExtensions
	{
		public static T? LatestValue<T>(this IEnumerable<History<T>> histories)
		{
			var latest = histories.LatestRecord();

			if (latest == null)
			{
				return default;
			}

			return latest.Value; // Guaranteed to be non-null
		}

		public static History<T>? LatestRecord<T>(this IEnumerable<History<T>> histories) => histories?.OrderByDescending(h => h.TimeStamp).FirstOrDefault();
	}

}
