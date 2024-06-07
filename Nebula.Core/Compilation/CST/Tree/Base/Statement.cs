using Nebula.Commons.Syntax;
using Nebula.Commons.Text;

namespace Nebula.Core.Parsing
{
    public abstract class Statement
        : Node
    {
        protected Statement(SourceCode code)
                : base(code)
        {
        }
    }
}
