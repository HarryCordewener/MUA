//-----------------------------------------------------------------------
// <copyright file="Object.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    /// <summary>
    ///     A MUA Object.
    /// </summary>
    public class Object
    {
        private int creationTime;
        private int dbReference;
        private int modificationTime;
        private string objectName;
        private Player owner;
        private MUA.Object parent;
        // private List<Attribute> rootAttributes 
        // private List<Flag> flags;                                                
    }
}