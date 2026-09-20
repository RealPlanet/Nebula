using Nebula.Commons.Syntax;
using Nebula.Commons.Text;

namespace Nebula.Core.Compilation.CST.Tree.Types
{
    public abstract class BaseTypeClause
        : Node
    {
        protected BaseTypeClause(SourceCode sourceCode) 
            : base(sourceCode)
        {
        }
    }
}
