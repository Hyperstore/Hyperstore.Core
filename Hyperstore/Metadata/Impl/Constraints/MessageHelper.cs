//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
//
//		This file is part of Hyperstore (http://www.hyperstore.org)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
 
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
