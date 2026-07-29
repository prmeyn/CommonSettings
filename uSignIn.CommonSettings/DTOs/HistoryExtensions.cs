namespace uSignIn.CommonSettings.DTOs
{
	/// <summary>
	/// Helpers for reading the most recent entry out of a sequence of <see cref="History{T}"/> records.
	/// </summary>
	public static class HistoryExtensions
	{
		/// <summary>
		/// Returns the value of the most recent record, or <c>default</c> when
		/// <paramref name="histories"/> is <see langword="null"/> or empty.
		/// </summary>
		/// <typeparam name="T">Type of the recorded value.</typeparam>
		/// <param name="histories">Records to search. May be <see langword="null"/>.</param>
		public static T? LatestValue<T>(this IEnumerable<History<T>>? histories)
		{
			var latest = histories.LatestRecord();

			if (latest == null)
			{
				return default;
			}

			return latest.Value;
		}

		/// <summary>
		/// Returns the record with the highest <see cref="History{T}.TimeStamp"/>, or
		/// <see langword="null"/> when <paramref name="histories"/> is <see langword="null"/> or
		/// empty. When several records share the highest timestamp, the first of them in sequence
		/// order is returned.
		/// </summary>
		/// <typeparam name="T">Type of the recorded value.</typeparam>
		/// <param name="histories">Records to search. May be <see langword="null"/>.</param>
		public static History<T>? LatestRecord<T>(this IEnumerable<History<T>>? histories) =>
			histories?.MaxBy(h => h.TimeStamp);
	}
}
