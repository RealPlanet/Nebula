using Nebula.Commons.Syntax;
using Nebula.Commons.Text;

namespace Nebula.Core.Compilation.CST.Tree.Base
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
