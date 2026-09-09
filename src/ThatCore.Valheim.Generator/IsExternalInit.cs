// Stupid fix for records expecting init accessors to be available, but this is not exposed in netstandard2.0...
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}