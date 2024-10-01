using System.ComponentModel;

// needed for records to work in .net 4.8, https://stackoverflow.com/a/67687186
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// This dummy class is required to compile records when targeting .NET Standard
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit
    {
    }
}