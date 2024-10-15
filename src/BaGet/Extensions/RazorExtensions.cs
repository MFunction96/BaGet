using Humanizer;

namespace BaGet.Extensions
{
    public static class RazorExtensions
    {
        public static string ToMetric(this long value)
        {
            return ((double) value).ToMetric();
        }
    }
}
