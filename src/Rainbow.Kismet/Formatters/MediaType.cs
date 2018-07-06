using Microsoft.Extensions.Primitives;
using System;
using System.Text;

namespace Rainbow.Kismet.Formatters
{
    internal struct MediaType
    {
        private static readonly StringSegment QualityParameter = new StringSegment("q");

        private readonly MediaTypeParameterParser _parameterParser;

        public MediaType(string mediaType)
            : this(mediaType, 0, mediaType.Length)
        {
        }

        public MediaType(StringSegment mediaType)
            : this(mediaType.Buffer, mediaType.Offset, mediaType.Length)
        {
        }

        public MediaType(string mediaType, int offset, int? length)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }

            if (offset < 0 || offset >= mediaType.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length != null)
            {
                if (length < 0 || length > mediaType.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }

                if (offset > mediaType.Length - length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
            }

            _parameterParser = default(MediaTypeParameterParser);

            var typeLength = GetTypeLength(mediaType, offset, out var type);
            if (typeLength == 0)
            {
                Type = new StringSegment();
                SubType = new StringSegment();
                SubTypeWithoutSuffix = new StringSegment();
                SubTypeSuffix = new StringSegment();
                return;
            }
            else
            {
                Type = type;
            }

            var subTypeLength = GetSubtypeLength(mediaType, offset + typeLength, out var subType);
            if (subTypeLength == 0)
            {
                SubType = new StringSegment();
                SubTypeWithoutSuffix = new StringSegment();
                SubTypeSuffix = new StringSegment();
                return;
            }
            else
            {
                SubType = subType;

                if (TryGetSuffixLength(subType, out var subtypeSuffixLength))
                {
                    SubTypeWithoutSuffix = subType.Subsegment(0, subType.Length - subtypeSuffixLength - 1);
                    SubTypeSuffix = subType.Subsegment(subType.Length - subtypeSuffixLength, subtypeSuffixLength);
                }
                else
                {
                    SubTypeWithoutSuffix = SubType;
                    SubTypeSuffix = new StringSegment();
                }
            }

            _parameterParser = new MediaTypeParameterParser(mediaType, offset + typeLength + subTypeLength, length);
        }

        private static int GetTypeLength(string input, int offset, out StringSegment type)
        {
            if (offset < 0 || offset >= input.Length)
            {
                type = default(StringSegment);
                return 0;
            }

            var current = offset + HttpTokenParsingRules.GetWhitespaceLength(input, offset);

            var typeLength = HttpTokenParsingRules.GetTokenLength(input, current);
            if (typeLength == 0)
            {
                type = default(StringSegment);
                return 0;
            }

            type = new StringSegment(input, current, typeLength);

            current += typeLength;
            current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

            return current - offset;
        }

        private static int GetSubtypeLength(string input, int offset, out StringSegment subType)
        {
            var current = offset;

            if (current < 0 || current >= input.Length || input[current] != '/')
            {
                subType = default(StringSegment);
                return 0;
            }

            current++; // skip delimiter.
            current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

            var subtypeLength = HttpTokenParsingRules.GetTokenLength(input, current);
            if (subtypeLength == 0)
            {
                subType = default(StringSegment);
                return 0;
            }

            subType = new StringSegment(input, current, subtypeLength);

            current += subtypeLength;
            current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

            return current - offset;
        }

        private static bool TryGetSuffixLength(StringSegment subType, out int suffixLength)
        {
            var startPos = subType.Offset + subType.Length - 1;
            for (var currentPos = startPos; currentPos >= subType.Offset; currentPos--)
            {
                if (subType.Buffer[currentPos] == '+')
                {
                    suffixLength = startPos - currentPos;
                    return true;
                }
            }

            suffixLength = 0;
            return false;
        }

        public StringSegment Type { get; }

        public bool MatchesAllTypes => Type.Equals("*", StringComparison.OrdinalIgnoreCase);

        public StringSegment SubType { get; }

        public StringSegment SubTypeWithoutSuffix { get; }

        public StringSegment SubTypeSuffix { get; }

        public bool MatchesAllSubTypes => SubType.Equals("*", StringComparison.OrdinalIgnoreCase);

