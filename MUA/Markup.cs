//-----------------------------------------------------------------------
// <copyright file="Markup.cs" company="Twilight Days">
// ...
// </copyright>
//-----------------------------------------------------------------------

namespace MUA
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains Markup information for a <see cref="MarkupString"/>.
    /// </summary>
    public class Markup
    {
        /// <summary>
        /// This function mixes a range of Markups (most likely inherited) and gives the resulting Markup object.
        /// </summary>
        /// <param name="markupList">The list of Markup objects to mix.</param>
        /// <returns>The resulting Markup object.</returns>
        public Markup Mix(ref List<Markup> markupList)
        {
            throw new NotImplementedException();
        }
    }
}
