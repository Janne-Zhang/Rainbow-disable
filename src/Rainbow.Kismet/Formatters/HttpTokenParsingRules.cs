using System.Diagnostics.Contracts;
using System.Text;

namespace Rainbow.Kismet.Formatters
{
    internal static class HttpTokenParsingRules
    {
        private static readonly bool[] TokenChars;
        private const int MaxNestedCount = 5;

        internal const char CR = '\r';
        internal const char LF = '\n';
        internal const char SP = ' ';
        internal const char Tab = '\t';
        internal const int MaxInt64Digits = 19;
        internal const int MaxInt32Digits = 10;

        // iso-8859-1, Western European (ISO)
        internal static readonly Encoding DefaultHttpEncoding = Encoding.GetEncoding(28591);

        static HttpTokenParsingRules()
        {

            TokenChars = new bool[128]; 

            for (int i = 33; i < 127; i++) 
            {
                TokenChars[i] = true;
            }
            
            TokenChars[(byte)'('] = false;
            TokenChars[(byte)')'] = false;
            TokenChars[(byte)'<'] = false;
            TokenChars[(byte)'>'] = false;
            TokenChars[(byte)'@'] = false;
            TokenChars[(byte)','] = false;
            TokenChars[(byte)';'] = false;
            TokenChars[(byte)':'] = false;
            TokenChars[(byte)'\\'] = false;
            TokenChars[(byte)'"'] = false;
            TokenChars[(byte)'/'] = false;
            TokenChars[(byte)'['] = false;
            TokenChars[(byte)']'] = false;
            TokenChars[(byte)'?'] = false;
            TokenChars[(byte)'='] = false;
            TokenChars[(byte)'{'] = false;
            TokenChars[(byte)'}'] = false;
        }

        internal static bool IsTokenChar(char character)
        {
            if (character > 127)
            {
                return false;
            }

            return TokenChars[character];
        }

        internal static int GetTokenLength(string input, int startIndex)
        {
            Contract.Requires(input != null);
            Contract.Ensures((Contract.Result<int>() >= 0) && (Contract.Result<int>() <= (input.Length - startIndex)));

            if (startIndex >= input.Length)
            {
                return 0;
            }

            var current = startIndex;

            while (current < input.Length)
            {
                if (!IsTokenChar(input[current]))
                {
                    return current - startIndex;
                }
                current++;
            }
            return input.Length - startIndex;
        }

        internal static int GetWhitespaceLength(string input, int startIndex)
        {
            Contract.Requires(input != null);
            Contract.Ensures((Contract.Result<int>() >= 0) && (Contract.Result<int>() <= (input.Length - startIndex)));

            if (startIndex >= input.Length)
            {
                return 0;
            }

            var current = startIndex;

            while (current < input.Length)
            {
                var c = input[current];

                if ((c == SP) || (c == Tab))
                {
                    current++;
                    continue;
                }

                if (c == CR)
                {
                    if ((current + 2 < input.Length) && (input[current + 1] == LF))
                    {
                        char spaceOrTab = input[current + 2];
                        if ((spaceOrTab == SP) || (spaceOrTab == Tab))
                        {
                            current += 3;
                            continue;
                        }
                    }
                }

                return current - startIndex;
            }
            
            return input.Length - startIndex;
        }

        internal static HttpParseResult GetQuotedStringLength(string input, int startIndex, out int length)
        {
            var nestedCount = 0;
            return GetExpressionLength(input, startIndex, '"', '"', false, ref nestedCount, out length);
        }
        
        internal static HttpParseResult GetQuotedPairLength(string input, int startIndex, out int length)
        {
            Contract.Requires(input != null);
            Contract.Requires((startIndex >= 0) && (startIndex < input.Length));
            Contract.Ensures((Contract.ValueAtReturn(out length) >= 0) &&
                (Contract.ValueAtReturn(out length) <= (input.Length - startIndex)));

            length = 0;

            if (input[startIndex] != '\\')
            {
                return HttpParseResult.NotParsed;
            }
            
            if ((startIndex + 2 > input.Length) || (input[startIndex + 1] > 127))
            {
                return HttpParseResult.InvalidFormat;
            }
            
            length = 2;
            return HttpParseResult.Parsed;
        }

        private static HttpParseResult GetExpressionLength(
            string input,
            int startIndex,
            char openChar,
            char closeChar,
            bool supportsNesting,
            ref int nestedCount,
            out int length)
        {
            Contract.Requires(input != null);
            Contract.Requires((startIndex >= 0) && (startIndex < input.Length));
            Contract.Ensures((Contract.Result<HttpParseResult>() != HttpParseResult.Parsed) ||
                (Contract.ValueAtReturn<int>(out length) > 0));

            length = 0;

            if (input[startIndex] != openChar)
            {
                return HttpParseResult.NotParsed;
            }

            var current = startIndex + 1;
            while (current < input.Length)
            {
                if ((current + 2 < input.Length) &&
                    (GetQuotedPairLength(input, current, out var quotedPairLength) == HttpParseResult.Parsed))
                {
                    current = current + quotedPairLength;
                    continue;
                }
                
                if (supportsNesting && (input[current] == openChar))
                {
                    nestedCount++;
                    try
                    {
                        if (nestedCount > MaxNestedCount)
                        {
                            return HttpParseResult.InvalidFormat;
                        }

                        var nestedResult = GetExpressionLength(
                            input,
                            current,
                            openChar,
                            closeChar,
                            supportsNesting,
                            ref nestedCount,
                            out var nestedLength);

                        switch (nestedResult)
                        {
                            case HttpParseResult.Parsed:
                                current += nestedLength; 
                                break;

                            case HttpParseResult.NotParsed:
                                Contract.Assert(false, "'NotParsed' is unexpected: We started nested expression " +
                                    "parsing, because we found the open-char. So either it's a valid nested " +
                                    "expression or it has invalid format.");
                                break;

                            case HttpParseResult.InvalidFormat:
                                return HttpParseResult.InvalidFormat;

                            default:
                                Contract.Assert(false, "Unknown enum result: " + nestedResult);
                                break;
                        }
                    }
                    finally
                    {
                        nestedCount--;
                    }
                }

                if (input[current] == closeChar)
                {
                    length = current - startIndex + 1;
                    return HttpParseResult.Parsed;
                }
                current++;
            }
            
            return HttpParseResult.InvalidFormat;
        }
    }
}