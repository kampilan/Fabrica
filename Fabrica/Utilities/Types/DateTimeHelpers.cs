using System;
using System.Collections.Generic;

namespace Fabrica.Utilities.Types
{


    public static class DateTimeHelpers
    {

        static DateTimeHelpers()
        {

            PastModels = new List<IDateTimeRange>
            {
                new DateTimeRangeModel {Label = "Last 1 Minute",   RangeKind = DateTimeRange.Prev1Min},
                new DateTimeRangeModel {Label = "Last 2 Minutes",  RangeKind = DateTimeRange.Prev2Min},
                new DateTimeRangeModel {Label = "Last 5 Minutes",  RangeKind = DateTimeRange.Prev5Min},
                new DateTimeRangeModel {Label = "Last 15 Minutes", RangeKind = DateTimeRange.Prev15Min},
                new DateTimeRangeModel {Label = "Last 30 Minutes", RangeKind = DateTimeRange.Prev30Min},
                new DateTimeRangeModel {Label = "Last 1 Hour",     RangeKind = DateTimeRange.Prev1Hour},
                new DateTimeRangeModel {Label = "Last 2 Hours",    RangeKind = DateTimeRange.Prev2Hour},
                new DateTimeRangeModel {Label = "Last 4 Hours",    RangeKind = DateTimeRange.Prev4Hour},
                new DateTimeRangeModel {Label = "Last 8 Hours",    RangeKind = DateTimeRange.Prev8Hour},
                new DateTimeRangeModel {Label = "Last 12 Hours",   RangeKind = DateTimeRange.Prev12Hour},
                new DateTimeRangeModel {Label = "Last 24 Hours",   RangeKind = DateTimeRange.Prev24Hour},
                new DateTimeRangeModel {Label = "Today",           RangeKind = DateTimeRange.Today},
                new DateTimeRangeModel {Label = "Yesterday",       RangeKind = DateTimeRange.Yesterday},
                new DateTimeRangeModel {Label = "This Week",       RangeKind = DateTimeRange.ThisWeek},
                new DateTimeRangeModel {Label = "Last Week",       RangeKind = DateTimeRange.LastWeek},
                new DateTimeRangeModel {Label = "This Month",      RangeKind = DateTimeRange.ThisMonth},
                new DateTimeRangeModel {Label = "Last Month",      RangeKind = DateTimeRange.LastMonth},
                new DateTimeRangeModel {Label = "This Year",       RangeKind = DateTimeRange.ThisYear},
                new DateTimeRangeModel {Label = "Last Year",       RangeKind = DateTimeRange.LastYear}
            };


            FutureModels = new List<IDateTimeRange>
            {
                new DateTimeRangeModel {Label = "Next 1 Minute",   RangeKind  = DateTimeRange.Next1Min},
                new DateTimeRangeModel {Label = "Next 2 Minutes",  RangeKind = DateTimeRange.Next2Min},
                new DateTimeRangeModel {Label = "Next 5 Minutes",  RangeKind = DateTimeRange.Next5Min},
                new DateTimeRangeModel {Label = "Next 15 Minutes", RangeKind = DateTimeRange.Next15Min},
                new DateTimeRangeModel {Label = "Next 30 Minutes", RangeKind = DateTimeRange.Next30Min},
                new DateTimeRangeModel {Label = "Next 1 Hour",     RangeKind = DateTimeRange.Next1Hour},
                new DateTimeRangeModel {Label = "Next 2 Hours",    RangeKind = DateTimeRange.Next2Hour},
                new DateTimeRangeModel {Label = "Next 4 Hours",    RangeKind = DateTimeRange.Next4Hour},
                new DateTimeRangeModel {Label = "Next 8 Hours",    RangeKind = DateTimeRange.Next8Hour},
                new DateTimeRangeModel {Label = "Next 12 Hours",   RangeKind = DateTimeRange.Next12Hour},
                new DateTimeRangeModel {Label = "Next 24 Hours",   RangeKind = DateTimeRange.Next24Hour},
                new DateTimeRangeModel {Label = "Today",           RangeKind = DateTimeRange.Today},
                new DateTimeRangeModel {Label = "Tomorrow",        RangeKind = DateTimeRange.Tommorrow},
                new DateTimeRangeModel {Label = "This Week",       RangeKind = DateTimeRange.ThisWeek},
                new DateTimeRangeModel {Label = "Next Week",       RangeKind = DateTimeRange.NextWeek},
                new DateTimeRangeModel {Label = "This Month",      RangeKind = DateTimeRange.ThisMonth},
                new DateTimeRangeModel {Label = "Next Month",      RangeKind = DateTimeRange.NextMonth},
                new DateTimeRangeModel {Label = "This Year",       RangeKind = DateTimeRange.ThisYear},
                new DateTimeRangeModel {Label = "Next Year",       RangeKind = DateTimeRange.NextYear}
            };


        }


        public static IReadOnlyCollection<IDateTimeRange> PastModels { get; }
        public static IReadOnlyCollection<IDateTimeRange> FutureModels { get; }



        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
        {

            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;

        }


        public static DateTime StartOfMonth( this DateTime dt )
        {


            var year  = dt.Year;
            var month = dt.Month;
            
            var start = new DateTime( year, month, 1, 0, 0, 0 );

            return start;

        }


