using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    /// <summary>
    /// An object that implements this interface can include comments in the compiled file
    /// </summary>
    public interface ISupportsComments
    {
        /// <summary>
        /// Comments appended before the object
        /// </summary>
        HashSet<string> LeadingComments { get; }
    }
}
