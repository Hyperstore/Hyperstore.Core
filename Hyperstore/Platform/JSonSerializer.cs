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

using Hyperstore.Modeling.Events;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Hyperstore.Modeling.Platform
{
    internal class JSonSerializer : Hyperstore.Modeling.IJsonSerializer
    {
        public string Serialize(object data)
        {
            if (data == null)
                return null;

            if (data is string)
                return (string)data;

            if (data is IModelElement)
                return Hyperstore.Modeling.Serialization.JSonSerializer.Serialize(data as IModelElement);

            var sb = new StringBuilder();
            if (data is IEvent)
            {
                SerializeEvent((IEvent)data, sb);
            }
            else { 
                SerializeValue(data, sb);
            }
            return sb.ToString();
        }

        private void SerializeEvent(IEvent o, StringBuilder sb)
        {
            bool first = true;
            Type type = o.GetType();
            sb.Append('{');

            var props = Hyperstore.Modeling.Utils.ReflectionHelper.GetProperties(type);
            foreach (var propInfo in props)
            {
                var getMethodInfo = propInfo.GetMethod;

                // Skip property if it has no get
                if (getMethodInfo == null)
                {
                    continue;
                }

                // Ignore indexed properties
                if (getMethodInfo.GetParameters().Length > 0) continue;
                var val = getMethodInfo.Invoke(o, null);
                if (val == null)
                    continue;

                if (!first) sb.Append(',');
                SerializeString(Conventions.ToJsonName( propInfo.Name ), sb);
                sb.Append(':');
                SerializeValue(val, sb);
                first = false;
            }
            sb.Append('}');
        }

        public object Deserialize(string data, object defaultValue, object obj=null)
        {
            if (data == null)
                return defaultValue;

            var val = new JSonDeserializer().Parse(data, obj);
            return val == null ? defaultValue : val;
        }

        // From Microsoft JavasScriptSerializer code
        protected void SerializeValue(object o, StringBuilder sb)
        {
            if (o == null)
            {
                sb.Append("null");
                return;
            }

            var os = o as String;
            if (os != null)
            {
                SerializeString(os, sb);
                return;
            }

            if (o is Char)
            {
                if ((char)o == '\0')
                {
                    sb.Append("null");
                    return;
                }
                SerializeString(o.ToString(), sb);
                return;
            }

            // Bools are represented as 'true' and 'false' (no quotes) in Javascript.
            if (o is bool)
            {
                SerializeBoolean((bool)o, sb);
                return;
            }

            if (o is DateTime)
            {
                SerializeDateTime((DateTime)o, sb);
                return;
            }

            if (o is DateTimeOffset)
            {
                // DateTimeOffset is converted to a UTC DateTime and serialized as usual.
                SerializeDateTime(((DateTimeOffset)o).UtcDateTime, sb);
                return;
            }

            if (o is Guid)
            {
                SerializeGuid((Guid)o, sb);
                return;
            }

            Uri uri = o as Uri;
            if (uri != null)
            {
                SerializeUri(uri, sb);
                return;
            }

            // Have to special case floats to get full precision
            if (o is double)
            {
                sb.Append(((double)o).ToString("r", CultureInfo.InvariantCulture));
                return;
            }

            if (o is float)
            {
                sb.Append(((float)o).ToString("r", CultureInfo.InvariantCulture));
                return;
            }
            if (o is int)
            {
                sb.Append(((int)o).ToString("d", CultureInfo.InvariantCulture));
                return;
            }
            if (o is long)
            {
                sb.Append(((long)o).ToString("f", CultureInfo.InvariantCulture));
                return;
            }
            if (o is short)
            {
                sb.Append(((short)o).ToString("d", CultureInfo.InvariantCulture));
                return;
            }            
            if (o is Decimal)
            {
                sb.Append(((Decimal)o).ToString("r", CultureInfo.InvariantCulture));
                return;
            }

            // Serialize enums as their integer value
            if (o is Enum)
            {
                sb.Append(((Enum)o).ToString("D"));
                return;
            }

            // Dictionaries are represented as Javascript objects.  e.g. { name1: val1, name2: val2 }
            var od = o as IDictionary;
            if (od != null)
            {
                SerializeDictionary(od, sb);
                return;
            }

            // Enumerations are represented as Javascript arrays.  e.g. [ val1, val2 ]
            IEnumerable oenum = o as IEnumerable;
            if (oenum != null)
            {
                SerializeEnumerable(oenum, sb);
                return;
            }

            // Only identity object are allowed
            if (!(o is Identity))
                throw new InvalidDataException("Only Identity object and primitives are allowed as property type for an event.");

            SerializeIdentity((Identity)o, sb);
        }

        private void SerializeIdentity(Identity identity, StringBuilder sb)
        {
            SerializeValue(identity.ToString(), sb);
        }

        private void SerializeEnumerable(IEnumerable enumerable, StringBuilder sb)
        {
            sb.Append('[');
            bool isFirstElement = true;
            foreach (object o in enumerable)
            {
                if (!isFirstElement)
                {
                    sb.Append(',');
                }

                SerializeValue(o, sb);
                isFirstElement = false;
            }
            sb.Append(']');
        }

        private void SerializeDictionaryKeyValue(string key, object value, StringBuilder sb)
        {
            SerializeString(key, sb);
            sb.Append(':');
            SerializeValue(value, sb);
        }

        private void SerializeDictionary(IDictionary o, StringBuilder sb)
        {
            sb.Append('{');
            bool isFirstElement = true;

            foreach (DictionaryEntry entry in (IDictionary)o)
            {

                string key = entry.Key as string;
                if (key == null)
                {
                    throw new ArgumentException("Invalid key value : null");
                }

                if (!isFirstElement)
                {
                    sb.Append(',');
                }
                SerializeDictionaryKeyValue(key, entry.Value, sb);
                isFirstElement = false;
            }
            sb.Append('}');
        }

        private static void SerializeUri(Uri uri, StringBuilder sb)
        {
            sb.Append("\"").Append(uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped)).Append("\"");
        }

        private static void SerializeGuid(Guid guid, StringBuilder sb)
        {
            sb.Append("\"").Append(guid.ToString()).Append("\"");
        }

        internal static readonly long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
        private static void SerializeDateTime(DateTime datetime, StringBuilder sb)
        {
            sb.Append(@"""\/Date(");
            sb.Append((datetime.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
            sb.Append(@")\/""");
        }

        private static void SerializeBoolean(bool o, StringBuilder sb)
        {
            if (o)
            {
                sb.Append("true");
            }
            else
            {
                sb.Append("false");
            }
        }

        private void SerializeString(string os, StringBuilder sb)
        {
            sb.Append('"');
            sb.Append(JavaScriptStringEncode(os));
            sb.Append('"');
        }

        protected internal virtual string JavaScriptStringEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            var b = new StringBuilder();
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                // Append the unhandled characters (that do not require special treament)
                // to the string builder when special characters are detected.
                if (CharRequiresJavaScriptEncoding(c))
                {
                    if (b == null)
                    {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0)
                    {
                        b.Append(value, startIndex, count);
                    }

                    startIndex = i + 1;
                    count = 0;
                }

                switch (c)
                {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\b':
                        b.Append("\\b");
                        break;
                    case '\f':
                        b.Append("\\f");
                        break;
                    default:
                        if (CharRequiresJavaScriptEncoding(c))
                        {
                            b.Append("\\u");
                            b.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            count++;
                        }
                        break;
                }
            }

            if (b == null)
            {
                return value;
            }

            if (count > 0)
            {
                b.Append(value, startIndex, count);
            }

            return b.ToString();
        }

        private bool CharRequiresJavaScriptEncoding(char c)
        {
            return c < 0x20 // control chars always have to be encoded
                || c == '\"' // chars which must be encoded per JSON spec
                || c == '\\'
                || c == '\'' // HTML-sensitive chars encoded for safety
                || c == '<'
                || c == '>'
                || c == '\u0085' 
                || c == '\u2028'
                || c == '\u2029';
        }
    }
}
