using System;
using System.Globalization;
using System.Net;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FromUnixTime(this string unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epoch = epoch.AddSeconds(Convert.ToDouble(unixTime));
            return epoch.ToLocalTime();
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            epoch = epoch.AddSeconds(Convert.ToDouble(unixTime));
            return epoch.ToLocalTime();
        }

        public static string ToUnixTime(this DateTime unixTime)
        {
            DateTime zuluTime = unixTime.ToUniversalTime();
            DateTime unixEpoch = new DateTime(1970, 1, 1);

            var unixTimeStamp = (zuluTime.Subtract(unixEpoch)).TotalSeconds.ToString(CultureInfo.InvariantCulture);
            return unixTimeStamp;
        }

        public static double ToUnixTimeDouble(this DateTime unixTime)
        {
            DateTime zuluTime = unixTime.ToUniversalTime();
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            return (zuluTime.Subtract(unixEpoch)).TotalSeconds;
        }

        public static long GetCurrentUnixTime()
        {
            var currentTime = DateTime.Now;
            var zuluTime = currentTime.ToUniversalTime();
            var unixEpoch = new DateTime(1970, 1, 1);

            long unixTimeStamp = (int) zuluTime.Subtract(unixEpoch).TotalSeconds;
            return unixTimeStamp;
        }

        public static DateTime AddTimeSpan(this DateTime sourceDateTime, int day, int hour, int min, int sec)
        {
            var targetTime = sourceDateTime.Add(new TimeSpan(day, hour, min, sec));

            return targetTime;
        }

        public static string AddTimeSpanToUnixTime(this string sourceDateTime, int day, int hour, int min, int sec)
        {
            var scrTime = sourceDateTime.FromUnixTime();
            var targetTime = scrTime.Add(new TimeSpan(day, hour, min, sec));

            return targetTime.ToUnixTime();
        }

        /// <summary>
        /// Calculate the difference of two string unix time, returned as double
        /// </summary>
        public static double DifferenceBetweenTwoUnixTime(string laterUnixTime, string formerUnixTime)
        {
            var later = Convert.ToDouble(laterUnixTime);
            var former = Convert.ToDouble(formerUnixTime);

            return (later - former);
        }

        /// <summary>
        /// Calculate the difference of two string unix time, returned as timespan
        /// </summary>
        public static TimeSpan DifferenceBetweenTwoStringTime(string laterTime, string formerTime)
        {
            if (String.IsNullOrEmpty(formerTime))
            {
                return TimeSpan.Zero;
            }

            DateTime start = DateTime.Parse(formerTime);
            DateTime dt = DateTime.Parse(laterTime);
            TimeSpan t = dt - start;
            return t;
        }

        /// <summary>
        /// Calculate the difference of two DateTime, returned as timespan
        /// </summary>
        public static TimeSpan DifferenceBetweenTwoDateTime(DateTime laterTime, DateTime formerTime)
        {
            var t = laterTime - formerTime;
            return t;
        }

        /// <summary>
        /// To get internet time as set with google.com
        /// </summary>
        /// <returns></returns>
        public static DateTime GetNetTime()
        {
            try
            {
                using (var response =
                    WebRequest.Create("http://www.google.com").GetResponse())
                    return DateTime.ParseExact(response.Headers["date"],
                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                        CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeUniversal);
            }
            catch (WebException)
            {
                return DateTime.Now; //In case something goes wrong. 
            }

            // try
            // {
            //     var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
            //     var response = myHttpWebRequest.GetResponse();
            //     string todaysDates = response.Headers["date"];
            //     return DateTime.ParseExact(todaysDates,
            //         "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
            //         CultureInfo.InvariantCulture.DateTimeFormat,
            //         DateTimeStyles.AssumeUniversal);
            // }
            // catch (Exception)
            // {
            //     return DateTime.Now;
            // }
        }
    }
}