using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HICAPSConnectLibrary.Utils
{
    public static class TemplateProcessor
    {
        public static string Process(string template, Dictionary<string, string> fields)
        {
            return fields.Aggregate(template, (current, field) => current.Replace("{" + field.Key + "}", field.Value));
        }
    }
}
