using System;

namespace OpenSource.Library.DataAccess
{
  public static class DbConvert
  {
    public static object ToDBNull(byte value)
    {
      return value != (byte) 0 ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(short value)
    {
      return value != (short) 0 ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(int value)
    {
      return value != 0 ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(long value)
    {
      return value != 0L ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(Decimal value)
    {
      return !(value == new Decimal(0)) ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(double value)
    {
      return value != 0.0 ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(string value)
    {
      return value != null && !(value == string.Empty) ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(DateTime value)
    {
      return !(value == DateTime.MinValue) ? (object) value : Convert.DBNull;
    }

    public static object ToDBNull(DateTime? value)
    {
      if (value.HasValue)
      {
        DateTime? nullable = value;
        DateTime minValue = DateTime.MinValue;
        if ((!nullable.HasValue ? 0 : (nullable.GetValueOrDefault() == minValue ? 1 : 0)) == 0)
          return (object) value;
      }
      return Convert.DBNull;
    }

    public static short ToInt16(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToInt16(value) : (short) 0;
    }

    public static int ToInt32(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToInt32(value) : 0;
    }

    public static long ToInt64(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToInt64(value) : 0L;
    }

    public static byte ToByte(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToByte(value) : (byte) 0;
    }

    public static string ToString(object value)
    {
      return !Convert.IsDBNull(value) ? value.ToString() : string.Empty;
    }

    public static Decimal ToDecimal(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToDecimal(value) : new Decimal(0);
    }

    public static double ToDouble(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToDouble(value) : 0.0;
    }

    public static DateTime ToDateTime(object value)
    {
      return !Convert.IsDBNull(value) ? Convert.ToDateTime(value) : DateTime.MinValue;
    }

    public static bool ToBoolean(object value)
    {
      return !Convert.IsDBNull(value) && Convert.ToBoolean(value);
    }

    public static Decimal? ToNullableDecimal(object value)
    {
      return !Convert.IsDBNull(value) ? new Decimal?(Convert.ToDecimal(value)) : new Decimal?();
    }

    public static DateTime? ToNullableDateTime(object value)
    {
      return !Convert.IsDBNull(value) ? new DateTime?(Convert.ToDateTime(value)) : new DateTime?();
    }

    public static TimeSpan ToTimeSpan(object value)
    {
      return !Convert.IsDBNull(value) ? (TimeSpan) value : TimeSpan.MinValue;
    }
  }
}
