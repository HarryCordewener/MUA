//-----------------------------------------------------------------------
// <copyright file="IFunction.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    /// <summary>
    /// This interface ensures that a new function definition has all of the following items defined.
    /// </summary>
    public interface IFunction
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of arguments. i less than 0 for No Minimum.
        /// </summary>
        int ArgMin { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of arguments. i less than 0 for No Maximum.
        /// </summary>
        int ArgMax { get; set; }

        /// <summary>
        /// Gets or sets the flags that control how the function is parsed. 
        /// </summary>
        /// <remarks>
        /// Type may change.
        /// </remarks>
        uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the Type - whether it is Soft- or Hard-Code
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the Permissions of the function.
        /// </summary>
        Permission Permissions { get; set; }
    }
}
