using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Ef.Converters;

public static class ValueConverters
{

    public static readonly DateTime MinDateTime = new (1883, 11, 19, 0, 0, 0, 0);

    public static readonly ValueConverter<DateTime, DateTime> MinDateTimeConverter = new (time => time != DateTime.MinValue ? time : MinDateTime, time => time != MinDateTime ? time : DateTime.MinValue);


    public static readonly DateTime MaxDateTime = new(2200, 1, 1, 0, 0, 0, 0 );

    public static readonly ValueConverter<DateTime, DateTime> MaxDateTimeConverter = new(time => time != DateTime.MaxValue ? time : MaxDateTime, time => time != MaxDateTime ? time : DateTime.MaxValue);

}