//-----------------------------------------------------------------------
// <copyright file="Markup.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Text;

namespace MUA
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the markup rules. This is only a test version.
    /// This will be far more complex in the future.
    /// </summary>
    public enum MarkupRule
    {
        /// <summary>
        /// HiLighting color
        /// </summary>
        HiLight,
        /// <summary>
        /// Inverting colors that follow.
        /// </summary>
        Inverse,
        /*Red,
        Blue,
        Green,
        Cyan,
        Yellow,
        Magenta,
        Black,
        White,
        Default,
        NoInherit, */
        /// <summary>
        /// OnlyInherit. Exclusive, and should only be in here for a 'new' markup node.
        /// </summary>
        OnlyInherit
    }

    /// <summary>
    /// Contains Markup information for a <see cref="MarkupString"/>.
    /// <remarks>
    /// It must be noted that a Markup when it is initialized, must not actually state any markup information,
    ///  unless it is given markup information. The reason for this, is to enable blank transitions. Also known 
    ///  as inheritance transitions.
    /// </remarks>
    /// </summary>
    public class Markup
    {
        /// <summary>
        /// Contains the markup as a string. <remarks>(TMP TEST VERSION)</remarks>
        /// </summary>
        public List<MarkupRule> MyMarkup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class.
        /// </summary>
        /// <param name="myMarkup">An optional string representation of the markup. <remarks>(TMP TEST VERSION)</remarks></param>
        public Markup(List<MarkupRule> myMarkup)
        {
            if (myMarkup == null)
            {
                MyMarkup = new List<MarkupRule> { MarkupRule.OnlyInherit };
                return;
            }
            MyMarkup = myMarkup;
        }

        /// <summary>
        /// This function mixes a Markup Object, assuming this object is the child (new addition) and the argument is the parent.
        /// It returns a resulting Markup object.
        /// </summary>
        /// <param name="markupList">The Markup object we inherit from.</param>
        /// <returns>The resulting Markup object.</returns>
        public Markup Mix(Markup markupList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns whether this node is OnlyInherit, and can thus be deleted.
        /// </summary>
        /// <returns></returns>
        public bool IsOnlyInherit()
        {
            return MyMarkup.Contains(MarkupRule.OnlyInherit);
        }

        /// <summary>
        /// Returns a string representation of the current Markup.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var each in MyMarkup)
                sb.Append(each);
            return sb.ToString();
        }
    }
}
