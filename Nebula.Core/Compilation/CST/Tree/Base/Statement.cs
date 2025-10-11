using Nebula.Commons.Syntax;
using Nebula.Commons.Text;

namespace Nebula.Core.Compilation.CST.Tree.Base
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
