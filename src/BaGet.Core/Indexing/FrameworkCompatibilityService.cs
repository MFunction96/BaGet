using NuGet.Frameworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BaGet.Core.Indexing
{
    using static FrameworkConstants;

    public class FrameworkCompatibilityService : IFrameworkCompatibilityService
    {
        private const string AnyFramework = "any";

        private static readonly Dictionary<string, NuGetFramework?> KnownFrameworks;
        private static readonly IEnumerable<OneWayCompatibilityMappingEntry> CompatibilityMapping;
        private static readonly ConcurrentDictionary<NuGetFramework, IEnumerable<string>> CompatibleFrameworks;

        static FrameworkCompatibilityService()
        {
            var supportedFrameworks = new HashSet<string>
            {
                FrameworkIdentifiers.NetStandard,
                FrameworkIdentifiers.NetCoreApp,
                FrameworkIdentifiers.Net
            };

            CompatibilityMapping = DefaultFrameworkMappings.Instance.CompatibilityMappings.ToList();
            CompatibleFrameworks = new ConcurrentDictionary<NuGetFramework, IEnumerable<string>>();

            KnownFrameworks = typeof(CommonFrameworks)
                .GetFields()
                .Where(f => f.IsStatic)
                .Where(f => f.FieldType == typeof(NuGetFramework))
                .Select(f => (NuGetFramework)f.GetValue(null))
                .Where(f => supportedFrameworks.Contains(f.Framework))
                .ToDictionary(f => f.GetShortFolderName());

            // Add more frameworks missing from "CommonFrameworks"
            KnownFrameworks["net472"] = new NuGetFramework(FrameworkIdentifiers.Net, new Version(4, 7, 2, 0));
            KnownFrameworks["net471"] = new NuGetFramework(FrameworkIdentifiers.Net, new Version(4, 7, 1, 0));
        }

        public IEnumerable<string> FindAllCompatibleFrameworks(string name)
        {
            if (!KnownFrameworks.TryGetValue(name, out var framework))
            {
                return new List<string> { name, AnyFramework };
            }

            return CompatibleFrameworks.GetOrAdd(framework, FindAllCompatibleFrameworks);
        }

        private IEnumerable<string> FindAllCompatibleFrameworks(NuGetFramework targetFramework)
        {
            var results = new HashSet<string> { AnyFramework };

            // Find all framework mappings that apply to the target framework
            foreach (var mapping in CompatibilityMapping)
            {
                // Skip this mapping if it isn't for our target framework.
                if (!mapping.TargetFrameworkRange.Satisfies(targetFramework))
                {
                    continue;
                }

                // Any framework that satisfies this mapping is compatible with the target framework.
                foreach (var possibleFramework in KnownFrameworks.Values)
                {
                    if (mapping.SupportedFrameworkRange.Satisfies(possibleFramework))
                    {
                        results.Add(possibleFramework.GetShortFolderName());
                    }
                }
            }

            // Find all frameworks that have the same "framework identifier" and a version
            // less than equal to our target framework. For example, "net45" is compatible
            // with "net20" and "net45".
            foreach (var possibleFramework in KnownFrameworks.Values)
            {
                if (possibleFramework.Framework == targetFramework.Framework &&
                    possibleFramework.Version <= targetFramework.Version)
                {
                    results.Add(possibleFramework.GetShortFolderName());
                }
            }

            return results.ToList();
        }
    }
}