        public static (DateTime begin, DateTime end) CalculateRange( DateTimeRange range, DateTime origin = default(DateTime) )
        {

            origin = origin == default(DateTime) ? DateTime.Now : origin;

            var begin = origin;
            var end = origin;

            switch (range)
            {
                case DateTimeRange.Prev1Min:
                    begin = origin - TimeSpan.FromMinutes(1);
                    break;
                case DateTimeRange.Prev2Min:
                    begin = origin - TimeSpan.FromMinutes(2);
                    break;
                case DateTimeRange.Prev5Min:
                    begin = origin - TimeSpan.FromMinutes(5);
                    break;
                case DateTimeRange.Prev15Min:
                    begin = origin - TimeSpan.FromMinutes(15);
                    break;
                case DateTimeRange.Prev30Min:
                    begin = origin - TimeSpan.FromMinutes(30);
                    break;
                case DateTimeRange.Prev1Hour:
                    begin = origin - TimeSpan.FromHours(1);
                    break;
                case DateTimeRange.Prev2Hour:
                    begin = origin - TimeSpan.FromHours(2);
                    break;
                case DateTimeRange.Prev4Hour:
                    begin = origin - TimeSpan.FromHours(4);
                    break;
                case DateTimeRange.Prev8Hour:
                    begin = origin - TimeSpan.FromHours(8);
                    break;
                case DateTimeRange.Prev12Hour:
                    begin = origin - TimeSpan.FromHours(12);
                    break;
                case DateTimeRange.Prev24Hour:
                    begin = origin - TimeSpan.FromHours(24);
                    break;
                case DateTimeRange.Next1Min:
                    end = origin + TimeSpan.FromMinutes(1);
                    break;
                case DateTimeRange.Next2Min:
                    end = origin + TimeSpan.FromMinutes(2);
                    break;
                case DateTimeRange.Next5Min:
                    end = origin + TimeSpan.FromMinutes(5);
                    break;
                case DateTimeRange.Next15Min:
                    end = origin + TimeSpan.FromMinutes(15);
                    break;
                case DateTimeRange.Next30Min:
                    end = origin + TimeSpan.FromMinutes(30);
                    break;
                case DateTimeRange.Next1Hour:
                    end = origin + TimeSpan.FromHours(1);
                    break;
                case DateTimeRange.Next2Hour:
                    end = origin + TimeSpan.FromHours(2);
                    break;
                case DateTimeRange.Next4Hour:
                    end = origin + TimeSpan.FromHours(4);
                    break;
                case DateTimeRange.Next8Hour:
                    end = origin + TimeSpan.FromHours(8);
                    break;
                case DateTimeRange.Next12Hour:
                    end = origin + TimeSpan.FromHours(12);
                    break;
                case DateTimeRange.Next24Hour:
                    end = origin + TimeSpan.FromHours(24);
                    break;
                case DateTimeRange.Today:
                    begin = origin.Date;
                    end   = begin + TimeSpan.FromHours(24);
                    break;
                case DateTimeRange.Yesterday:
                    begin = origin.Date - TimeSpan.FromHours(24);
                    end   = begin + TimeSpan.FromHours(24);
                    break;
                case DateTimeRange.Tommorrow:
                    begin = origin.Date + TimeSpan.FromHours(24);
                    end   = begin + TimeSpan.FromHours(24);
                    break;
                case DateTimeRange.ThisWeek:
                    begin = origin.Date.StartOfWeek();
                    end   = begin + TimeSpan.FromDays(7);
                    break;
                case DateTimeRange.LastWeek:
                    begin = origin.Date.StartOfWeek() - TimeSpan.FromDays(7);
                    end   = begin + TimeSpan.FromDays(7);
                    break;
                case DateTimeRange.NextWeek:
                    begin = origin.Date.StartOfWeek() + TimeSpan.FromDays(7);
                    end   = begin + TimeSpan.FromDays(7);
                    break;
                case DateTimeRange.ThisMonth:
                    begin = origin.Date.StartOfMonth();
                    end   = (begin + TimeSpan.FromDays(32)).StartOfMonth();
                    break;
                case DateTimeRange.LastMonth:
                    begin = (origin.Date - TimeSpan.FromDays(1)).StartOfMonth();
                    end   = (begin + TimeSpan.FromDays(32)).StartOfMonth();
                    break;
                case DateTimeRange.NextMonth:
                    begin = (origin.Date + TimeSpan.FromDays(32)).StartOfMonth();
                    end   = (begin + TimeSpan.FromDays(32)).StartOfMonth();
                    break;
                case DateTimeRange.ThisYear:
                    begin = new DateTime(origin.Year, 1, 1, 0, 0, 0, 0);
                    end   = new DateTime(begin.Year + 1, 1, 1, 0, 0, 0, 0);
                    break;
                case DateTimeRange.LastYear:
                    begin = new DateTime(origin.Year - 1, 1, 1, 0, 0, 0, 0);
                    end   = new DateTime(begin.Year + 1, 1, 1, 0, 0, 0, 0);
                    break;
                case DateTimeRange.NextYear:
                    begin = new DateTime(origin.Year + 1, 1, 1, 0, 0, 0, 0);
                    end   = new DateTime(begin.Year + 1, 1, 1, 0, 0, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(range), range, null);
            }


            return (begin, end);


        }



    }

}
