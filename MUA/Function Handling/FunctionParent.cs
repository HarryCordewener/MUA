//-----------------------------------------------------------------------
// <copyright file="FunctionParent.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The FunctionParent is intended by be inherited by all functions. A call back to ExecuteBody,
    ///     only defined in the parent, will then execute a function's body.
    /// </summary>
    public class FunctionParent : IFunction
    {
        /// <summary>
        ///     Gets or sets the name of the function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the minimum amount of arguments. i less than 0 for No Minimum.
        /// </summary>
        public int ArgMin { get; set; }

        /// <summary>
        ///     Gets or sets the maximum amount of arguments. i less than 0 for No Maximum.
        /// </summary>
        public int ArgMax { get; set; }

        /// <summary>
        ///     Gets or sets the flags that control how the function is parsed.
        /// </summary>
        /// <remarks>
        ///     Type may change.
        /// </remarks>
        public uint Flags { get; set; }

        /// <summary>
        ///     Gets or sets the Type - whether it is Soft- or Hard-Code
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Gets or sets the Permissions of the function.
        /// </summary>
        public Permission Permissions { get; set; }

        /// <summary>
        ///     Executes the body of the child. This will never be called on itself.
        /// </summary>
        /// <param name="obj">The object executing the body, used for the sake of evaluating Permissions.</param>
        /// <param name="arguments">A Stack of FunctionResults that represent the arguments.</param>
        /// <returns>The MarkupString to be appended as a result of this function call.</returns>
        /// <remarks>This may have to be changed to a FunctionResultStringTuple or something similar to enable the use of tuples.</remarks>
        public FunctionResult ExecuteBody(ref MUA.MObject obj, ref Stack<FunctionResult> arguments)
        {
            this.Permissions.HasPermission(ref obj);
            throw new NotImplementedException();
        }
    }
}