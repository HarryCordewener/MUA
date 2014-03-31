//-----------------------------------------------------------------------
// <copyright file="MObject.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    /// <summary>
    ///     A MUA Object.
    /// </summary>
    public class MObject
    {
        /// <summary>
        ///     Personal identifier number.
        /// </summary>
        private int databaseReference;

        /// <summary>
        ///     The name of the object.
        /// </summary>
        private string objectName;

        /// <summary>
        ///     linux timestamp since creation.
        /// </summary>
        private int creationTime;

        /// <summary>
        ///     linux timestamp of the last time this object was edited.
        /// </summary>
        private int modificationTime;

        /// <summary>
        ///     All objects are owned by a Player object. Players own themselves.
        ///     Considering renaming the Player object to something more fitting.
        /// </summary>
        private Player owner;

        /// <summary>
        ///     Optional MObject parent.
        /// </summary>
        private MUA.MObject parent;

        // private List<Attribute> rootAttributes 
        // private List<Flag> flags;                                                
    }
}