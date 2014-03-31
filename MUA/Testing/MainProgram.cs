//-----------------------------------------------------------------------
// <copyright file="MainProgram.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

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
        ///     The main program runtime.
        /// </summary>
        /// <returns>Nothing is returned.</returns>
        public static int Main()
        {
            // p used for testUnit.
            var p = new Parser();
            Console.WriteLine(p.ToString());

            List<string> testcases = new List<string>();
            string testcase0 = @"fun(abc) fun3()";
            string testcase1 = @"fun(\\\)\)) fun3()";
            string testcase2 = @"fun(fun2(\)\\\(( fun3())";
            string testcase3 = @"fun(fun2(() fun3()))";
            testcases.Add(testcase0);
            testcases.Add(testcase1);
            testcases.Add(testcase2);
            testcases.Add(testcase3);

            Regex catchRegex = new Regex(@"^fun\(((?:[^()\\]|\\.|(?<o>\()|(?<-o>\)))+(?(o)(?!)))[\),](.*$)");

            foreach (var testcase in testcases)
            {
                Console.WriteLine(catchRegex.Match(testcase).Groups[1]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[2]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[3]);
                Console.WriteLine(catchRegex.Match(testcase).Groups[4]); 
                Console.WriteLine("---");
            }
            
            Console.Read();

            const MarkupRule Mar1 = MarkupRule.HiLight;
            var mar1L = new HashSet<MarkupRule> { Mar1 };
            var root = new MarkupString();
            var test = new MarkupString(new Markup(mar1L));
            test.Insert("DOOD", 0);
            root.Insert("Omnomnom", 0);
            root.Insert(test, 4);
            Console.WriteLine(root.ToString());
            root.Remove(3, 4);
            Console.WriteLine(root.ToString());

            var test2 = new List<MarkupString>();
            root.FlattenInto(ref test2);

            var sb2 = new StringBuilder();

            foreach (MarkupString each in test2)
            {
                sb2.Append(each.ToTestString());
            }

            Console.WriteLine(sb2.ToString());
            Console.Read();
            Console.Read();

            var root2 = new MarkupString(root, 2, 4);
            Console.WriteLine(root2.ToString());
            root2.Insert("Graaaa", 2);
            root2.Replace(new MarkupString("IttyBittyKittyCommitty"), 3, 4);
            root2.Replace(root, 4, 2);
            test2.Clear();

            Console.WriteLine("---------");
            Console.WriteLine(root2.ToTestString());
            Console.WriteLine("---------");

            root2.FlattenInto(ref test2);

            sb2.Clear();

            foreach (MarkupString each in test2)
            { 
                sb2.Append(each.ToTestString());
            }

            Console.WriteLine("---------");
            Console.WriteLine(sb2.ToString());
            Console.WriteLine("---------");

            List<MarkupString> splitlist = root2.Split("itty");

            foreach (MarkupString word in splitlist)
            {
                Console.WriteLine(word.ToTestString());
            }

            Console.Read();
            Console.Read();

            return 0;
        }
    }
}