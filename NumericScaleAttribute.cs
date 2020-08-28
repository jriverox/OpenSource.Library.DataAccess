using System;

namespace OpenSource.Library.DataAccess
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class NumericScaleAttribute : Attribute
  {
    private int scale;

    public NumericScaleAttribute(int scale)
    {
      this.scale = scale;
    }

    public int Scale
    {
      get
      {
        return this.scale;
      }
      set
      {
        this.scale = value;
      }
    }
  }
}
