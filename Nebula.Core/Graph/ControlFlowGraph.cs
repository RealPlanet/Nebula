using Nebula.Core.Binding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nebula.Core.Graph
{
    public sealed class ControlFlowGraph
    {
        #region Public
        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public void WriteTo(TextWriter writer)
        {
            static string Quote(string text)
            {
                return "\"" + text.TrimEnd().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace(Environment.NewLine, "\\l") + "\"";
            }

            writer.WriteLine("digraph G {");
            Dictionary<BasicBlock, string> blocksIds = new();
            for (int i = 0; i < Blocks.Count; i++)
            {
                BasicBlock? node = Blocks[i];
                string id = $"N{i}";
                blocksIds.Add(node, id);
            }

            foreach (BasicBlock? block in Blocks)
            {
                string id = blocksIds[block];
                string label = Quote(block.ToString());
                writer.WriteLine($"     {id} [label = {label}, shape = box]");
            }

            foreach (BasicBlockBranch? branch in Branches)
            {
                string fromId = blocksIds[branch.From];
                string toId = blocksIds[branch.To];
                string label = Quote(branch.ToString());
                writer.WriteLine($"     {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        #endregion

        public ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        #region Static

        public static bool AllPathsReturn(AbstractBlockStatement loweredBody)
        {
            ControlFlowGraph graph = Create(loweredBody);
            foreach (BasicBlockBranch? branch in graph.End.Incoming)
            {
                AbstractStatement? lastStatement = branch.From.Statements.LastOrDefault();
                if (lastStatement == null || lastStatement.Type != AbstractNodeType.ReturnStatement)
                    return false;
            }

            return true;
        }

        public static ControlFlowGraph Create(AbstractBlockStatement body)
        {
            BasicBlockBuilder? basicBlockBuilder = new();
            List<BasicBlock> blocks = basicBlockBuilder.Build(body);

            GraphBuilder graphBuilder = new();
            return graphBuilder.Build(blocks);
        }

        #endregion
    }
}