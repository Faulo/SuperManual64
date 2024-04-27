using System.Runtime.CompilerServices;
using SuperManual64;

[assembly: InternalsVisibleTo(AssemblyInfo.NAMESPACE_EDITOR)]
[assembly: InternalsVisibleTo(AssemblyInfo.NAMESPACE_TESTS)]
[assembly: InternalsVisibleTo(AssemblyInfo.NAMESPACE_PROXYGEN)]

namespace SuperManual64 {
    static class AssemblyInfo {
        public const string NAMESPACE_RUNTIME = "SuperManual64";
        public const string NAMESPACE_EDITOR = "SuperManual64.Editor";
        public const string NAMESPACE_TESTS = "SuperManual64.Tests";
        public const string NAMESPACE_PROXYGEN = "DynamicProxyGenAssembly2";
    }
}
