//-----------------------------------------------------------------------
// <copyright file="Parser.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MUA
{
    /// <summary>
    ///     The Parser takes a String and evaluates it according to the parsing rules.
    /// </summary>
    public class Parser
    {
        /// <summary>
        ///     Detects the start of a function call.
        /// </summary>
        private readonly Regex functionNameAscii = new Regex(@"^((?<FunctionName>[a-z](?:\d|\w|_)+))\((.*$)");

        /// <summary>
        ///     Looks for special characters, while not looking for a specific end condition.
        /// </summary>
        private readonly Regex specialChar = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%])(.*$)");

        /// <summary>
        ///     Detects additional arguments to a function.
        /// </summary>
        /// <remarks>
        ///     This is likely unused and needs to go.
        /// </remarks>
        private Regex functionPlus = new Regex(@"^(.*(?<!\\)(?:\\\\)*)[,\)](.*$)");

        /// <summary>
        ///     Looks for special characters, while also looking for a '}' as an end condition.
        /// </summary>
        private Regex specialCharCurlyEndHalt = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%}])(.*$)");

        /// <summary>
        ///     Looks for special characters, while also looking for a ')' or ',' as an end condition.
        /// </summary>
        private Regex specialCharFunEndHalt = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%,\)])(.*$)");

        /// <summary>
        ///     Looks for special characters, while also looking for a '>' as an end condition.
        /// </summary>
        private Regex specialCharLtEndHalt = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%>])(.*$)");

        /// <summary>
        ///     Looks for special characters, while also looking for a ']' as an end condition.
        /// </summary>
        private Regex specialCharSquareEndHalt = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%\]])(.*$)");

        /// <summary>
        ///     Indicates what type of string we are matching. Command-List, and standard Strings have different evaluation rules.
        /// </summary>
        private enum MatchState
        {
            /// <summary>
            ///     Command List type matching.
            /// </summary>
            Commandlist,

            /// <summary>
            ///     String Type matching.
            /// </summary>
            String
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        public Parser()
        {
            var tester =
                new StringBuilder(
                    "testfunction2() testfunction3() \\[donotevalfun()] [testfunction(4)] The following is a function evaluation: >[testfunction(arg,arg2)]< and we are safe.");
            tester.EnsureCapacity(16384);
            Parse(0, ref tester, ref specialChar);
            Console.WriteLine("Result:" + tester);
            Console.Read();
        }

        /// <summary>
        ///     Parses the incoming string. Recursively called on itself to handle many things.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         We keep parsing until we either have reached the end of the StringBuilder, or reach the stop-character.
        ///         We _DO NOT_ eat the stop-character. As some items may need to check what the stop-character was.
        ///         They can eat if all if they want. And they (probably) should.
        ///     </para>
        ///     <para>The return value is the length of the string we have 'gone over', and is guaranteed to still 'exist'.</para>
        ///     <para>
        ///         WHAT IF THE LAST CHARACTER IS AN OPENING CHARACTER!?
        ///         PARSE SHOULD HANDLE THIS
        ///     </para>
        ///     <para>
        ///         String parseString should get altered to use StringBuilder for faster response time and less memory waste.
        ///         However, since Regex.Match doesn't LIKE using StringBuilder, and a ToString() would likely take ages...
        ///     </para>
        ///     <para>Needs to be changed to use MarkupString instead of String!</para>
        /// </remarks>
        /// <param name="startPosition">Starting position of the full 'mystring' ref where we begin evaluation.</param>
        /// <param name="mystring">The string reference to edit as we run through the parse-sequence.</param>
        /// <param name="haltexpression">Where we halt and return to our previous caller.</param>
        /// <param name="closure">
        ///     Forcefully add a closure for security purposes. This defines the required closing character of
        ///     the 'haltexpression'.
        /// </param>
        /// <param name="evaluate">
        ///     Whether or not we evaluate the string we parse.
        ///     <remarks>Not Yet Implemented.</remarks>
        /// </param>
        /// <returns>Returns the integer representation of how many characters we've 'changed' of the 'mystring'.</returns>
        private int Parse(int startPosition, ref StringBuilder mystring, ref Regex haltexpression, char closure = '\0',
            bool evaluate = true)
        {
            int readerPosition = startPosition;
            string parseString = mystring.ToString().Remove(0, startPosition);
            GroupCollection function = functionNameAscii.Match(parseString).Groups;

            if (!(closure == '\0' || parseString.Contains(closure)))
            {
                mystring.Append(closure);
                parseString = parseString.Insert(parseString.Length, closure.ToString(CultureInfo.InvariantCulture));
            }

            if (functionNameAscii.IsMatch(parseString))
            {
                readerPosition += FunctionParse(readerPosition, ref mystring, function[1].ToString());
                parseString = mystring.ToString().Remove(0, readerPosition);
            }

            while (haltexpression.IsMatch(parseString))
            {
                Match specialMatch = haltexpression.Match(parseString);
                readerPosition += specialMatch.Groups[1].Length;
                parseString = mystring.ToString().Remove(0, readerPosition);

                int len;

                // PLEASE TEST THIS TEMPORARY SOLUTION LATER!
                if (readerPosition == mystring.Length)
                {
                    return readerPosition - startPosition;
                }

                switch (parseString[0])
                {
                    case '\\':
                        len = EscapeParse(readerPosition, ref mystring);
                        break;
                    case '[':
                        len = SquareParse(readerPosition, ref mystring);
                        break;
                    case '%':
                        len = PercentParse(readerPosition, ref mystring);
                        break;
                    case '{':
                        len = CurlyParse(readerPosition, ref mystring);
                        break;
                    default:
                        // We leave the string be! We hit a halting condition.
                        return readerPosition - startPosition;
                }

                readerPosition += len;
                parseString = mystring.ToString().Remove(0, readerPosition);
            }

            return readerPosition - startPosition;
        }

        /// <summary>
        ///     The parse-handler for [...] expressions found. This is used to initiate a new parse-run and allow a function call
        ///     further down a string.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'mystring'.</param>
        /// <param name="mystring">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int SquareParse(int readerPosition, ref StringBuilder mystring)
        {
            // Does this not dangerously assume that there is stuff after this? Or will parse insert the closure?
            // This needs to be checked.
            int startPosition = readerPosition;
            int parseLength = Parse(++readerPosition, ref mystring, ref specialCharSquareEndHalt, ']');
            mystring.Remove(startPosition, 1); // [
            mystring.Remove(startPosition + parseLength, 1); // ]
            return parseLength;
        }

        /// <summary>
        ///     The parse-handler for {...} expressions found. This is used to initiate a new parse-run and ignore commas.
        ///     <remarks>This may need edits to not allow a new Function Call!</remarks>
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'mystring'.</param>
        /// <param name="mystring">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int CurlyParse(int readerPosition, ref StringBuilder mystring)
        {
            int startPosition = readerPosition;
            int parseLength = Parse(++readerPosition, ref mystring, ref specialCharCurlyEndHalt, '}');
            mystring.Remove(startPosition, 1); // {
            mystring.Remove(startPosition + parseLength, 1); // }
            return parseLength;
        }

        /// <summary>
        ///     The parse-handler for \. expressions found. This is to handle escape sequences.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'mystring'.</param>
        /// <param name="mystring">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int EscapeParse(int readerPosition, ref StringBuilder mystring)
        {
            mystring.Remove(readerPosition, 1);
            return 1;
        }

        /// <summary>
        ///     The parse-handler for %. and &lt;...&gt; expressions found. This is to handle percent-expression sequences.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'mystring'.</param>
        /// <param name="mystring">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int PercentParse(int readerPosition, ref StringBuilder mystring)
        {
            int startPosition = readerPosition;
            int percentLen;
            switch (mystring[++readerPosition])
            {
                case '<':
                    // We may want to make a <...> parse function, for the sake of both FunctionTuples and PercentParse.
                    percentLen = Parse(readerPosition, ref mystring, ref specialCharLtEndHalt, '>') + 1;
                    break;
                default:
                    percentLen = 1;
                    break;
            }

            string percentarg = mystring.ToString().Substring(startPosition, percentLen);

            mystring.Remove(startPosition, 1); // %
            mystring.Remove(startPosition, percentLen); // ...> or 'a character after %'
            throw new NotImplementedException();
        }

        /// <summary>
        ///     The parse-handler for Functions. This is to handle function(...,...) calls as well as the tuple calls.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'mystring'.</param>
        /// <param name="mystring">The string we are evaluating.</param>
        /// <param name="functionName">The string that expresses the function call's name.</param>
        /// <remarks>
        ///     Seeing as we have cases like the 'if()' function, or let() function, where not all arguments are evaluated,
        ///     We should _CHANGE_ the Parse() call in do{} to make it not evaluate. Then from there, when we have a 'Function
        ///     Signature', we will actually parse it.
        /// </remarks>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int FunctionParse(int readerPosition, ref StringBuilder mystring, string functionName)
        {
            Console.WriteLine("<Function: " + functionName + ">");
            int initialPosition = readerPosition;
            var functionStack = new Stack<string>();

            readerPosition += functionName.Length; // Function name and opening bracket.
            do
            {
                // We should not be changing mystring until we get to the end of functionparse!!!
                // We need to advance it to 'start' reading the argument one character further.
                // As readerPosition is ',' or ')' or '(' guaranteed at this moment.
                int arglength = Parse(++readerPosition, ref mystring, ref specialCharFunEndHalt, ')', false);
                functionStack.Push(mystring.ToString().Substring(readerPosition, arglength));
                readerPosition += arglength;
            } while (mystring[readerPosition] != ')');

            mystring.Remove(readerPosition, 1); // ')'

            foreach (string argument in functionStack)
            {
                Console.WriteLine("Argument: " + argument);
            }

            string functionoutput = "This was once function " + functionName + "!";
            mystring.Remove(initialPosition, readerPosition - initialPosition);
            mystring.Insert(initialPosition, functionoutput);

            return functionoutput.Length;
        }
    }
}