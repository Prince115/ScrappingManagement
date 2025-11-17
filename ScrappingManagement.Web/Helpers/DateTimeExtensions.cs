using System;

namespace ScrappingManagement.Web.Helpers
{
	public static class DateTimeExtensions
	{
		private static TimeZoneInfo GetIndiaTimeZone()
		{
			// Try Windows id first, then IANA id for Linux/macOS
			var windowsId = "India Standard Time";
			var ianaId = "Asia/Kolkata";

			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
			}
			catch (TimeZoneNotFoundException)
			{
				// fall through to try IANA
			}
			catch (InvalidTimeZoneException)
			{
				// fall through to try IANA
			}

			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
			}
			catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
			{
				throw new InvalidOperationException("India time zone not found on this system. Tried 'India Standard Time' and 'Asia/Kolkata'.", ex);
			}
		}

		/// <summary>
		/// Convert a DateTime to India Standard Time (IST).
		/// If the incoming DateTime.Kind is Unspecified, <paramref name="assumeUtc"/> controls whether it is treated as UTC (true) or Local (false).
		/// The returned DateTime has Kind = Unspecified (represents wall-clock in IST).
		/// </summary>
		public static DateTime ToIndianTime(this DateTime dateTime, bool assumeUtc = true)
		{
			var tz = GetIndiaTimeZone();

			DateTime utc;

			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					utc = dateTime;
					break;
				case DateTimeKind.Local:
					utc = dateTime.ToUniversalTime();
					break;
				default: // Unspecified
					if (assumeUtc)
					{
						utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
					}
					else
					{
						utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
					}
					break;
			}

			var india = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
			// Return as Unspecified to represent the IST wall-clock without another Kind that could cause re-conversion
			return DateTime.SpecifyKind(india, DateTimeKind.Unspecified);
		}

		/// <summary>
		/// Convert a DateTimeOffset to India Standard Time (IST) and return a DateTimeOffset with the IST offset.
		/// </summary>
		public static DateTimeOffset ToIndianTime(this DateTimeOffset dto)
		{
			var tz = GetIndiaTimeZone();
			return TimeZoneInfo.ConvertTime(dto, tz);
		}

		/// <summary>
		/// Convenience: convert DateTime to DateTimeOffset in IST.
		/// If the incoming DateTime.Kind is Unspecified, <paramref name="assumeUtc"/> controls interpretation.
		/// </summary>
		public static DateTimeOffset ToIndianTimeOffset(this DateTime dateTime, bool assumeUtc = true)
		{
			var tz = GetIndiaTimeZone();
			DateTimeOffset inputDto = dateTime.Kind switch
			{
				DateTimeKind.Utc => new DateTimeOffset(dateTime, TimeSpan.Zero),
				DateTimeKind.Local => new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero),
				_ => assumeUtc ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), TimeSpan.Zero)
							   : new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime(), TimeSpan.Zero)
			};

			return TimeZoneInfo.ConvertTime(inputDto, tz);
		}
	}

	// Example usage:
	// var nowUtc = DateTime.UtcNow;
	// var ist = nowUtc.ToIndianTime(); // returns wall-clock IST as DateTime (Kind = Unspecified)
	// var dto = DateTimeOffset.UtcNow.ToIndianTime(); // returns DateTimeOffset with IST offset (+05:30)
}