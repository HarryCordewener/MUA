//-----------------------------------------------------------------------
// <copyright file="MainProgram.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Linq;
using MUA.Server.TCP;
using MUA.Server.TCP.Telnet;

namespace MUA
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     The Main Program.
    /// </summary>
    public class MainProgram
    {

        /// <summary>
        /// StringTest tests strings. To be converted into a UnitTest later.
        /// </summary>
        public static void StringTest()
        {
            Console.Read();

            const MarkupRule hiLite = MarkupRule.HiLight;
            var hiLiteAsList = new HashSet<MarkupRule> { hiLite };
            var testString1 = new MarkupString(new Markup(hiLiteAsList));
            var testString2 = new MarkupString();

            /* ----------------------------------
             *  Testing Ansi Insert and Remove
             * ---------------------------------- */
            Console.WriteLine("Inserting DOOD into HiLite markup string 1 at position 0.");
            testString1.Insert("DOOD", 0);
            Console.WriteLine("Inserting Omnomnom into markup string 2 at position 0.");
            testString2.Insert("Omnomnom", 0);
            Console.WriteLine("Inserting markup string 1 into markup string 2 at position 4.");
            testString2.Insert(testString1, 4);
            Console.WriteLine("Printing markup string 2: ");
            Console.WriteLine(testString2.ToString());
            Console.WriteLine("Removing 4 characters starting at position 3 from markup string 2.");
            testString2.Remove(3, 4);
            Console.WriteLine("Printing markup string 2: ");
            Console.WriteLine(testString2.ToString());

            /* ---------------------------------- 
             *  Testing Ansi Flattening
             * ---------------------------------- */
            Console.WriteLine("Ansi Flattening Tests");
            var testString3 = new List<MarkupString>();
            testString2.FlattenInto(ref testString3);

            Console.WriteLine("Flattening string 2 into string 3, and printing.");
            Console.WriteLine(testString2.ToString());

            var sb2 = new StringBuilder();
            foreach (MarkupString each in testString3)
            {
                sb2.Append(each.ToTestString());
            }

            Console.WriteLine(sb2.ToString());
            Console.WriteLine("\n\n\n");
            Console.ReadLine();


            Console.WriteLine("Creating string 4 from string 2 (" + testString2 + "), starting at position 2, length 4.");
            var testString4 = new MarkupString(testString2, 2, 4);
            Console.WriteLine(testString4.ToString());

            Console.WriteLine("\nInserting 'Graaaa' into string 4 at position 2.");
            testString4.Insert("Graaaa", 2);

            Console.WriteLine("Printing test string 4");
            Console.WriteLine(testString4);

            Console.WriteLine("Replacing string 4 at position 3 for length 4 with 'IttyBittyKittyCommitty'");
            testString4.Replace(new MarkupString("IttyBittyKittyCommitty"), 3, 4);
            Console.WriteLine("Printing test string 4");
            Console.WriteLine(testString4);

            Console.WriteLine("Replacing string 4 at position 4 for length 2 with string 2 (" + testString2 + ")");
            testString4.Replace(testString2, 4, 2);
            Console.WriteLine("Printing test string 4");
            Console.WriteLine(testString4);
            Console.ReadLine();
        }

        /// <summary>
        ///     The main program runtime.
        /// </summary>
        /// <returns>Nothing is returned.</returns>
        public static int Main()
        {
            // p used for testUnit.
            var p = new Parser();
            Console.WriteLine(p.ToString());

            List<string> testcases = new List<string>();
            const string testcase0 = @"fun(abc) fun3()";
            const string testcase1 = @"fun(\\\)\)) fun3()";
            const string testcase2 = @"fun(fun2(\)\\\(( fun3())";
            const string testcase3 = @"fun(fun2(() fun3()))";
            const string testcase4 = @"fun(fun2(() fun3()))\";
            testcases.Add(testcase0);
            testcases.Add(testcase1);
            testcases.Add(testcase2);
            testcases.Add(testcase3);
            testcases.Add(testcase4);

            Regex catchRegex = new Regex(@"^fun\(((?:[^()\\]|\\.|(?<o>\()|(?<-o>\)))+(?(o)(?!)))[\),](.*$)");

            foreach (var testcase in testcases)
            {
                Console.WriteLine(catchRegex.Match(testcase).Groups[1]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[2]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[3]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[4]); 
                Console.WriteLine("---");
            }

            // Console.Read();
            Console.WriteLine("---");

            TCPInitializer tcp = new TCPInitializer();
            tcp.Serve();

            return 0;
        }
    }
}