using System;

namespace OpenSource.Library.DataAccess
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class ViewPropertyAttribute : Attribute
  {
  }
}
