using System.Collections.Generic;

namespace BaGet.Core.Indexing
{
    /// <summary>
    /// Used to determine the compatibility matrix between frameworks.
    /// </summary>
    public interface IFrameworkCompatibilityService
    {
        /// <summary>
        /// Given a framework, find all other compatible frameworks.
        /// </summary>
        /// <param name="framework">The input framework.</param>
        /// <returns>The list of compatible frameworks.</returns>
        IEnumerable<string> FindAllCompatibleFrameworks(string framework);
    }
}
