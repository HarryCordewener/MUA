﻿//-----------------------------------------------------------------------
// <copyright file="MarkupString.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Text;

namespace MUA
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The MarkupString class, containing Markup and the lot. Awaiting implementation.
    /// </summary>
    public class MarkupString
    {
        /// <summary>
        /// The flat string contained within the leaf of a Rope. null or "" otherwise.
        /// </summary>
        private StringBuilder markupString;

        /// <summary>
        /// Contains the markup for the elements below. It inherits down. The Markup below us is 'mixed' and overrides.
        /// We leave that to the Markup class.
        /// </summary>
        public Markup MyMarkup { get; set; }

        /// <summary>
        /// How long is the string beneath me? (How many character elements)
        /// We make use of this to rapidly find positions.
        /// </summary>
        private readonly List<int> stringWeight;

        /// <summary>
        /// The MarkupString on the left. Null if none exists.
        /// </summary>
        private readonly List<MarkupString> beneathList;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupString"/> class. 
        /// This call is most likely to be used if it is the 'root' of the Rope. Or if you are concatenating two MarkupStrings.
        /// </summary>
        public MarkupString()
        {
            beneathList = new List<MarkupString>();
            stringWeight = new List<int>();
            MyMarkup = new Markup();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupString"/> class.
        /// This is used for a Leaf node.
        /// As such, we do not initialize beneathList, stringWeight, nor markup.
        /// </summary>
        /// <param name="value">The leaf's string value.</param>
        public MarkupString(string value)
        {
            markupString = new StringBuilder(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupString"/> class.
        /// This is used for creating a Markup node.
        /// </summary>
        /// <param name="mup">The Markup to use the left side of this MarkupString Node.</param>
        public MarkupString(Markup mup)
        {
            MyMarkup = mup;
            beneathList = new List<MarkupString>();
            stringWeight = new List<int>();
        }

        /// <summary>       
        /// This function returns the location of the MarkupString after concatenating two existing ones.
        /// <remarks>It is up to another function to link this result in properly.</remarks>
        /// </summary>
        /// <param name="left">MarkupString on the left.</param>
        /// <param name="right">MarkupString on the right.</param>
        /// <returns>A MarkupString that simply has concatenated the two MarkupStrings.</returns>
        public MarkupString Concat(ref MarkupString left, ref MarkupString right)
        {
            var newMarkupString = new MarkupString();

            newMarkupString.beneathList.Add(left);
            newMarkupString.stringWeight.Add(left.Weight());

            newMarkupString.beneathList.Add(right);
            newMarkupString.stringWeight.Add(right.Weight());

            return newMarkupString;
        }

        /// <summary>
        /// Evaluates the weight of the node, by checking either the length of the string, or the items beneath it.
        /// </summary>
        /// <returns>An integer representation of how many characters are beneath this node.</returns>
        private int Weight()
        {
            return stringWeight == null ?
                    markupString.Length :
                    stringWeight.Sum();
        }


        /// <summary>
        /// The InsertString will put iString into the position in the MarkupString structure.
        /// </summary>
        /// <remarks>
        ///       5                         5                    6             String Weights
        /// [  A   B   C   D  E  ] [  F  G  H  I  J  ] [  K  L  M  N  O  P  ]  String Arrays
        ///    0   1   2   3  4       5  6  7  8  9      10 11 12 13 14 15     String Positions
        ///  0  1   2   3   4  5    5  6  7  8  9  10   10 11 12 13 14 15 16   Insert Positions
        ///
        /// Insert at position: 12
        ///    12 - 5 = 7                 7 - 5 = 2            2 - 6 = -4 (target is here)         
        ///
        /// Insert at position: 5
        ///    5 - 5 = 0
        /// 
        /// What is the standard for handling this case? OPTIONS:
        /// (O) Should it create a seperate String/Markup unit in between the two. This would be like 'appending' or 'prepending'
        /// (X) Should it append directly /into/ the end of the first string? (Inheriting a Markup rule)
        /// (X) Should it prepend direction /into/ the beginning of the next string. (Inhereting a Markup rule)
        /// </remarks>
        /// <param name="position">The position into which to insert. See remarks for insertion logic.</param>
        /// <param name="iString">The string to insert.</param>
        /// <returns>The markupstring itself.</returns>
        public MarkupString InsertString(int position, string iString)
        {

            if (!IsString())
            {
                var targetPosition = position;
                // We need to find the way this string is now 'split', and leave the 'remainder' up to another call of InsertString.
                var passedWeights = stringWeight.TakeWhile(val => (targetPosition -= val) > 0).Count();

                if (targetPosition == 0)
                {
                    // We must place it 'between', at the beginning, or at the 'end' of a beneathList.
                    // We know how many elements in we should be... so:
                    // If count is at 0, it's an insert.
                    // If count is equal to stringWeight.Count, append at end.
                    // Otherwise, pass the new targetPosition to insert into the String.
                    beneathList.Insert(passedWeights, new MarkupString(iString));
                    stringWeight.Insert(passedWeights, iString.Length);
                }
                
                // Implied else.
                beneathList[passedWeights].InsertString(targetPosition, iString);
                stringWeight[passedWeights] += iString.Length;
            }
            else
            {
                //  0 1 2 3 4 5 6
                // 0 1 2 3 4 5 6 7
                // var a = "0123456";
                // a = a.Insert(3, "-");
                // Console.WriteLine(a);
                // >> +012-3456
                markupString = markupString.Insert(position, iString);
            }

            return this;
        }

        /// <summary>
        /// Deletes a string and potential markup from the MarkupString, based on character position and length.
        /// </summary>
        /// <param name="location">The array-location for the first character to remove from the current MarkupString Node.</param>
        /// <param name="length">The amount of characters to delete, starting at the first character in this MarkupString Node.</param>
        /// <returns>Itself.</returns>
        public MarkupString DeleteString(int location, int length)
        {
            if (IsString())
            {
                markupString.Remove(location, length);
            }
            else
            {
                foreach (var markupStringItem in beneathList)
                {
                    // We do this, because keeping a count will get corrupted by deletions.
                    var index = beneathList.IndexOf(markupStringItem);

                    // We're done if we have nothing else to delete.
                    if (length <= 0) break;

                    // If the weight is less than the location, or equal to, this is not where we want to delete.
                    // So reduce the relative-location and try again.
                    if (markupStringItem.Weight() <= location)
                    {
                        location -= markupStringItem.Weight();
                        continue;
                    }

                    var maxCut = markupStringItem.Weight() - location;
                    var curCut = (maxCut < length ? maxCut : length);

                    markupStringItem.DeleteString(location, curCut);
                    // Should this be done with a evalWeight() function or similar?
                    stringWeight[index] = markupStringItem.Weight();
                    length -= curCut;
                    location = 0;

                    if (markupStringItem.Weight() != 0) continue;

                    // If this item is empty now, delete it. We no longer need it.
                    beneathList.RemoveAt(index);
                    stringWeight.RemoveAt(index);
                }
            }
            return this;
        }

        /// <summary>
        /// A simple function that returns whether or not this is a String node. The inverse is this being a Markup node.
        /// </summary>
        /// <returns>Whether this is a string or not.</returns>
        private bool IsString()
        {
            return markupString != null;
        }
    }
}
