//-----------------------------------------------------------------------
// <copyright file="MainProgram.cs" company="Twilight Days">
// ...
// </copyright>
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

            var myList = new List<StringBuilder>();
            myList.Add(new StringBuilder("ABCDEF"));
            myList.Add(new StringBuilder("GHIJKLMNOP"));
            myList.Add(new StringBuilder("QRSTUVWXYZ"));

            var argPos = 6;
            var argLen = 99;

            foreach (var item in myList)
            {
                if (argLen <= 0)
                    break;

                if (item.Length > argPos) // We are now guaranteed the position is in here.
                {
                    var maxCut = item.Length - argPos;
                    var curCut = (maxCut < argLen ? maxCut : argLen);
                    item.Remove(argPos, curCut);
                    argLen -= curCut;
                    argPos = 0;
                }
                else
                {
                    argPos -= item.Length;
                }
            }

            foreach (var item in myList)
            {
                Console.WriteLine(item);
            }

            Console.Read();

            return 0;
        }
    }
}
