using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fabrica.Persistence.Converters
{

    public static class ValueConverters
    {

        public static readonly DateTime MinDateTime = new DateTime(1883, 11, 19, 0, 0, 0, 0);

        public static readonly ValueConverter<DateTime, DateTime> MinDateTimeConverter = new ValueConverter<DateTime, DateTime>(time => time != DateTime.MinValue ? time : MinDateTime, time => time != MinDateTime ? time : DateTime.MinValue);


        public static readonly DateTime MaxDateTime = new DateTime(2200, 1, 1, 0, 0, 0, 0 );

        public static readonly ValueConverter<DateTime, DateTime> MaxDateTimeConverter = new ValueConverter<DateTime, DateTime>(time => time != DateTime.MaxValue ? time : MaxDateTime, time => time != MaxDateTime ? time : DateTime.MaxValue);

    }


}
