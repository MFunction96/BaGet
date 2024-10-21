using BaGet.Core.Metadata;
using BaGet.Protocol.Models;
using System.Collections.Generic;

namespace BaGet.Core.Search
{
    public interface ISearchResponseBuilder
    {
        SearchResponse BuildSearch(IEnumerable<PackageRegistration> results);
        AutocompleteResponse BuildAutocomplete(IReadOnlyList<string> data);
        DependentsResponse BuildDependents(IReadOnlyList<PackageDependent> results);
    }
}
