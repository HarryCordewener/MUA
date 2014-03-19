using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MUA
{
    partial class MarkupString
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkupString" />, copied from the object given.
        /// </summary>
        /// <param name="copyFrom">The MarkupString node to assumed to be root - and copy from.</param>
        public MarkupString(MarkupString copyFrom)
        {
            copyFrom.CopyInto(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkupString" />, copied from the object given, based on
        ///     position and length. This is also known as the Substring copy constructor.
        /// </summary>
        /// <param name="copyFrom">The MarkupString node to assumed to be root - and copy from.</param>
        /// <param name="position">What character position (0 based) to start the copy from.</param>
        /// <param name="length">How many characters to copy.</param>
        public MarkupString(MarkupString copyFrom, int position, int length)
        {
            copyFrom.CopySubstringInto(this, position, length);
        }

        /// <summary>
        ///     Wrapper for flattening the MarkupString. This is meant to create an equivalent MarkupString List.
        ///     <remarks>
        ///         This should call Mix as we move down, and then when we hit text, create the new MarkupString representation
        ///         of that string, with a parent that holds this new Mixed Markup.
        ///     </remarks>
        /// </summary>
        /// <param name="markupStringList">A reference to a markupStringList to put the generated MarkupString instances into.</param>
        public void FlattenInto(ref List<MarkupString> markupStringList)
        {
            var myMarkup = new Markup();
            FlattenInto(ref markupStringList, MyMarkup.Mix(myMarkup));
        }

        /// <summary>
        /// Implementation of flattening the MarkupString. This assumes the wrapper has given us a mixed markup for the first step.
        /// </summary>
        /// <param name="markupStringList">A reference to a markupStringList to put the generated MarkupString instances into.</param>
        /// <param name="mup">The Markup of our parent to Markup.Mix()</param>
        private void FlattenInto(ref List<MarkupString> markupStringList, Markup mup)
        {
            if (!IsString())
            {
                foreach (MarkupString each in beneathList)
                {
                    each.FlattenInto(ref markupStringList, MyMarkup.Mix(mup));
                }
            }
            else
            {
                var myMarkupParent = new MarkupString(mup);
                myMarkupParent.Insert(markupString.ToString(), 0);
                markupStringList.Add(myMarkupParent);
            }
        }

        /// <summary>
        ///     Retrieves a substring from this instance. The substring starts at a specified position and has a specified length.
        /// </summary>
        /// <param name="position">The zero-based starting character position in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A new MarkupString object containing the object's substring.</returns>
        public MarkupString Substring(int position, int length)
        {
            return new MarkupString(this, position, length);
        }

        /// <summary>
        ///     Retrieves a substring from this instance. The substring starts at a specified position.h.
        /// </summary>
        /// <param name="position">The zero-based starting character position in this instance.</param>
        /// <returns>A new MarkupString object containing the object's substring.</returns>
        public MarkupString Substring(int position)
        {
            return new MarkupString(this,position,Weight()-position);
        }

        /// <summary>
        ///     The work-horse for destructively copying a substring set of a MarkupString into the given MarkupString object.
        /// </summary>
        /// <param name="newMarkupString">The MarkupString object to copy into.</param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns>The newMarkupString, now filled with this MarkupString's information.</returns>
        private MarkupString CopySubstringInto(MarkupString newMarkupString, int position, int length)
        {
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
                foreach (MarkupString markupStringItem in beneathList)
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

                    int maxCut = markupStringItem.Weight() - position;
                    int curCut = (maxCut < length ? maxCut : length);

                    var thisOne = new MarkupString();
                    markupStringItem.CopySubstringInto(thisOne, position, curCut);

                    newMarkupString.beneathList.Add(thisOne);
                    newMarkupString.stringWeight.Add(thisOne.Weight());

                    length -= curCut;
                    position = 0;
                }
            }
            return this;
        }

        /// <summary>
        ///     Destructively Copies the whole MarkupString Structure into the given MarkupString Object.
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
            foreach (MarkupString mySubMarkupString in beneathList)
            {
                newMarkupString.markupString = null;
                var thisOne = new MarkupString();
                newMarkupString.beneathList.Add(mySubMarkupString.CopyInto(thisOne));
                newMarkupString.stringWeight.Add(thisOne.Weight());
            }

            return newMarkupString;
        }

        /// <summary>
        ///     A very basic surface level ToString. This only evaluates the String representation of the current item,
        ///     and none of its children.
        /// </summary>
        /// <returns>A string representation of this MarkupString element only.</returns>
        public string ToSurfaceString()
        {
            return IsString() ? markupString.ToString() : MyMarkup.ToString();
        }

        /// <summary>
        ///     Returns the String representation of the MarkupString. This visits all of its children.
        /// </summary>
        /// <returns>A string.</returns>
        public string ToTestString()
        {
            if (IsString()) return markupString.ToString();

            var result = new StringBuilder();
            result.Append("<" + MyMarkup + ">");

            foreach (MarkupString each in beneathList)
            {
                result.Append(each.ToTestString());
            }

            result.Append("</" + MyMarkup + ">");
            return result.ToString();
        }

        /// <summary>
        ///     Returns the plain String representation of the MarkupString. This visits all of its children.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            if (IsString()) return markupString.ToString();

            var result = new StringBuilder();

            foreach (MarkupString each in beneathList)
            {
                result.Append(each);
            }

            return result.ToString();
        }

        /// <summary>
        /// Splits the MarkupString into a List of MarkupStrings, split by the delimiter, and with the delimiter removed.
        /// </summary>
        /// <param name="delimiter">The string delimiter to use in splitting the MarkupString</param>
        /// <returns>A List of MarkupStrings, split by the delimiter, and with the delimiter removed.</returns>
        public List<MarkupString> Split(string delimiter)
        {
            var result = new List<MarkupString>();
            Regex genRegex = new Regex(Regex.Escape(delimiter));
            Split(ref genRegex, ref result);
            return result;
        }
        
        /// <summary>
        /// Splits the MarkupString into a List of MarkupStrings, split by the delimiter, and with the delimiter removed.
        /// It additively alters the given markupStringList to do this.
        /// </summary>
        /// <param name="delimiter">A regular expression that describes the delimiter.</param>
        /// <param name="markupStringList">The list to which to add the substrings.</param>
        private void Split(ref Regex delimiter, ref List<MarkupString> markupStringList)
        {
            int leftbehind = 0;
            string myself = ToString();
            var results = delimiter.Matches(myself);

            if (results.Count == 0)
            {
                markupStringList.Add(new MarkupString(this));
                return;
            } 

            foreach (Match group in results)
            {
                markupStringList.Add(Substring(leftbehind, group.Index - leftbehind));
                leftbehind = group.Index + group.Length;
            }

            markupStringList.Add(Substring(leftbehind));
        }
    }
}