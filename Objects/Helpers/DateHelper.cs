using NodaTime;
using NodaTime.Extensions;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class DateHelper
    {
        // The withins are here for completeness in case they are ever implemented, Within... options not currently used.
        // They are here, and the options are structired this way, so we can use a threshold to compare the goodness of the match
        public enum MatchType { NotChecked, NoMatch, WithinYear, SameYear, WithinMonth, SameMonth, WithinWeek, SameWeek, WithinDay, SameDay, WithinHour, SameHour, WithinMinute, SameMinute, WithinSecond, SameSecond, Same }

        /// <summary>
        /// Calculates number of business days, taking into account:
        ///  - weekends (Saturdays and Sundays)
        ///  - bank holidays in the middle of the week
        /// </summary>
        /// <param name="firstDay">First day in the time interval</param>
        /// <param name="lastDay">Last day in the time interval</param>
        /// <param name="bankHolidays">List of bank holidays excluding weekends</param>
        /// <returns>Number of business days during the 'span'</returns>
        public static int BusinessDaysUntil(this DateTime firstDay, DateTime lastDay, params DateTime[] bankHolidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (firstDay <= bh && bh <= lastDay)
                    --businessDays;
            }

            return businessDays;
        }

        public static DateTime? GetNullableDateTime(this DateTime dateTime)
        {
            return dateTime == DateTime.MinValue ? (DateTime?)null : dateTime;
        }
        public static DateTime GetDateTime(this DateTime? dateTime)
        {
            return dateTime ?? DateTime.MinValue;
        }
        public static string GetDateString(this DateTime? dateTime)
        {
            return dateTime != null ? dateTime.Value.ToShortDateString() : "";
        }
        public static DateTimeOffset GetMonthYearOffSet(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0, TimeSpan.Zero);
        }
        public static DateTime GetMonthYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0);
        }
        public static DateTime GetMonthYear(this DateTimeOffset dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0);
        }

        public static bool IsMonthYearEqual(this DateTimeOffset dateTimeOffset1, DateTimeOffset dateTimeOffset2)
        {
            return dateTimeOffset1.Month == dateTimeOffset2.Month && dateTimeOffset1.Year == dateTimeOffset2.Year;
        }
        public static bool IsMonthYearEqual(this DateTime dateTimeOffset1, DateTime dateTimeOffset2)
        {
            return dateTimeOffset1.Month == dateTimeOffset2.Month && dateTimeOffset1.Year == dateTimeOffset2.Year;
        }
        public static DateTimeOffset GetMonthYearOffSet(this DateTimeOffset dateTime)
        {
            return new DateTimeOffset(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0, TimeSpan.Zero);
        }
        public static DateTimeOffset GetMonthYearOffSet(this DateTime? dateTime)
        {
            if (dateTime != null)
                return new DateTimeOffset(dateTime.Value.Year, dateTime.Value.Month, 1, 0, 0, 0, 0, TimeSpan.Zero);
            return DateTimeOffset.MinValue;
        }
        public static string ToTimeString(this int? value)
        {
            if (value == null || value.Value == 0)
            {
                return "-";
            }
            return TimeSpan.FromSeconds(value.Value).ToString("g");
        }

        public static int BusinessDaysSince(this DateTime lastDay, DateTime firstDay, params DateTime[] bankHolidays)
        {
            return BusinessDaysUntil(firstDay, lastDay, bankHolidays);
        }

        public static MatchType GetMatch(DateTime thisDateTime, DateTime otherDateTime)
        {
            return thisDateTime.Match(otherDateTime);
        }

        public static MatchType Match(this DateTime thisDateTime, DateTime otherDateTime)
        {
            // We do no yet implemented the "Within..." items
            if (thisDateTime.Year == otherDateTime.Year)
            {
                if (thisDateTime.Month == otherDateTime.Month)
                {
                    if (thisDateTime.Day == otherDateTime.Day)
                    {
                        if (thisDateTime.Hour == otherDateTime.Hour)
                        {
                            if (thisDateTime.Minute == otherDateTime.Minute)
                            {
                                if (thisDateTime.Second == otherDateTime.Second)
                                {
                                    if (thisDateTime.Millisecond == otherDateTime.Millisecond)
                                    {
                                        return MatchType.Same;
                                    }
                                    return MatchType.SameSecond;
                                }
                                return MatchType.SameMinute;
                            }
                            return MatchType.SameHour;
                        }
                        return MatchType.SameDay;
                    }
                    return MatchType.SameMonth;
                }
                return MatchType.SameYear;
            }
            return MatchType.NoMatch;
        }

        public static string ToShortDate(this DateTime? dateTime, string emptyString = "-")
        {
            if (dateTime == null)
            {
                return emptyString;
            }
            return dateTime.Value.ToShortDateString();
        }
        public static string ToShortDate(this DateTimeOffset? dateTime, string emptyString = "-")
        {
            if (dateTime == null)
            {
                return emptyString;
            }
            return dateTime.Value.ToString();
        }
        public static string ToShortDate(this DateTime dateTime, string emptyString = "-")
        {
            if (dateTime == DateTime.MinValue)
            {
                return emptyString;
            }
            return dateTime.ToShortDateString();
        }
        public static string ToShortDate(this DateTimeOffset dateTime, string emptyString = "-")
        {
            if (dateTime == DateTimeOffset.MinValue)
            {
                return emptyString;
            }
            return dateTime.ToString();
        }
        public static string ToHumanDateTime(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.ToString("MMM dd, yyyy hh:mm tt");
        }
        public static string ToHumanDateTime(this DateTimeOffset dateTime, bool convertToLocal = true)
        {
            if (dateTime == DateTimeOffset.MinValue)
            {
                return String.Empty;
            }
            return dateTime.LocalDateTime.ToString("MMM dd, yyyy hh:mm tt");
        }
        public static string ToHumanDateTime(this DateTime? dateTime)
        {
            if (dateTime == null || dateTime.Value == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.Value.ToString("MMM dd, yyyy hh:mm tt");
        }
        public static string ToHumanDateTime(this DateTimeOffset? dateTime, bool convertToLocal = true)
        {
            if (dateTime == null || dateTime.Value == DateTimeOffset.MinValue)
            {
                return String.Empty;
            }
            return dateTime.Value.LocalDateTime.ToString("MMM dd, yyyy hh:mm tt");
        }
        public static string ToHumanDate(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.ToString("MMM dd, yyyy");
        }
        public static string ToHumanDate(this DateTimeOffset dateTime)
        {
            if (dateTime == DateTimeOffset.MinValue)
            {
                return String.Empty;
            }
            return dateTime.ToString("MMM dd, yyyy");
        }
        public static string ToHumanDate(this DateTime? dateTime)
        {
            if (dateTime == null || dateTime == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.Value.ToString("MMM dd, yyyy");
        }
        public static string ToHumanDateShort(this DateTime? dateTime)
        {
            if (dateTime == null || dateTime == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.Value.ToString("MMM dd, yy");
        }
        public static string ToHumanDate(this DateTimeOffset? dateTime)
        {
            if (dateTime == null || dateTime == DateTimeOffset.MinValue)
            {
                return String.Empty;
            }
            return dateTime.Value.ToString("MMM dd, yyyy");
        }
        public static string ToHumanTime(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return String.Empty;
            }
            return dateTime.ToString("hh:mm tt");
        }
        public static string ToHumanTime(this DateTimeOffset dateTime)
        {
            if (dateTime == DateTimeOffset.MinValue)
            {
                return String.Empty;
            }
            return dateTime.ToString("hh:mm tt");
        }
        public static string ToTimeSinceString(this DateTime? value, bool returnTime = true)
        {
            if (value != null)
            {
                return ToTimeSinceString(value.Value, returnTime);
            }
            return String.Empty;
        }
        public static string ToTimeSinceString(this DateTimeOffset? value, bool returnTime = true)
        {
            if (value != null)
            {
                return ToTimeSinceString(value.Value.Date, returnTime);
            }
            return String.Empty;
        }
        public static string ToTimeSinceString(this DateTime value, bool returnTime = true)
        {
            if ((DateTime.Now.Date - value.Date).TotalDays == 0)
            {
                if (returnTime)
                {
                    return $"Today {value.ToString("hh:mm tt")}";
                }
                else
                {
                    return $"Today";
                }
            }
            if ((DateTime.Now.Date - value.Date).TotalDays == 1)
            {
                if (returnTime)
                {
                    return $"Yesterday {value.ToString("hh:mm tt")}";
                }
                else
                {
                    return $"Yesterday";
                }
            }
            if (returnTime)
            {
                return value.ToHumanDateTime();
            }
            else
            {
                return value.ToHumanDate();
            }
        }
        public static DateTime? MonthEndDate(this DateTime? dateTime)
        {
            if (dateTime == null || dateTime == DateTime.MinValue)
            {
                return null;
            }
            return new DateTime(dateTime.Value.Year, dateTime.Value.Month, DateTime.DaysInMonth(dateTime.Value.Year, dateTime.Value.Month));
        }
        public static long ToUnixEpochDate(this DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);
        public static long ToUnixEpochDate13Digit(this DateTime date)
            => (long)Math.Round((date.ToUniversalTime() -
                                 new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalMilliseconds);
        public static string DateTimeToHex(this DateTime dateTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime - unixEpoch;
            long unixTimestampSeconds = (long)timeSpan.TotalSeconds;
            return unixTimestampSeconds.ToString("X");
        }
        public static string DateTimeToHex(this DateTimeOffset dateTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime - unixEpoch;
            long unixTimestampSeconds = (long)timeSpan.TotalSeconds;
            return unixTimestampSeconds.ToString("X");
        }

        public static DateTime HexToDateTime(this string hexDate)
        {
            long unixTimestampSeconds = long.Parse(hexDate, System.Globalization.NumberStyles.HexNumber);
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(unixTimestampSeconds);

        }

        public static DateTimeOffset NodaConvertToLocalTime(this DateTime dateTime)
        {
            var systemDefaultZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var desiredTime = dateTime.ToLocalDateTime();
            var desiredZonedDateTime = desiredTime.InZoneStrictly(systemDefaultZone);
            return desiredZonedDateTime.ToDateTimeOffset();
        }

        public static DateRange GetDateRange(this DateTime monthYear,int days)
        {
            var dateRange=new DateRange();
            var endDate = new DateTime(monthYear.Year, monthYear.Month, monthYear.Day);
            var startDate = endDate.AddDays(days * -1);
            dateRange.StartDate = startDate.NodaConvertToLocalTime();
            dateRange.EndDate = endDate.NodaConvertToLocalTime();
            return dateRange;
        }
        public static DateRange GetMonthRange(this DateTime monthYear)
        {
            var dateRange=new DateRange();
            var startDate = new DateTime(monthYear.Year, monthYear.Month, 1);
            var endDate = startDate.AddMonths(1);
            dateRange.StartDate = startDate.NodaConvertToLocalTime();
            dateRange.EndDate = endDate.NodaConvertToLocalTime();
            return dateRange;
        }
        public static DateRange GetMonthRange(this DateTimeOffset monthYear)
        {
            var dateRange=new DateRange();
            var startDate = new DateTime(monthYear.Year, monthYear.Month, 1);
            var endDate = startDate.AddMonths(1);
            dateRange.StartDate = startDate.NodaConvertToLocalTime();
            dateRange.EndDate = endDate.NodaConvertToLocalTime();
            return dateRange;
        }
    }

    public class DateRange
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}

