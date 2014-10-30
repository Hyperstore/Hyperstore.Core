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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform
{
    class JSonDeserializer
    {
        enum TokenKind
        {
            Eof,
            LBracket,
            RBracket,
            LCurly,
            RCurly,
            Colon,
            Comma,
            True,
            False,
            Null,
            String,
            Number
        }

        struct TokenInfo
        {
            public TokenKind Kind;
            public object Value;
        }

        private Lexer lexer;

        class Lexer
        {
            private readonly string _buffer;
            private int _position;
            private const char Eof = Char.MaxValue;
            private TokenInfo info;
            public TokenInfo Current { get { return info; } }

            public Lexer(string text)
            {
                _buffer = text;
                _position = 0;
            }

            public TokenInfo NextToken()
            {
                info.Value = null;

                for (; ; )
                {
                    var ch = PeekChar();
                    if (ch == Eof)
                    {
                        info.Kind = TokenKind.Eof;
                        return info;
                    }

                    switch (ch)
                    {
                        case '\u0009':
                        case '\u000A':
                        case '\u000D':
                        case '\u0020':
                            AdvanceChar();
                            break;
                        case '{':
                            AdvanceChar();
                            info.Kind = TokenKind.LCurly;
                            return info;
                        case '}':
                            AdvanceChar();
                            info.Kind = TokenKind.RCurly;
                            return info;
                        case '[':
                            AdvanceChar();
                            info.Kind = TokenKind.LBracket;
                            return info;
                        case ']':
                            AdvanceChar();
                            info.Kind = TokenKind.RBracket;
                            return info;
                        case ':':
                            AdvanceChar();
                            info.Kind = TokenKind.Colon;
                            return info;
                        case ',':
                            AdvanceChar();
                            info.Kind = TokenKind.Comma;
                            return info;
                        case '\"':
                            info.Kind = TokenKind.String;
                            info.Value = ParseString();
                            return info;
                        default:
                            if (ch == '-' || Char.IsDigit(ch))
                            {
                                info.Kind = TokenKind.Number;
                                info.Value = ParseNumber();
                                return info;
                            }
                            if (ch == 't' && PeekChar(1) == 'r' && PeekChar(2) == 'u' && PeekChar(3) == 'e')
                            {
                                info.Kind = TokenKind.True;
                                AdvanceChar(4);
                                return info;
                            }

                            if (ch == 'f' && PeekChar(1) == 'a' && PeekChar(2) == 'l' && PeekChar(3) == 's' && PeekChar(4) == 'e')
                            {
                                info.Kind = TokenKind.False;
                                AdvanceChar(5);
                                return info;
                            }

                            if (ch == 'n' && PeekChar(1) == 'u' && PeekChar(2) == 'l' && PeekChar(3) == 'l')
                            {
                                info.Kind = TokenKind.Null;
                                AdvanceChar(4);
                                return info;
                            }

                            throw new Exception("Malformed json string");
                    }
                }
            }

            private string ParseNumber()
            {
                var sb = new StringBuilder();
                var ch = PeekChar();
                while (ch == '_' || ch == '+' || ch == 'e' || ch == 'E' || ch == '.' || Char.IsDigit(ch))
                {
                    sb.Append(ch);
                    AdvanceChar();
                    ch = PeekChar();
                }
                return sb.ToString();
            }

            private string ParseString()
            {
                var sb = new StringBuilder();

                AdvanceChar();
                for (; ; )
                {
                    var ch = PeekChar();
                    switch (ch)
                    {
                        case '"':
                            AdvanceChar();
                            return sb.ToString();
                        case '\\':
                            AdvanceChar();
                            ch = PeekChar();
                            switch (ch)
                            {
                                case '"':
                                case '\\':
                                case 'b':
                                case 'f':
                                case 'n':
                                case 'r':
                                case 't':
                                    sb.Append(ch);
                                    AdvanceChar();
                                    break;
                                case 'u':
                                    AdvanceChar();
                                    for (int i = 0; i < 4; i++)
                                    {
                                        sb.Append(PeekChar(i));
                                    }
                                    AdvanceChar(4);
                                    break;
                                default:
                                    throw new Exception("Invalid character in string");
                            }
                            break;
                        case Eof:
                            throw new Exception("Malformed string");
                        default:
                            if (Char.IsControl(ch))
                                throw new Exception("Invalid character in string");
                            sb.Append(ch);
                            AdvanceChar();
                            break;
                    }
                }
            }

            private void AdvanceChar(int delta = 1)
            {
                _position += delta;
            }

            private char PeekChar(int delta = 0)
            {
                var ch = _position + delta < _buffer.Length ? _buffer[_position + delta] : Eof;
                return ch;
            }
        }

        public object Parse(string text, object obj = null)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            lexer = new Lexer(text);
            TokenInfo token = lexer.NextToken();
            if (token.Kind == TokenKind.LCurly)
            {
                DebugContract.Assert(obj != null);
                ParseObject(obj);
                return obj;
            }

            return ParseValue();
        }

        private object ParseValue()
        {
            object value = null;
            switch (lexer.Current.Kind)
            {
                case TokenKind.True:
                    value = true;
                    lexer.NextToken();
                    break;
                case TokenKind.False:
                    value = false;
                    lexer.NextToken();
                    break;
                case TokenKind.Null:
                    value = null;
                    lexer.NextToken();
                    break;
                case TokenKind.LBracket:
                    return ParseArray();
                case TokenKind.Number:
                    value = ParseNumber(lexer.Current.Value.ToString());
                    lexer.NextToken();
                    break;
                case TokenKind.String:
                    value = lexer.Current.Value;
                    lexer.NextToken();
                    break;
                default:
                    throw new Exception("Malformed json string. Value expected");
            }
            return value;
        }

        private object ParseNumber(string input)
        {
            bool hasDecimalPoint = input.IndexOf('.') >= 0;
            bool hasExponent = input.LastIndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!hasExponent)
            {
                if (!hasDecimalPoint)
                {
                    int n;
                    if (Int32.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                    {
                        return n;
                    }

                    long l;
                    if (Int64.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out l))
                    {
                        return l;
                    }
                }

                decimal dec;
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out dec))
                {
                    return dec;
                }
            }

            Double d;
            if (Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
            {
                return d;
            }

            // must be an illegal primitive
            throw new Exception(String.Format(CultureInfo.InvariantCulture, "Illegal number {0}", input));
        }

        private object ParseArray()
        {
            var dic = new List<object>();
            Accept(TokenKind.LBracket);
            while (lexer.Current.Kind != TokenKind.RBracket)
            {
                var value = ParseValue();
                dic.Add(value);
                if (lexer.Current.Kind == TokenKind.Comma)
                    Accept(TokenKind.Comma);
                else if (lexer.Current.Kind != TokenKind.RBracket)
                    throw new Exception("Unclosed array");
            }
            Accept(TokenKind.RBracket);
            return dic;
        }

        private void ParseObject(object obj)
        {
            var type = obj.GetType();
            var props = Hyperstore.Modeling.Utils.ReflectionHelper.GetProperties(type).ToArray();
            Accept(TokenKind.LCurly);
            while (lexer.Current.Kind != TokenKind.RCurly)
            {
                var name = lexer.Current.Value.ToString();
                if (String.IsNullOrEmpty(name))
                    throw new Exception(String.Format("Invalid name {0}", name));

                Accept(TokenKind.String);
                Accept(TokenKind.Colon);
                var value = ParseValue();
                name = Char.ToUpper(name[0]) + name.Substring(1);

                var prop = props.FirstOrDefault(p => p.Name == name);
                if (prop != null)
                {
                    if (prop.PropertyType == typeof(Identity))
                        value = Identity.Parse((string)value);
                    prop.SetValue(obj, value);
                }
                if (lexer.Current.Kind == TokenKind.Comma)
                    Accept(TokenKind.Comma);
                else if (lexer.Current.Kind != TokenKind.RCurly)
                    throw new Exception("Unclosed object");
            }
            Accept(TokenKind.RCurly);
        }

        private TokenInfo Accept(TokenKind tokenKind)
        {
            if (lexer.Current.Kind != tokenKind)
                throw new Exception(String.Format("{0} expected", tokenKind));
            return lexer.NextToken();
        }
    }
}
