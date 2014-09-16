// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A message helper.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class MessageHelper
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Helper to format a message with named item corresponding to a property element. Each item
        ///  must be declared with a property name. The format to use is {propertyName[,length][:formatstring]}
        /// </summary>
        /// <param name="message">
        ///  Message to format.
        /// </param>
        /// <param name="elem">
        ///  Element to use.
        /// </param>
        /// <returns>
        ///  a formatted string.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static string CreateMessage(string message, IModelElement elem)
        {
            if (elem == null)
                return message;

            var result = ReplacePropertyNameWhithIndexValues(message);
            List<object> values = new List<object>();
            foreach (var property in result.Item2)
            {
                switch (property)
                {
                    case "Id":
                        values.Add(elem.Id);
                        break;
                    case "DomainModel":
                        values.Add(elem.DomainModel.Name);
                        break;
                    case "SchemaInfo":
                        values.Add(elem.SchemaInfo.Id);
                        break;
                    default:
                        var schemaProperty = elem.SchemaInfo.GetProperties(true).FirstOrDefault(p => String.Compare(p.Name, property, StringComparison.Ordinal) == 0);
                        PropertyValue pv = null;
                        if (schemaProperty != null && (pv=elem.GetPropertyValue(schemaProperty)) != null)
                        {
                            values.Add(pv.Value);
                        }
                        else
                        {
                            values.Add(null);
                        }
                        break;
                }
            }

            return string.Format(result.Item1, values.ToArray());
        }

        private static Tuple<string, string[]> ReplacePropertyNameWhithIndexValues(string message)
        {
            Dictionary<string, int> properties = new Dictionary<string, int>();
            string newMessage = message;
            var ix = 0;
            var regex = new Regex(@"(?<!{){\s*(\w+)\s*(,\s*-?\d*)?\s*(:(?:.[^}]*))?\s*}(?!})", RegexOptions.IgnoreCase);
            foreach (Match m in regex.Matches(message))
            {
                var propertyName = m.Groups[1].Value;
                int index;
                if (!properties.TryGetValue(propertyName, out index))
                {
                    index = ix++;
                    properties.Add(propertyName, index);
                }
                else
                {
                    switch (propertyName)
                    {
                        case "Id":
                        case "DomainModel":
                        case "SchemaInfo":
                            index = ix++;
                            properties.Add(propertyName, index);
                            break;
                    }
                }

                newMessage = newMessage.Replace(m.Groups[0].Value, string.Format("{{{0}{1}{2}}}", index, m.Groups[2], m.Groups[3]));
            }
            return Tuple.Create(newMessage, properties.OrderBy(kv => kv.Value).Select(kv => kv.Key).ToArray());
        }
    }
}
