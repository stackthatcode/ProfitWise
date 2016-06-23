using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Push.Utilities.General
{
    public static class ObjectIntrospection
    {

        public static string DumpProperties(this object obj)
        {
            var sb = new StringBuilder();
            DumpProperties(obj, 0, sb);
            return sb.ToString();
        }

        private static void DumpProperties(object obj, int indent, StringBuilder output)
        {
            if (obj == null)
            {
                return;
            }

            var indentString = new string(' ', indent);
            var objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(obj, null);
                var elems = propValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        DumpProperties(item, indent + 3, output);
                    }
                }
                else
                {
                    // This will not cut-off System.Collections because of the first check
                    if (property.PropertyType.Assembly == objType.Assembly)
                    {
                        output.Append($"{indentString}{property.Name}:" + Environment.NewLine);
                        DumpProperties(propValue, indent + 2, output);
                    }
                    else
                    {
                        output.Append($"{indentString}{property.Name}: {propValue}" + Environment.NewLine);
                    }
                }
            }
        }
    }
}
