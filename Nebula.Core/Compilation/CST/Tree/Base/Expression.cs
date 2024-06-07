using Nebula.Commons.Syntax;
using Nebula.Commons.Text;

namespace Nebula.Core.Parsing
{
    public abstract class Expression
        : Node
    {
        protected Expression(SourceCode sourceCode)
            : base(sourceCode)
        {
        }
    }
}
