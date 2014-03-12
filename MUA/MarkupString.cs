//-----------------------------------------------------------------------
// <copyright file="MarkupString.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace MUA
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The MarkupString class, containing Markup and the lot. Awaiting implementation.
    /// It must be noted that the MarkupString class can be in either or two states:
    /// 1) StringNode: The leaf of the structure, defining the contents.
    /// 2) MarkupNode: The root nodes of the structure, defining the markup of the underlying nodes and leaves.
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
        private List<int> stringWeight;

        /// <summary>
        /// The MarkupString on the left. Null if none exists.
        /// </summary>
        private List<MarkupString> beneathList;

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
            MyMarkup = new Markup(mup);
            beneathList = new List<MarkupString>();
            stringWeight = new List<int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupString"/>, copied from the object given.
        /// </summary>
        /// <param name="copyFrom">The MarkupString node to assumed to be root - and copy from.</param>
        public MarkupString(MarkupString copyFrom)
        {
            copyFrom.CopyInto(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupString"/>, copied from the object given, based on
        /// position and length. This is also known as the Substring copy constructor.
        /// </summary>
        /// <param name="copyFrom">The MarkupString node to assumed to be root - and copy from.</param>
        /// <param name="position">What character position (0 based) to start the copy from.</param>
        /// <param name="length">How many characters to copy.</param>
        public MarkupString(MarkupString copyFrom, int position, int length)
        {
            copyFrom.CopySubstringInto(this, position, length);
        }

        /// <summary>       
        /// This function returns the position of the MarkupString after concatenating two existing ones.
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
                else
                {
                    // This is the adjustment for the 'last step reduction' error.
                    targetPosition += stringWeight[passedWeights];

                    beneathList[passedWeights].InsertString(targetPosition, iString);
                    stringWeight[passedWeights] += iString.Length;
                }
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
        /// Appends a MarkupString to the end of current instance of MarkupString.
        /// </summary>
        /// <param name="mString">The MarkupString being added to this instance.</param>
        /// <returns>Itself.</returns>
        public MarkupString Append(MarkupString mString)
        {
            beneathList.Add(mString);
            stringWeight.Add(mString.Weight());
            return this;
        }

        /// <summary>
        /// The InsertString will put mString into the position in the MarkupString structure.
        /// To do this, it may split up a string beneath it. After all, the node is expected to be Marked Up.
        /// </summary>
        /// <param name="position">The position into which to insert. See remarks for insertion logic.</param>
        /// <param name="mString">The MarkupString to insert.</param>
        /// <returns>The markupstring itself.</returns>
        public MarkupString InsertString(int position, MarkupString mString)
        {
            if (IsString())
            {
                beneathList = new List<MarkupString>();
                stringWeight = new List<int>();
                MyMarkup = new Markup(); // Blank Markup Transition

                var rightside = markupString.ToString().Substring(position);
                var leftside = markupString.ToString().Substring(0, markupString.Length - rightside.Length);

                beneathList.Insert(0, new MarkupString(leftside));
                beneathList.Insert(1, mString);
                beneathList.Insert(2, new MarkupString(rightside));

                markupString = null;

                stringWeight.Insert(0, beneathList[0].Weight());
                stringWeight.Insert(1, beneathList[1].Weight());
                stringWeight.Insert(2, beneathList[2].Weight());
            }
            else
            {
                var targetPosition = position;
                // We need to find the way this string is now 'split', and leave the 'remainder' up to another call of InsertString.
                var passedWeights = stringWeight.TakeWhile(val => (targetPosition -= val) >= 0).Count();
                // Warning: here, a position of 3 generates a targetPosition of -5 after noticing a weight 8. The position it needs to
                // go into is 3. But had it passed over a weight of 2, the targetposition would be '1' for the /next/ unit. Which is correct.
                // But if that had a weight of 4, it'd end up being 1-4 = -3. We need to re-add the last step, on which the evaluation failed.

                if (targetPosition == 0)
                {
                    // We must place it 'between', at the beginning, or at the 'end' of a beneathList.
                    // We know how many elements in we should be... so:
                    // If count is at 0, it's an insert.
                    // If count is equal to stringWeight.Count, append at end.
                    // Otherwise, pass the new targetPosition to insert into the String.
                    beneathList.Insert(passedWeights, mString);
                    stringWeight.Insert(passedWeights, mString.Weight());
                }
                else
                {
                    // This is the adjustment for the 'last step reduction' error.
                    targetPosition += stringWeight[passedWeights];

                    beneathList[passedWeights].InsertString(targetPosition, mString);
                    stringWeight[passedWeights] += mString.Weight();

                    if (!beneathList[passedWeights].MyMarkup.IsOnlyInherit()) return this;

                    // If the below is now an empty (OnlyInherit) Markup, we can 'pull it up'.
                    // That is to say, we can put its individual beneathLists in the position where we had our old one.
                    // If the item below is not an empty (OnlyInherit) Markup, then it wasn't the item below that we did
                    // the final insert into.
                    var reference = beneathList[passedWeights];
                    beneathList.RemoveAt(passedWeights);
                    stringWeight.RemoveAt(passedWeights);
                    beneathList.InsertRange(passedWeights, reference.beneathList);
                    stringWeight.InsertRange(passedWeights, reference.stringWeight);
                }
            }
            return this;
        }

        /// <summary>
        /// Deletes a string and potential markup from the MarkupString, based on character position (starting at 0) and length.
        /// </summary>
        /// <param name="position">The array-position for the first character to remove from the current MarkupString Node.</param>
        /// <param name="length">The amount of characters to delete, starting at the first character in this MarkupString Node.</param>
        /// <returns>Itself.</returns>
        public MarkupString DeleteString(int position, int length)
        {
            var deletionIndexes = new List<int>();
            if (IsString())
            {
                markupString.Remove(position, length);
            }
            else
            {
                // We can't do this in parallel. Must be done in-order.
                foreach (var markupStringItem in beneathList)
                {
                    // We do this, because keeping a count will get corrupted by deletions.
                    var index = beneathList.IndexOf(markupStringItem);

                    // We're done if we have nothing else to delete.
                    if (length <= 0) break;

                    // If the weight is less than the position, or equal to, this is not where we want to delete.
                    // So reduce the relative-position and try again.
                    if (markupStringItem.Weight() <= position)
                    {
                        position -= markupStringItem.Weight();
                        continue;
                    }

                    var maxCut = markupStringItem.Weight() - position;
                    var curCut = (maxCut < length ? maxCut : length);

                    markupStringItem.DeleteString(position, curCut);
                    // Should this be done with a evalWeight() function or similar?
                    stringWeight[index] = markupStringItem.Weight();
                    length -= curCut;
                    position = 0;

                    if (markupStringItem.Weight() != 0) continue;

                    // If this item is empty now, delete it. We no longer need it.
                    deletionIndexes.Add(index);
                }

                // We must reverse them. Because if we do it the wrong way around, we will be at the
                // wrong index after the first Removal.
                deletionIndexes.Reverse();

                // Begone! Evil empty nodes! You serve no purpose!
                foreach (var index in deletionIndexes)
                {
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

        /// <summary>
        /// Returns the String representation of the MarkupString. This visits all of its children.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            if (IsString()) return markupString.ToString();

            var result = new StringBuilder();
            result.Append("<" + MyMarkup + ">");

            foreach (MarkupString each in beneathList)
            {
                result.Append(each);
            }

            result.Append("</" + MyMarkup + ">");
            return result.ToString();
        }

        /// <summary>
        /// A very basic surface level ToString. This only evaluates the String representation of the current item, 
        /// and none of its children.
        /// </summary>
        /// <returns>A string representation of this MarkupString element only.</returns>
        public string ToSurfaceString()
        {
            if (IsString()) return markupString.ToString();
            return MyMarkup.ToString();
        }

        /// <summary>
        /// Implementation of flattening the MarkupString. This is meant to create an equivalent MarkupString List.
        /// <remarks>
        /// This should call Mix as we move down, and then when we hit text, create the new MarkupString representation
        /// of that string, with a parent that holds this new Mixed Markup.
        /// </remarks>
        /// </summary>
        /// <param name="markupStringList">A reference to a markupStringList that you wish to put things into.</param>
        public void FlattenInto(ref List<MarkupString> markupStringList)
        {
            var myMarkup = new Markup();
            FlattenInto(ref markupStringList, MyMarkup.Mix(myMarkup));
        }

        private void FlattenInto(ref List<MarkupString> markupStringList, Markup mup)
        {
            if (!IsString())
            {
                foreach (var each in beneathList)
                {
                    each.FlattenInto(ref markupStringList, MyMarkup.Mix(mup));
                }
            }
            else
            {
                var myMarkupParent = new MarkupString(mup);
                myMarkupParent.InsertString(0, markupString.ToString());
                markupStringList.Add(myMarkupParent);
            }
        }

        /// <summary>
        /// The work-horse for destructively copying a substring set of a MarkupString into the given MarkupString object.
        /// </summary>
        /// <param name="newMarkupString">The MarkupString object to copy into.</param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns>The newMarkupString, now filled with this MarkupString's information.</returns>
        public MarkupString CopySubstringInto(MarkupString newMarkupString, int position, int length)
        {
            var copyIndexes = new List<int>();
            if (IsString())
            {
                newMarkupString.MyMarkup = null;
                newMarkupString.beneathList = null;
                newMarkupString.stringWeight = null;
                newMarkupString.markupString = new StringBuilder(markupString.ToString().Substring(position, length));
            }
            else
            {
                newMarkupString.MyMarkup = new Markup(MyMarkup);
                newMarkupString.beneathList = new List<MarkupString>();
                newMarkupString.stringWeight = new List<int>();
                newMarkupString.markupString = null;

                // We can't do this in parallel. Must be done in-order.
                foreach (var markupStringItem in beneathList)
                {
                    // We're done if we have nothing else to add to the substring node.
                    if (length <= 0) break;

                    // If the weight is less than the position, or equal to, this is not where we want to copy from.
                    // So reduce the relative-position and try again.
                    if (markupStringItem.Weight() <= position)
                    {
                        position -= markupStringItem.Weight();
                        continue;
                    }

                    var maxCut = markupStringItem.Weight() - position;
                    var curCut = (maxCut < length ? maxCut : length);

                    var thisOne = new MarkupString();
                    markupStringItem.CopySubstringInto(thisOne,position, curCut);

                    newMarkupString.beneathList.Add(thisOne);
                    newMarkupString.stringWeight.Add(thisOne.Weight());

                    length -= curCut;
                    position = 0;
                }
            }
            return this;
        }

        /// <summary>
        /// Destructively Copies the whole MarkupString Structure into the given MarkupString Object.
        /// </summary>
        /// <param name="newMarkupString">The MarkupString object to copy into.</param>
        /// <returns>The newMarkupString, now filled with this MarkupString's information.</returns>
        public MarkupString CopyInto(MarkupString newMarkupString)
        {
            if (IsString())
            {
                newMarkupString.beneathList = null;
                newMarkupString.stringWeight = null;
                newMarkupString.MyMarkup = null;
                newMarkupString.markupString = new StringBuilder();
                newMarkupString.markupString.Append(markupString);
                return newMarkupString;
            }
            // Implied else.

            newMarkupString.beneathList = new List<MarkupString>();
            newMarkupString.stringWeight = new List<int>();
            newMarkupString.MyMarkup = new Markup(MyMarkup);
            foreach (var mySubMarkupString in beneathList)
            {
                newMarkupString.markupString = null;
                var thisOne = new MarkupString();
                newMarkupString.beneathList.Add(mySubMarkupString.CopyInto(thisOne));
                newMarkupString.stringWeight.Add(thisOne.Weight());
            }

            return newMarkupString;
        }
    }
}
