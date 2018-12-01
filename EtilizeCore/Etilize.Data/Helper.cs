namespace Etilize.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class Helper
    {
        public static List<T> DataTableToList<T>(this DataTable table) where T: class, new()
        {
            try
            {
                List<T> list = new List<T>();
                foreach (DataRow row in table.AsEnumerable())
                {
                    T item = Activator.CreateInstance<T>();
                    PropertyInfo[] properties = item.GetType().GetProperties();
                    int index = 0;
                    while (true)
                    {
                        if (index >= properties.Length)
                        {
                            list.Add(item);
                            break;
                        }
                        PropertyInfo info = properties[index];
                        try
                        {
                            PropertyInfo property = item.GetType().GetProperty(info.Name);
                            property.SetValue(item, Convert.ChangeType(row[info.Name], property.PropertyType), null);
                        }
                        catch
                        {
                        }
                        index++;
                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }
    }
}

