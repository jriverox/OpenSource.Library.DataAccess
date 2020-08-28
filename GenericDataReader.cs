using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace OpenSource.Library.DataAccess
{
  public class GenericDataReader<T> : DbDataReader where T : class
  {
    private List<PropertyInfo> properties = new List<PropertyInfo>();
    private Dictionary<string, int> propertyNames = new Dictionary<string, int>((IEqualityComparer<string>) StringComparer.CurrentCultureIgnoreCase);
    private IEnumerator<T> list;
    private int recordsAffected;
    private int numericScale;

    public GenericDataReader(IEnumerable<T> list)
    {
      this.numericScale = 4;
      this.list = list.GetEnumerator();
      int num = 0;
      foreach (PropertyInfo property in typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty))
      {
        if (property.PropertyType.IsPrimitive || property.PropertyType == typeof (Decimal) || (property.PropertyType == typeof (string) || property.PropertyType.IsEnum) || (property.PropertyType == typeof (DateTime) || property.PropertyType == typeof (TimeSpan)))
        {
          bool flag = true;
          foreach (object customAttribute in property.GetCustomAttributes(false))
          {
            if (customAttribute is ViewPropertyAttribute)
            {
              flag = false;
              break;
            }
          }
          if (flag)
          {
            this.properties.Add(property);
            this.propertyNames.Add(property.Name, num);
            ++num;
          }
        }
      }
    }

    public override bool HasRows
    {
      get
      {
        return true;
      }
    }

    public override IEnumerator GetEnumerator()
    {
      return (IEnumerator) this.list;
    }

    public override void Close()
    {
      this.list.Dispose();
      this.list = (IEnumerator<T>) null;
    }

    public override int Depth
    {
      get
      {
        return 0;
      }
    }

    public override DataTable GetSchemaTable()
    {
      DataTable dataTable = new DataTable();
      foreach (PropertyInfo property in this.properties)
      {
        Type propertyType = property.PropertyType;
        dataTable.Columns.Add(property.Name, property.PropertyType);
      }
      DataTable schemaTable;
      using (DataTableReader dataReader = dataTable.CreateDataReader())
        schemaTable = dataReader.GetSchemaTable();
      foreach (DataRow row in (InternalDataCollectionBase) schemaTable.Rows)
      {
        if (row["DataType"].ToString() == "System.Decimal")
        {
          row["NumericPrecision"] = (object) 29;
          row["NumericScale"] = (object) this.numericScale;
          row.AcceptChanges();
        }
      }
      return schemaTable;
    }

    public override bool IsClosed
    {
      get
      {
        return this.list == null;
      }
    }

    public override bool NextResult()
    {
      return false;
    }

    public override bool Read()
    {
      bool flag = this.list.MoveNext();
      if (flag)
        ++this.recordsAffected;
      return flag;
    }

    public override int RecordsAffected
    {
      get
      {
        return this.recordsAffected;
      }
    }

    public override int FieldCount
    {
      get
      {
        return this.properties.Count;
      }
    }

    public override bool GetBoolean(int i)
    {
      return (bool) this.GetValue(i);
    }

    public override byte GetByte(int i)
    {
      return (byte) this.GetValue(i);
    }

    public override long GetBytes(
      int i,
      long fieldOffset,
      byte[] buffer,
      int bufferOffset,
      int length)
    {
      byte[] numArray = (byte[]) this.GetValue(i);
      int num1 = numArray.Length - (int) fieldOffset;
      if (num1 > length)
        num1 = length;
      int num2 = (int) fieldOffset + num1 - 1;
      int index1 = (int) fieldOffset;
      int index2 = bufferOffset;
      while (index1 <= num2)
      {
        buffer[index2] = numArray[index1];
        ++index1;
        ++bufferOffset;
      }
      return (long) num1;
    }

    public override char GetChar(int i)
    {
      return (char) this.GetValue(i);
    }

    public override long GetChars(
      int i,
      long fieldOffset,
      char[] buffer,
      int bufferOffset,
      int length)
    {
      string str = (string) this.GetValue(i);
      int count = str.Length - (int) fieldOffset;
      if (count > length)
        count = length;
      str.CopyTo((int) fieldOffset, buffer, bufferOffset, count);
      return (long) count;
    }

    protected override DbDataReader GetDbDataReader(int ordinal)
    {
      throw new NotSupportedException();
    }

    public override string GetDataTypeName(int i)
    {
      return this.properties[i].PropertyType.FullName;
    }

    public override DateTime GetDateTime(int i)
    {
      return (DateTime) this.GetValue(i);
    }

    public override Decimal GetDecimal(int i)
    {
      return (Decimal) this.GetValue(i);
    }

    public override double GetDouble(int i)
    {
      return (double) this.GetValue(i);
    }

    public override Type GetFieldType(int i)
    {
      return this.properties[i].PropertyType;
    }

    public override float GetFloat(int i)
    {
      return (float) this.GetValue(i);
    }

    public override Guid GetGuid(int i)
    {
      return (Guid) this.GetValue(i);
    }

    public override short GetInt16(int i)
    {
      return (short) this.GetValue(i);
    }

    public override int GetInt32(int i)
    {
      return (int) this.GetValue(i);
    }

    public override long GetInt64(int i)
    {
      return (long) this.GetValue(i);
    }

    public override string GetName(int i)
    {
      return this.properties[i].Name;
    }

    public override int GetOrdinal(string name)
    {
      return this.propertyNames[name];
    }

    public override string GetString(int i)
    {
      return (string) this.GetValue(i);
    }

    public override object GetValue(int i)
    {
      return this.properties[i].GetValue((object) this.list.Current, (object[]) null);
    }

    public override int GetValues(object[] values)
    {
      for (int ordinal = 0; ordinal < this.properties.Count; ++ordinal)
        values[ordinal] = this.GetValue(ordinal);
      return this.properties.Count;
    }

    public override bool IsDBNull(int i)
    {
      return this.GetValue(i) == null;
    }

    public override object this[string name]
    {
      get
      {
        return this.GetValue(this.GetOrdinal(name));
      }
    }

    public override object this[int i]
    {
      get
      {
        return this.GetValue(i);
      }
    }
  }
}
