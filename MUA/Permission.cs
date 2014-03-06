//-----------------------------------------------------------------------
// <copyright file="Permission.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    using System;

    /// <summary>
    /// The permissions of a function, or otherwise. A class that codifies these.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Checks permissions of a player versus its evaluation body.
        /// </summary>
        /// <param name="player">A reference to the player object trying to evaluate against the Permission Object.</param>
        /// <returns>A boolean, whether or not the player has access do whatever they are trying to do.</returns>
        public bool HasPermission(ref Player player) 
        {
            throw new NotImplementedException();
        }
    }
}