        public bool MatchesAllSubTypesWithoutSuffix => SubTypeWithoutSuffix.Equals("*", StringComparison.OrdinalIgnoreCase);

        public Encoding Encoding => GetEncodingFromCharset(GetParameter("charset"));

        public StringSegment Charset => GetParameter("charset");

        public bool HasWildcard
        {
            get
            {
                return MatchesAllTypes ||
                    MatchesAllSubTypesWithoutSuffix ||
                    GetParameter("*").Equals("*", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsSubsetOf(MediaType set)
        {
            return MatchesType(set) &&
                MatchesSubtype(set) &&
                ContainsAllParameters(set._parameterParser);
        }

        public StringSegment GetParameter(string parameterName)
        {
            return GetParameter(new StringSegment(parameterName));
        }

        public StringSegment GetParameter(StringSegment parameterName)
        {
            var parametersParser = _parameterParser;

            while (parametersParser.ParseNextParameter(out var parameter))
            {
                if (parameter.HasName(parameterName))
                {
                    return parameter.Value;
                }
            }

            return new StringSegment();
        }

        public static string ReplaceEncoding(string mediaType, Encoding encoding)
        {
            return ReplaceEncoding(new StringSegment(mediaType), encoding);
        }

        public static string ReplaceEncoding(StringSegment mediaType, Encoding encoding)
        {
            var parsedMediaType = new MediaType(mediaType);
            var charset = parsedMediaType.GetParameter("charset");

            if (charset.HasValue && charset.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase))
            {
                return mediaType.Value;
            }

            if (!charset.HasValue)
            {
                return CreateMediaTypeWithEncoding(mediaType, encoding);
            }

            var charsetOffset = charset.Offset - mediaType.Offset;
            var restOffset = charsetOffset + charset.Length;
            var restLength = mediaType.Length - restOffset;
            var finalLength = charsetOffset + encoding.WebName.Length + restLength;

            var builder = new StringBuilder(mediaType.Buffer, mediaType.Offset, charsetOffset, finalLength);
            builder.Append(encoding.WebName);
            builder.Append(mediaType.Buffer, restOffset, restLength);

            return builder.ToString();
        }

        public static Encoding GetEncoding(string mediaType)
        {
            return GetEncoding(new StringSegment(mediaType));
        }

        public static Encoding GetEncoding(StringSegment mediaType)
        {
            var parsedMediaType = new MediaType(mediaType);
            return parsedMediaType.Encoding;
        }

        private static Encoding GetEncodingFromCharset(StringSegment charset)
        {
            if (charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }

            try
            {
                return charset.HasValue ? Encoding.GetEncoding(charset.Value) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CreateMediaTypeWithEncoding(StringSegment mediaType, Encoding encoding)
        {
            return $"{mediaType.Value}; charset={encoding.WebName}";
        }

        private bool MatchesType(MediaType set)
        {
            return set.MatchesAllTypes ||
                set.Type.Equals(Type, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesSubtype(MediaType set)
        {
            if (set.MatchesAllSubTypes)
            {
                return true;
            }

            if (set.SubTypeSuffix.HasValue)
            {
                if (SubTypeSuffix.HasValue)
                {
                    return MatchesSubtypeWithoutSuffix(set) && MatchesSubtypeSuffix(set);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return set.SubType.Equals(SubType, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool MatchesSubtypeWithoutSuffix(MediaType set)
        {
            return set.MatchesAllSubTypesWithoutSuffix ||
                set.SubTypeWithoutSuffix.Equals(SubTypeWithoutSuffix, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesSubtypeSuffix(MediaType set)
        {
            return set.SubTypeSuffix.Equals(SubTypeSuffix, StringComparison.OrdinalIgnoreCase);
        }

        private bool ContainsAllParameters(MediaTypeParameterParser setParameters)
        {
            var parameterFound = true;
            while (setParameters.ParseNextParameter(out var setParameter) && parameterFound)
            {
                if (setParameter.HasName("q"))
                {
                    break;
                }

                if (setParameter.HasName("*"))
                {
                    continue;
                }
                
                var subSetParameters = _parameterParser;
                parameterFound = false;
                while (subSetParameters.ParseNextParameter(out var subSetParameter) && !parameterFound)
                {
                    parameterFound = subSetParameter.Equals(setParameter);
                }
            }

            return parameterFound;
        }

        private struct MediaTypeParameterParser
        {
            private readonly string _mediaTypeBuffer;
            private readonly int? _length;

            public MediaTypeParameterParser(string mediaTypeBuffer, int offset, int? length)
            {
                _mediaTypeBuffer = mediaTypeBuffer;
                _length = length;
                CurrentOffset = offset;
                ParsingFailed = false;
            }

            public int CurrentOffset { get; private set; }

            public bool ParsingFailed { get; private set; }

            public bool ParseNextParameter(out MediaTypeParameter result)
            {
                if (_mediaTypeBuffer == null)
                {
                    ParsingFailed = true;
                    result = default(MediaTypeParameter);
                    return false;
                }

                var parameterLength = GetParameterLength(_mediaTypeBuffer, CurrentOffset, out result);
                CurrentOffset += parameterLength;

                if (parameterLength == 0)
                {
                    ParsingFailed = _length != null && CurrentOffset < _length;
                    return false;
                }

                return true;
            }

            private static int GetParameterLength(string input, int startIndex, out MediaTypeParameter parsedValue)
            {
                if (OffsetIsOutOfRange(startIndex, input.Length) || input[startIndex] != ';')
                {
                    parsedValue = default(MediaTypeParameter);
                    return 0;
                }

                var nameLength = GetNameLength(input, startIndex, out var name);

                var current = startIndex + nameLength;

                if (nameLength == 0 || OffsetIsOutOfRange(current, input.Length) || input[current] != '=')
                {
                    if (current == input.Length && name.Equals("*", StringComparison.OrdinalIgnoreCase))
                    {
                        var asterisk = new StringSegment("*");
                        parsedValue = new MediaTypeParameter(asterisk, asterisk);
                        return current - startIndex;
                    }
                    else
                    {
                        parsedValue = default(MediaTypeParameter);
                        return 0;
                    }
                }

                var valueLength = GetValueLength(input, current, out var value);

                parsedValue = new MediaTypeParameter(name, value);
                current += valueLength;

                return current - startIndex;
            }

            private static int GetNameLength(string input, int startIndex, out StringSegment name)
            {
                var current = startIndex;

                current++; // skip ';'
                current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

                var nameLength = HttpTokenParsingRules.GetTokenLength(input, current);
                if (nameLength == 0)
                {
                    name = default(StringSegment);
                    return 0;
                }

                name = new StringSegment(input, current, nameLength);

                current += nameLength;
                current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

                return current - startIndex;
            }

            private static int GetValueLength(string input, int startIndex, out StringSegment value)
            {
                var current = startIndex;

                current++;
                current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

                var valueLength = HttpTokenParsingRules.GetTokenLength(input, current);

                if (valueLength == 0)
                {
                    var result = HttpTokenParsingRules.GetQuotedStringLength(input, current, out valueLength);
                    if (result != HttpParseResult.Parsed)
                    {
                        value = default(StringSegment);
                        return 0;
                    }
                    
                    value = new StringSegment(input, current + 1, valueLength - 2);
                }
                else
                {
                    value = new StringSegment(input, current, valueLength);
                }

                current += valueLength;
                current += HttpTokenParsingRules.GetWhitespaceLength(input, current);

                return current - startIndex;
            }

            private static bool OffsetIsOutOfRange(int offset, int length)
            {
                return offset < 0 || offset >= length;
            }
        }

        private struct MediaTypeParameter : IEquatable<MediaTypeParameter>
        {
            public MediaTypeParameter(StringSegment name, StringSegment value)
            {
                Name = name;
                Value = value;
            }

            public StringSegment Name { get; }

            public StringSegment Value { get; }

            public bool HasName(string name)
            {
                return HasName(new StringSegment(name));
            }

            public bool HasName(StringSegment name)
            {
                return Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            }

            public bool Equals(MediaTypeParameter other)
            {
                return HasName(other.Name) && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            }

            public override string ToString() => $"{Name}={Value}";
        }
    }
}
