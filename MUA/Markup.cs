//-----------------------------------------------------------------------
// <copyright file="Markup.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

using System.Text;

namespace MUA
{
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
        /// Contains the markup as a string. 
        /// <remarks>(TMP TEST VERSION). Also, we may be able to reduce memory for MarkupString with GetValueOrDefault()?</remarks>
        /// </summary>
        public HashSet<MarkupRule> MyMarkup;

        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class.
        /// </summary>
        public Markup()
        {
            MyMarkup = new HashSet<MarkupRule> { MarkupRule.OnlyInherit };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class. Based on a MarkupRule unique list.
        /// </summary>
        /// <param name="myMarkup">An optional MarkupRule representation of the markup.</param>
        public Markup(IEnumerable<MarkupRule> myMarkup)
        {
            MyMarkup = new HashSet<MarkupRule>();
            if (myMarkup == null)
            {
                MyMarkup.Add(MarkupRule.OnlyInherit);
                return;
            }
            MyMarkup.UnionWith(myMarkup);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Markup"/> class. Copies from the given myMarkup variable.
        /// </summary>
        /// <param name="myMarkup">The Markup object to copy off of.</param>
        public Markup(Markup myMarkup)
        {
            MyMarkup = new HashSet<MarkupRule>();
            if (myMarkup == null)
            {
                MyMarkup.Add(MarkupRule.OnlyInherit);
                return;
            }
            myMarkup.CopyMarkup(this);
        }

        /// <summary>
        /// This function mixes a Markup Object, assuming this object is the child (new addition) and the argument is the parent.
        /// It returns a resulting Markup object.
        /// </summary>
        /// <param name="markup">The Markup object we inherit from.</param>
        /// <returns>The resulting Markup object.</returns>
        public Markup Mix(Markup markup)
        {
            var result = new Markup(markup.MyMarkup);
            result.MyMarkup.UnionWith(MyMarkup);

            return result;
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

        /// <summary>
        /// Destructively copies the current Markup of this instance to the markup object given.
        /// </summary>
        /// <param name="newMarkup">A new Markup object made with the default Constructor.</param>
        /// <returns>The newMarkup given, now copied into.</returns>
        private void CopyMarkup(Markup newMarkup)
        {
            newMarkup.MyMarkup.UnionWith(MyMarkup);
        }
    }
}
