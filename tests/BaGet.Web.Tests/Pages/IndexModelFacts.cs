using BaGet.Core.Search;
using BaGet.Pages;
using BaGet.Protocol.Models;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BaGet.Web.Tests
{
    public class IndexModelFacts
    {
        private readonly IndexModel _target;

        private SearchRequest _capturedRequest;
        private readonly SearchResponse _response = new SearchResponse();
        private readonly CancellationToken _cancellation = CancellationToken.None;

        public IndexModelFacts()
        {
            var search = new Mock<ISearchService>();
            search
                .Setup(s => s.SearchAsync(It.IsAny<SearchRequest>(), _cancellation))
                .Callback((SearchRequest r, CancellationToken c) => _capturedRequest = r)
                .ReturnsAsync(_response);

            _target = new IndexModel(search.Object);
        }

        [Fact]
        public async Task DefaultSearch()
        {
            await _target.OnGetAsync(_cancellation);

            Assert.Null(_capturedRequest);
        }

        [Fact]
        public async Task AcceptsParameters()
        {
            _target.Prerelease = false;
            _target.PageIndex = 5;
            _target.PackageType = "foo";
            _target.Framework = "bar";
            _target.Query = "Hello world";

            await _target.OnGetAsync(_cancellation);

            Assert.Equal(5, _capturedRequest.PageIndex);
            Assert.Equal(20, _capturedRequest.PageCount);
            Assert.False(_capturedRequest.IncludePrerelease);
            Assert.True(_capturedRequest.IncludeSemVer2);
            Assert.Equal("foo", _capturedRequest.PackageType);
            Assert.Equal("bar", _capturedRequest.Framework);
            Assert.Equal("Hello world", _capturedRequest.Query);
        }
    }
}
