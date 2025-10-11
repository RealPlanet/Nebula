using Nebula.CodeGeneration.Interfaces;
using Nebula.Commons.Syntax;

namespace Nebula.CodeGeneration
{
    public interface IEmitterObject
        : ISupportsComments
    {
        /// <summary>The original node that is emitting this object, used for debug file generation</summary>
        Node? OriginalNode { get; }
    }
}
