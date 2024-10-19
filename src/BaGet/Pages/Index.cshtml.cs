using BaGet.Core;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Search;

namespace BaGet.Pages
{
    public class IndexModel(ISearchService search)
        : PageModel
    {
        public const int ResultsPerPage = 20;

        [BindProperty(Name = "q", SupportsGet = true)]
        public string Query { get; set; } = string.Empty;

        [BindProperty(Name = "p", SupportsGet = true)]
        [Range(1, int.MaxValue)]
        public int PageIndex { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string PackageType { get; set; } = "any";

        [BindProperty(SupportsGet = true)]
        public string Framework { get; set; } = "any";

        [BindProperty(SupportsGet = true)]
        public bool Prerelease { get; set; } = true;

        public IList<SearchResult> Packages { get; private set; } = new List<SearchResult>();

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var packageType = PackageType == "any" ? null : PackageType;
            var framework = Framework == "any" ? null : Framework;

            var search1 = await search.SearchAsync(
                new SearchRequest
                {
                    Skip = (PageIndex - 1) * ResultsPerPage,
                    Take = ResultsPerPage,
                    IncludePrerelease = Prerelease,
                    IncludeSemVer2 = true,
                    PackageType = packageType,
                    Framework = framework,
                    Query = Query,
                },
                cancellationToken);

            Packages = search1.Data.ToList();

            return Page();
        }
    }
}
