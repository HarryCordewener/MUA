//-----------------------------------------------------------------------
// <copyright file="Parser.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     The Parser takes a String and evaluates it according to the parsing rules.
    /// </summary>
    public class Parser
    {
        /// <summary>
        ///     Detects the start of a function call.
        /// </summary>
        private 
            readonly Regex functionNameAscii = new Regex(@"^((?<FunctionName>[a-z](?:\d|\w|_)+))\((.*$)");

        /// <summary>
        ///     Looks for special characters, while not looking for a specific end condition.
        /// </summary>
        private readonly Regex specialChar = new Regex(@"^(.*?)(?<SpecialChar>[\\\[{%])(.*$)");

        /// <summary>
        ///     An alternate version of handling the functionPlus cases. To be implemented later.
        /// </summary>
        /// <remarks>
        ///     Based on my Stack Overflow Question here: http://StackOverflow.com/questions/22745729
        /// </remarks>
        private Regex catchRegex = new Regex(@"^fun\(((?:[^()\\]|\\.|(?<o>\()|(?<-o>\)))+(?(o)(?!)))([\),])(.*$)");

        /// <summary>
        ///     Detects additional arguments to a function.
        /// </summary>
        private Regex functionPlus = new Regex(@"^(.*?(?<!\\)(?:\\\\)*)(?<SpecialChar>[,\)])(.*$)");

        /// <summary>
        ///     This is the opposite of functionPlus. It detects when a bracket opens.
        /// </summary>
        private Regex functionPlusStart = new Regex(@"^(.*?(?<!\\)(?:\\\\)*)(?<SpecialChar>[\(])(.*$)");

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
        ///     Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        public Parser()
        {
            var tester =
                new StringBuilder(
                    @"function(innerfunction(),innerfunction2(),arg3()) \[noeval()\] noeval2() [eval()]");
            tester.EnsureCapacity(16384);
            this.Parse(0, ref tester, ref this.specialChar);
            Console.WriteLine("Result:" + tester);
            Console.Read();
        }

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
        /// <param name="startPosition">Starting position of the full 'myString' ref where we begin evaluation.</param>
        /// <param name="myString">The string reference to edit as we run through the parse-sequence.</param>
        /// <param name="haltExpression">Where we halt and return to our previous caller.</param>
        /// <param name="closure">
        ///     Forcefully add a closure for security purposes. This defines the required closing character of
        ///     the 'haltExpression'.
        /// </param>
        /// <returns>Returns the integer representation of how many characters we've 'changed' of the 'myString'.</returns>
        private int Parse(int startPosition, ref StringBuilder myString, ref Regex haltExpression, char closure = '\0')
        {
            int readerPosition = startPosition;
            string parseString = myString.ToString().Remove(0, startPosition);
            GroupCollection function = this.functionNameAscii.Match(parseString).Groups;

            if (!(closure == '\0' || parseString.Contains(closure)))
            {
                // Warning!!! May fail when we have a \ at the end of mystring?
                myString.Append(closure);
                parseString = parseString.Insert(parseString.Length, closure.ToString(CultureInfo.InvariantCulture));
            }

            if (this.functionNameAscii.IsMatch(parseString))
            {
                readerPosition += this.FunctionParse(readerPosition, ref myString, function[1].ToString());
                parseString = myString.ToString().Remove(0, readerPosition);
            }

            while (haltExpression.IsMatch(parseString))
            {
                Match specialMatch = haltExpression.Match(parseString);
                readerPosition += specialMatch.Groups[1].Length;
                parseString = myString.ToString().Remove(0, readerPosition);

                int len;

                // PLEASE TEST THIS TEMPORARY SOLUTION LATER!
                if (readerPosition == myString.Length)
                {
                    return readerPosition - startPosition;
                }

                switch (parseString[0])
                {
                    case '\\':
                        len = this.EscapeParse(readerPosition, ref myString);
                        break;
                    case '[':
                        len = this.SquareParse(readerPosition, ref myString);
                        break;
                    case '%':
                        len = this.PercentParse(readerPosition, ref myString);
                        break;
                    case '{':
                        len = this.CurlyParse(readerPosition, ref myString);
                        break;
                    default:
                        // We leave the string be! We hit a halting condition.
                        return readerPosition - startPosition;
                }

                readerPosition += len;
                parseString = myString.ToString().Remove(0, readerPosition);
            }

            return readerPosition - startPosition;
        }

        /// <summary>
        ///     A version of Parse that does not evaluate its contents.
        /// </summary>
        /// <param name="startPosition">Starting position of the full 'myString' ref where we begin evaluation.</param>
        /// <param name="myString">The string reference to edit as we run through the parse-sequence.</param>
        /// <param name="haltExpression">Where we halt and return to our previous caller.</param>
        /// <param name="startExpression">Ensuring correctness of the halt-condition.</param>
        /// <param name="closure">
        ///     Forcefully add a closure for security purposes. This defines the required closing character of
        ///     the 'haltExpression'.
        /// </param>
        /// <returns>Returns the integer representation of how many characters we've 'changed' of the 'myString'.</returns>
        private int NoEvalParse(int startPosition, ref StringBuilder myString, ref Regex haltExpression, ref Regex startExpression, char closure = '\0')
        {
            int readerPosition = startPosition;
            var parseString = new StringBuilder(myString.ToString().Remove(0, startPosition));
            int searchQty = 1;

            while (searchQty > 0)
            {
                if (!haltExpression.IsMatch(parseString.ToString()))
                {
                    // We're missing )s. So we are just going to go straight to the end and call it quits.
                    myString.Append(closure);
                    return myString.Length - startPosition - 1;
                }

                // We are ensured to have at least 1 ) now. So the following regexp must match unless there is a \ at the end.
                string caught = haltExpression.Match(parseString.ToString()).Groups[1].Value;

                if (startExpression.IsMatch(caught))
                {
                    // We found a ( before the ), so we must find an extra )!
                    int matchLen = startExpression.Match(caught).Groups[1].Length + 1;

                    readerPosition += matchLen;
                    searchQty++; // Add an extra one.
                    parseString.Remove(0, matchLen);
                }
                else
                {
                    readerPosition += caught.Length;
                    searchQty--;
                    parseString.Remove(0, caught.Length);
                    if (searchQty != 0)
                    {
                        readerPosition++;
                        parseString.Remove(0, 1);
                    }
                }
            }

            return readerPosition - startPosition;
        }

        /// <summary>
        ///     The parse-handler for [...] expressions found. This is used to initiate a new parse-run and allow a function call
        ///     further down a string.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'myString'.</param>
        /// <param name="myString">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int SquareParse(int readerPosition, ref StringBuilder myString)
        {
            // Does this not dangerously assume that there is stuff after this? Or will parse insert the closure?
            // This needs to be checked.
            int startPosition = readerPosition;
            int parseLength = this.Parse(++readerPosition, ref myString, ref this.specialCharSquareEndHalt, ']');
            myString.Remove(startPosition, 1); // [
            myString.Remove(startPosition + parseLength, 1); // ]
            return parseLength;
        }

        /// <summary>
        ///     The parse-handler for {...} expressions found. This is used to initiate a new parse-run and ignore commas.
        ///     <remarks>This may need edits to not allow a new Function Call!</remarks>
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'myString'.</param>
        /// <param name="myString">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int CurlyParse(int readerPosition, ref StringBuilder myString)
        {
            int startPosition = readerPosition;
            int parseLength = this.Parse(++readerPosition, ref myString, ref this.specialCharCurlyEndHalt, '}');
            myString.Remove(startPosition, 1); // {
            myString.Remove(startPosition + parseLength, 1); // }
            return parseLength;
        }

        /// <summary>
        ///     The parse-handler for \. expressions found. This is to handle escape sequences.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'myString'.</param>
        /// <param name="myString">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int EscapeParse(int readerPosition, ref StringBuilder myString)
        {
            myString.Remove(readerPosition, 1);
            return 1;
        }

        /// <summary>
        ///     The parse-handler for %. and &lt;...&gt; expressions found. This is to handle percent-expression sequences.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'myString'.</param>
        /// <param name="myString">The string we are evaluating.</param>
        /// <returns>
        ///     How many characters were added/changed forwards from the starting position. Aka, how much should the next call
        ///     skip.
        /// </returns>
        private int PercentParse(int readerPosition, ref StringBuilder myString)
        {
            int startPosition = readerPosition;
            int percentLen;
            switch (myString[++readerPosition])
            {
                case '<':
                    // We may want to make a <...> parse function, for the sake of both FunctionTuples and PercentParse.
                    percentLen = this.Parse(readerPosition, ref myString, ref this.specialCharLtEndHalt, '>') + 1;
                    break;
                default:
                    percentLen = 1;
                    break;
            }

            string percentarg = myString.ToString().Substring(startPosition, percentLen);

            myString.Remove(startPosition, 1); // %
            myString.Remove(startPosition, percentLen); // ...> or 'a character after %'
            throw new NotImplementedException();
        }

        /// <summary>
        ///     The parse-handler for Functions. This is to handle function(...,...) calls as well as the tuple calls.
        /// </summary>
        /// <param name="readerPosition">Where we start reading in the 'myString'.</param>
        /// <param name="myString">The string we are evaluating.</param>
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
        private int FunctionParse(int readerPosition, ref StringBuilder myString, string functionName)
        {
            Console.WriteLine("<Function: " + functionName + ">");
            int initialPosition = readerPosition;
            var functionStack = new Queue<string>();

            readerPosition += functionName.Length; // Function name and opening bracket.
            do
            {
                // We should not be changing mystring until we get to the end of functionparse!!!
                // We need to advance it to 'start' reading the argument one character further.
                // As readerPosition is ',' or ')' or '(' guaranteed at this moment.
                int arglength = this.NoEvalParse(++readerPosition, ref myString, ref this.functionPlus, ref this.functionPlusStart, ')');
                functionStack.Enqueue(myString.ToString().Substring(readerPosition, arglength));
                readerPosition += arglength;
            } 
            while (myString[readerPosition] != ')');

            myString.Remove(readerPosition, 1); // ')'

            int i = 0;
            foreach (string argument in functionStack)
            {
                Console.WriteLine("Argument " + i + " for " + functionName + ": " + argument);
                i++;
            }

            string functionoutput = "This was once function " + functionName + "!";
            myString.Remove(initialPosition, readerPosition - initialPosition);
            myString.Insert(initialPosition, functionoutput);

            return functionoutput.Length;
        }
    }
}