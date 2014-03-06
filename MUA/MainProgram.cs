//-----------------------------------------------------------------------
// <copyright file="MainProgram.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;

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
            var mar1L = new List<MarkupRule> {mar1};
            var root = new MarkupString();
            var test = new MarkupString(new Markup(mar1L));
            test.InsertString(0,"DOOD");
            root.InsertString(0, "Omnomnom");
            root.InsertString(4, test);
            Console.WriteLine(root.ToString());

            Console.Read();
            Console.Read();

            return 0;
        }
    }
}
