//-----------------------------------------------------------------------
// <copyright file="FunctionResult.cs" company="Twilight Days">
// ...
// </copyright>
// <author>Harry Cordewener</author>
//-----------------------------------------------------------------------

namespace MUA
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The purpose of a Function Result is to be able to funnel a function's response straight into a command or another
    ///     function.
    ///     This by-steps the buffer requirements, until we get to the end. We're passing around the same object in a chain,
    ///     editing the same object,
    ///     thus reducing the strain this might have otherwise - if we had to constantly copy objects.
    ///     By design, a FunctionResult can carry a Tuple of MushString results.
    ///     This assists in functions who return more than one thing.
    ///     A FunctionResult must also be able to spawn a separate FunctionResult when requested;
    ///     to be used as part of a QREG or otherwise.
    /// 
    ///     The item at the 'start' of the List should be the latest 'main' result of a function.
    /// </summary>
    internal class FunctionResult
    {
        /// <summary>
        /// Contains the MarkupStrings that represent the results.
        /// </summary>
        private readonly List<MarkupString> results = new List<MarkupString>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionResult" /> class, along with a copy of the MarkupString given.
        /// </summary>
        /// <param name="markupString">The MarkupString to insert into the Result.</param>
        public FunctionResult(MarkupString markupString)
        {
            this.results.Add(new MarkupString(markupString));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionResult" /> class, copied from another instance.
        ///     Also copies each of its MarkupStrings.
        /// </summary>
        /// <param name="copyFrom">A FunctionResult instance to copy from.</param>
        public FunctionResult(FunctionResult copyFrom)
        {
            this.CopyResult(copyFrom);
        }

        /// <summary>
        /// Returns a single MarkupString from the list of MushStrings.
        /// </summary>
        /// <param name="index">The integer zero-based index of the MarkupString.</param>
        /// <returns>A MushString.</returns>
        public MarkupString this[int index]
        {
            get { return this.GetResult(index); }
        }

        /// <summary>
        ///     Adds a copy of each MarkupString of the FunctionResult given into this FunctionResult.
        /// </summary>
        /// <param name="result">A FunctionResult instance to copy from.</param>
        public void CopyResult(FunctionResult result)
        {
            // Makes a copy of each item in 'result', and then stores it in this instance's own results.
            this.results.Clear();
            this.results.AddRange(result.GetResults().Select(each => new MarkupString(each)).ToList());
        }

        /// <summary>
        /// Returns the list of MushStrings.
        /// </summary>
        /// <returns>A List of MarkupString.</returns>
        public List<MarkupString> GetResults()
        {
            return this.results;
        }

        /// <summary>
        /// Returns a single MarkupString from the list of MushStrings.
        /// </summary>
        /// <param name="index">The integer zero-based index of the MarkupString.</param>
        /// <returns>A MushString.</returns>
        public MarkupString GetResult(int index)
        {
            if (index > this.results.Count || index < 0)
            {
                return null;
            }

            return this.results.ElementAt(index);
        }
    }
}