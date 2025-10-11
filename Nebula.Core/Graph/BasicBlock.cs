using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Utility.Concrete;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Nebula.Core.Graph
{
    public sealed class BasicBlock
    {
        /// <summary>
        /// List of instructions (flattened) where only the first instruction can be targetted by a goto instruction and only the last instruction can escape the block.
        /// </summary>
        public List<AbstractStatement> Statements { get; } = new();

        /// <summary>
        /// Each node knows the nodes which are entering and exiting this block of instructions
        /// </summary>
        public List<BasicBlockBranch> Incoming { get; } = new();

        /// <summary>
        /// Each node knows the nodes which are entering and exiting this block of instructions
        /// </summary>
        public List<BasicBlockBranch> Outgoing { get; } = new();
        public bool IsStart { get; }
        public bool IsEnd { get; }
        public BasicBlock() { }
        public BasicBlock(bool isStart)
        {
            IsStart = isStart;
            IsEnd = !isStart;
        }
        public override string ToString()
        {
            if (IsStart)
            {
                return "<start>";
            }

            if (IsEnd)
            {
                return "<end>";
            }

            using (StringWriter writer = new())
            using (IndentedTextWriter indentedWriter = new(writer))
            {
                foreach (AbstractStatement statement in Statements)
                {
                    statement.WriteTo(indentedWriter);
                }

                return writer.ToString();
            }
        }
    }
}