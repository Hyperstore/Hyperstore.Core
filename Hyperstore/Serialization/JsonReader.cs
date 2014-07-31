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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Serialization
{
    public enum JToken
    {
        Begin,
        EOF,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        String,
        Value,
        Comma,
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

        public int CurrentPos
        {
            get { return _pos; }
        }

        public string CurrentValue
        {
            get { return _value; }
        }

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
                    throw new Exception("EOF not expected");

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
                    throw new Exception("Undelimited string");

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
                            throw new Exception("Undelimited string");
                    }
                    continue;
                }

                sb.Append(ch);
            }
        }
    }
}
