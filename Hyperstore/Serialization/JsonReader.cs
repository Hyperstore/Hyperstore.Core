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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Serialization
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Values that represent JToken.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public enum JToken
    {
        /// <summary>
        ///  Specifies the begin option.
        /// </summary>
        Begin,
        /// <summary>
        ///  Specifies the EOF option.
        /// </summary>
        EOF,
        /// <summary>
        ///  Specifies the start object option.
        /// </summary>
        StartObject,
        /// <summary>
        ///  Specifies the end object option.
        /// </summary>
        EndObject,
        /// <summary>
        ///  Specifies the start array option.
        /// </summary>
        StartArray,
        /// <summary>
        ///  Specifies the end array option.
        /// </summary>
        EndArray,
        /// <summary>
        ///  Specifies the string option.
        /// </summary>
        String,
        /// <summary>
        ///  Specifies the value option.
        /// </summary>
        Value,
        /// <summary>
        ///  Specifies the comma option.
        /// </summary>
        Comma,
        /// <summary>
        ///  Specifies the colon option.
        /// </summary>
        Colon
    }

    class JsonReader
    {
        private const int BufferSize = 2;
        private const char EOF = '\0';
        private readonly TextReader _reader;
        private char[] _buffer = new char[BufferSize];
        private int _bufferPos;
        private int _bufferLength;
        private string _value;
        private char _currentChar;
        private int _pos;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current position.
        /// </summary>
        /// <value>
        ///  The current position.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int CurrentPos
        {
            get { return _pos; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current value.
        /// </summary>
        /// <value>
        ///  The current value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string CurrentValue
        {
            get { return _value; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="reader">
        ///  The reader.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public JsonReader(TextReader reader)
        {
            _reader = reader;
            _bufferPos = _bufferLength = BufferSize; // Force first read
            NextChar();
        }

        private char NextChar()
        {
            if( _bufferPos == _bufferLength)
            {
                if( _bufferLength < BufferSize)
                {
                    return _currentChar = EOF;
                }

                ReadBuffer();
                if( _bufferLength == 0)
                {
                    return EOF;
                }
            }

            _pos++;
            _currentChar = _buffer[_bufferPos++];
            return _currentChar;
        }

        private void ReadBuffer()
        {
            _bufferLength = _reader.Read(_buffer, 0, BufferSize);
            _bufferPos = 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the read.
        /// </summary>
        /// <returns>
        ///  A JToken.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public JToken Read()
        {
            SkipWhiteSpace();
            _value = null;
            switch(_currentChar)
            {
                case EOF:
                    return JToken.EOF;
                case '[':
                    NextChar();
                    return JToken.StartArray;
                case ']':
                    NextChar();
                    return JToken.EndArray;
                case '{':
                    NextChar();
                    return JToken.StartObject;
                case '}':
                    NextChar();
                    return JToken.EndObject;
                case ':':
                    NextChar();
                    return JToken.Colon;
                case ',':
                    NextChar();
                    return JToken.Comma;
                case '"':
                    ReadString();
                    return JToken.String;
                default:
                    ReadValue();
                    return JToken.Value;
            }

        }

        private void SkipWhiteSpace()
        {
            while(true)
            {
                switch( _currentChar )
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        NextChar();
                        continue;
                    default:
                        return;
                }
            }
        }

        private void ReadValue()
        {
            var sb = new StringBuilder();
            sb.Append(_currentChar);
            while (true)
            {
                var ch = NextChar();
                if (ch == EOF)
                    throw new JsonSerializationException("EOF not expected");

                if (ch == ' ' || ch == '{' || ch == '}' || ch == '[' || ch == ']' || ch == ':' || ch == ',')
                {
                    _value = sb.ToString();
                    break;
                }

                sb.Append(ch);
            }
        }

        private void ReadString()
        {
            var sb = new StringBuilder();
            while( true )
            {
                var ch = NextChar();
                if (ch == EOF)
                    throw new JsonSerializationException("Undelimited string");

                if( ch == '"')
                {
                    NextChar();
                    _value = sb.ToString();
                    break;
                }

                if( ch == '\\')
                {
                    ch = NextChar();
                    switch (ch)
                    {
                        case '"':
                            sb.Append('\"');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        default:
                            throw new JsonSerializationException("Undelimited string");
                    }
                    continue;
                }

                sb.Append(ch);
            }
        }
    }
}
