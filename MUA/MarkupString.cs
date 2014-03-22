//-----------------------------------------------------------------------
// <copyright file="MarkupString.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     The MarkupString class, containing Markup and the lot. Awaiting implementation.
    ///     It must be noted that the MarkupString class can be in either or two states:
    ///     1) String Node: The leaf of the structure, defining the contents.
    ///     2) Markup Node: The root nodes of the structure, defining the markup of the underlying nodes and leaves.
    /// </summary>
    public partial class MarkupString
    {
        /// <summary>
        ///     The MarkupStrings beneath this Markup Node. Null if this is not a Markup Node.
        /// </summary>
        private List<MarkupString> beneathList;

        /// <summary>
        ///     The flat string contained within the leaf of a MarkupString, making this a String Node. null otherwise.
        /// </summary>
        private StringBuilder markupString;

        /// <summary>
        ///     Returns the total length of the strings beneath this node.
        ///     <remarks>
        ///         We internally make use of this to rapidly find positions.
        ///     </remarks>
        /// </summary>
        private List<int> stringWeight;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkupString" /> class.
        /// </summary>
        public MarkupString()
        {
            this.beneathList = new List<MarkupString>();
            this.stringWeight = new List<int>();
            this.MyMarkup = new Markup();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkupString" /> class.
        ///     <remark>
        ///         This is used for a Leaf node.
        ///         As such, we do not initialize beneathList, stringWeight, nor markup.
        ///     </remark>
        /// </summary>
        /// <param name="value">The leaf's string value.</param>
        public MarkupString(string value)
        {
            this.markupString = new StringBuilder(value);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkupString" /> class.
        ///     <remark>
        ///         This is used for creating a Markup node.
        ///     </remark>
        /// </summary>
        /// <param name="mup">The Markup to use the left side of this MarkupString Node.</param>
        public MarkupString(Markup mup)
        {
            this.MyMarkup = new Markup(mup);
            this.beneathList = new List<MarkupString>();
            this.stringWeight = new List<int>();
        }

        /// <summary>
        ///     Gets or sets the markup for the elements below. 
        ///     It inherits down. The Markup below us is 'mixed' and overrides.
        ///     We leave that to the Markup class.
        /// </summary>
        public Markup MyMarkup { get; set; }

        /// <summary>
        ///     This function returns the position of the MarkupString after concatenating two existing ones.
        ///     <remarks>It is up to another function to link this result in properly.</remarks>
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
        ///     The InsertString will put iString into the position in the MarkupString structure.
        /// </summary>
        /// <remarks>
        ///     5                         5                    6             String Weights
        ///     [  A   B   C   D  E  ] [  F  G  H  I  J  ] [  K  L  M  N  O  P  ]  String Arrays
        ///     0   1   2   3  4       5  6  7  8  9      10 11 12 13 14 15     String Positions
        ///     0  1   2   3   4  5    5  6  7  8  9  10   10 11 12 13 14 15 16   Insert Positions
        ///     Insert at position: 12
        ///     12 - 5 = 7                 7 - 5 = 2            2 - 6 = -4 (target is here)
        ///     Insert at position: 5
        ///     5 - 5 = 0
        ///     What is the standard for handling this case? OPTIONS:
        ///     (O) Should it create a separate String/Markup unit in between the two. This would be like 'appending' or
        ///     'prepending'
        ///     (X) Should it append directly /into/ the end of the first string? (Inheriting a Markup rule)
        ///     (X) Should it prepend direction /into/ the beginning of the next string. (Inheriting a Markup rule)
        /// </remarks>
        /// <param name="insertString">The string to insert.</param>
        /// <param name="position">The position into which to insert. See remarks for insertion logic.</param>
        /// <returns>The MarkupString itself.</returns>
        public MarkupString Insert(string insertString, int position)
        {
            if (!this.IsString())
            {
                int targetPosition = position;

                // We need to find the way this string is now 'split', and leave the 'remainder' up to another call of InsertString.
                int passedWeights = this.stringWeight.TakeWhile(val => (targetPosition -= val) > 0).Count();

                if (targetPosition == 0)
                {
                    // We must place it 'between', at the beginning, or at the 'end' of a beneathList.
                    // We know how many elements in we should be... so:
                    // If count is at 0, it's an insert.
                    // If count is equal to stringWeight.Count, append at end.
                    // Otherwise, pass the new targetPosition to insert into the String.
                    this.beneathList.Insert(passedWeights, new MarkupString(insertString));
                    this.stringWeight.Insert(passedWeights, insertString.Length);
                }
                else
                {
                    // This is the adjustment for the 'last step reduction' error.
                    targetPosition += this.stringWeight[passedWeights];

                    this.beneathList[passedWeights].Insert(insertString, targetPosition);
                    this.stringWeight[passedWeights] += insertString.Length;
                }
            }
            else
            {
                // >  0 1 2 3 4 5 6
                // > 0 1 2 3 4 5 6 7
                // var a = "0123456";
                // a = a.Insert(3, "-");
                // Console.WriteLine(a);
                // >> +012-3456
                this.markupString = this.markupString.Insert(position, insertString);
            }

            return this;
        }

        /// <summary>
        ///     The InsertString will put mString into the position in the MarkupString structure.
        ///     To do this, it may split up a string beneath it. After all, the node is expected to be Marked Up.
        /// </summary>
        /// <param name="markupStringArg">The MarkupString to insert. Please make sure to give it a Copy!</param>
        /// <param name="position">The position into which to insert. See remarks for insertion logic.</param>
        /// <returns>The MarkupString itself.</returns>
        public MarkupString Insert(MarkupString markupStringArg, int position)
        {
            if (this.IsString())
            {
                this.beneathList = new List<MarkupString>();
                this.stringWeight = new List<int>();
                this.MyMarkup = new Markup(); // Blank Markup Transition

                string rightside = this.markupString.ToString().Substring(position);
                string leftside = this.markupString.ToString().Substring(0, this.markupString.Length - rightside.Length);

                this.beneathList.Insert(0, new MarkupString(leftside));
                this.beneathList.Insert(1, markupStringArg);
                this.beneathList.Insert(2, new MarkupString(rightside));

                this.markupString = null;

                this.stringWeight.Insert(0, this.beneathList[0].Weight());
                this.stringWeight.Insert(1, this.beneathList[1].Weight());
                this.stringWeight.Insert(2, this.beneathList[2].Weight());
            }
            else
            {
                int targetPosition = position;

                // We need to find the way this string is now 'split', and leave the 'remainder' up to another call of InsertString.
                int passedWeights = this.stringWeight.TakeWhile(val => (targetPosition -= val) >= 0).Count();

                /* Warning: here, a position of 3 generates a targetPosition of -5 after noticing a weight 8. The position it needs to
                 *  go into is 3. But had it passed over a weight of 2, the targetposition would be '1' for the /next/ unit. Which is correct.
                 * 
                 * But if that had a weight of 4, it'd end up being 1-4 = -3. We need to re-add the last step, on which the evaluation failed. 
                 */
                if (targetPosition == 0)
                {
                    // We must place it 'between', at the beginning, or at the 'end' of a beneathList.
                    // We know how many elements in we should be... so:
                    // If count is at 0, it's an insert.
                    // If count is equal to stringWeight.Count, append at end.
                    // Otherwise, pass the new targetPosition to insert into the String.
                    this.beneathList.Insert(passedWeights, markupStringArg);
                    this.stringWeight.Insert(passedWeights, markupStringArg.Weight());
                }
                else
                {
                    // This is the adjustment for the 'last step reduction' error.
                    targetPosition += this.stringWeight[passedWeights];

                    this.beneathList[passedWeights].Insert(markupStringArg, targetPosition);
                    this.stringWeight[passedWeights] += markupStringArg.Weight();

                    if (!this.beneathList[passedWeights].MyMarkup.IsOnlyInherit())
                    {
                        return this;
                    }

                    // If the below is now an empty (OnlyInherit) Markup, we can 'pull it up'.
                    // That is to say, we can put its individual beneathLists in the position where we had our old one.
                    // If the item below is not an empty (OnlyInherit) Markup, then it wasn't the item below that we did
                    // the final insert into.
                    MarkupString reference = this.beneathList[passedWeights];
                    this.beneathList.RemoveAt(passedWeights);
                    this.stringWeight.RemoveAt(passedWeights);
                    this.beneathList.InsertRange(passedWeights, reference.beneathList);
                    this.stringWeight.InsertRange(passedWeights, reference.stringWeight);
                }
            }

            return this;
        }

        /// <summary>
        ///     Deletes a string and potential markup from the MarkupString, based on character position (starting at 0) and
        ///     length.
        /// </summary>
        /// <param name="position">The array-position for the first character to remove from the current MarkupString Node.</param>
        /// <param name="length">The amount of characters to delete, starting at the first character in this MarkupString Node.</param>
        /// <returns>The MarkupString itself.</returns>
        public MarkupString Remove(int position, int length)
        {
            var deletionIndexes = new List<int>();
            if (this.IsString())
            {
                this.markupString.Remove(position, length);
            }
            else
            {
                // We can't do this in parallel. Must be done in-order.
                foreach (MarkupString markupStringItem in this.beneathList)
                {
                    // We do this, because keeping a count will get corrupted by deletions.
                    int index = this.beneathList.IndexOf(markupStringItem);

                    // We're done if we have nothing else to delete.
                    if (length <= 0)
                    {
                        break;
                    }

                    // If the weight is less than the position, or equal to, this is not where we want to delete.
                    // So reduce the relative-position and try again.
                    if (markupStringItem.Weight() <= position)
                    {
                        position -= markupStringItem.Weight();
                        continue;
                    }

                    int maxCut = markupStringItem.Weight() - position;
                    int curCut = maxCut < length ? maxCut : length;

                    markupStringItem.Remove(position, curCut);

                    // Should this be done with a evalWeight() function or similar?
                    this.stringWeight[index] = markupStringItem.Weight();
                    length -= curCut;
                    position = 0;

                    if (markupStringItem.Weight() != 0)
                    {
                        continue;
                    }

                    // If this item is empty now, delete it. We no longer need it.
                    deletionIndexes.Add(index);
                }

                // We must reverse them. Because if we do it the wrong way around, we will be at the
                // wrong index after the first Removal.
                deletionIndexes.Reverse();

                // Begone! Evil empty nodes! You serve no purpose!
                foreach (int index in deletionIndexes)
                {
                    this.beneathList.RemoveAt(index);
                    this.stringWeight.RemoveAt(index);
                }
            }

            return this;
        }

        /// <summary>
        ///     Appends a MarkupString to the end of this instance of MarkupString.
        /// </summary>
        /// <param name="markupStringArg">The MarkupString being added to this instance.</param>
        /// <returns>The MarkupString itself.</returns>
        public MarkupString Append(MarkupString markupStringArg)
        {
            this.beneathList.Add(markupStringArg);
            this.stringWeight.Add(markupStringArg.Weight());
            return this;
        }

        /// <summary>
        ///     An edit function that replaces the position->range with a copy of the new MarkupString.
        /// </summary>
        /// <param name="newMarkupString">The new MarkupString to copy and insert into this structure.</param>
        /// <param name="position">The position where the edit begins, based on an insert position.</param>
        /// <param name="length">The length of string to 'replace' with this edit.</param>
        /// <returns>The MarkupString itself.</returns>
        public MarkupString Replace(MarkupString newMarkupString, int position, int length)
        {
            var replacement = new MarkupString(newMarkupString);
            this.Insert(replacement, position);
            this.Remove(position + replacement.Weight(), length);
            return this;
        }

        /// <summary>
        ///     Returns the plain String representation of the MarkupString. This visits all of its children.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            if (this.IsString())
            {
                return this.markupString.ToString();
            }

            var result = new StringBuilder();

            foreach (MarkupString each in this.beneathList)
            {
                result.Append(each);
            }

            return result.ToString();
        }

        /// <summary>
        ///     An in-place version of the Substring routine. It will cut off all unneeded pieces.
        /// </summary>
        /// <param name="position">The zero-based starting character position in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        private void CutSubstring(int position, int length)
        {
            if (this.IsString())
            {
                this.markupString.Remove(position + length, this.markupString.Length);
                this.markupString.Remove(0, position);
            }
            else
            {
                // We can optimize this logic by not relying on our own coded Remove, and create a faster deletion logic specifically
                // for CutSubstring. It's TODO.
                this.Remove(position + length, this.Weight());
                this.Remove(0, position);
            }
        }

        /// <summary>
        ///     Evaluates the weight of the node, by checking either the length of the string, or the items beneath it.
        /// </summary>
        /// <returns>An integer representation of how many characters are beneath this node.</returns>
        private int Weight()
        {
            return this.stringWeight == null
                ? this.markupString.Length
                : this.stringWeight.Sum();
        }

        /// <summary>
        ///     Returns whether or not this is a String node. The inverse is this being a Markup node.
        /// </summary>
        /// <returns>Whether this is a string or not.</returns>
        private bool IsString()
        {
            return this.markupString != null;
        }
    }
}