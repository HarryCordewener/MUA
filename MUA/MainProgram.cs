//-----------------------------------------------------------------------
// <copyright file="MainProgram.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace MUA
{
    using System;

    /// <summary>
    /// The Main Program.
    /// </summary>
    public class MainProgram
    {
        /// <summary>
        /// The main program runtime.
        /// </summary>
        /// <returns>Nothing is returned.</returns>
        public static int Main()
        {
            var p = new Parser();
            // p used for testUnit.

            Console.Read();

            const MarkupRule mar1 = MarkupRule.HiLight;
            var mar1L = new HashSet<MarkupRule> {mar1};
            var root = new MarkupString();
            var test = new MarkupString(new Markup(mar1L));
            test.InsertString(0,"DOOD");
            root.InsertString(0, "Omnomnom");
            root.InsertString(4, test);
            Console.WriteLine(root.ToString());
            root.DeleteString(3, 4);
            Console.WriteLine(root.ToString());

            var test2 = new List<MarkupString>();
            root.FlattenInto(ref test2);

            var sb2 = new StringBuilder();
            
            foreach (var each in test2)
            {
                sb2.Append(each);
            }
            Console.WriteLine(sb2.ToString());
            Console.Read();
            Console.Read();

            var root2 = new MarkupString(root,2,4);
            Console.WriteLine(root2.ToString());
            root2.InsertString(2, "Graaaa");
            test2.Clear();
            root2.FlattenInto(ref test2);

            sb2.Clear();

            foreach (var each in test2)
                sb2.Append(each);

            Console.WriteLine(sb2.ToString());

            Console.Read();
            Console.Read();

            return 0;
        }
    }
}
