using BaGet.Core.Entities;
using System.Collections.Generic;

namespace BaGet.Core.Metadata
{
    /// <summary>
    /// The information on all versions of a package.
    /// </summary>
    public class PackageRegistration
    {
        /// <summary>
        /// Create a new registration object.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="packages">All versions of the package.</param>
        public PackageRegistration(
            string packageId,
            IEnumerable<Package> packages)
        {
            PackageId = packageId;
            Packages = packages;
        }

        /// <summary>
        /// The package's ID.
        /// </summary>
        public string PackageId { get; }

        /// <summary>
        /// The information for each version of the package.
        /// </summary>
        public IEnumerable<Package> Packages { get; }
    }
}
