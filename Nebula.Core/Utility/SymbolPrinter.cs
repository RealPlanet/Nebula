using Nebula.Commons.Syntax;
using Nebula.Commons.Text.Printers;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Symbols.Base;
using System;
using System.IO;

namespace Nebula.Core.Utility
{
    public sealed class SymbolPrinter
    {
        public static void WriteTo(Symbol symbol, TextWriter writer)
        {
            switch (symbol.SymbolType)
            {
                case SymbolType.Function:
                    WriteFunctionTo((FunctionSymbol)symbol, writer);
                    break;
                //case SymbolType.GLOBAL_VARIABLE:
                //    WriteGlobalVariableTo((GlobalVariableSymbol)symbol, writer);
                //    break;
                case SymbolType.LocalVariable:
                    WriteLocalVariableTo((LocalVariableSymbol)symbol, writer);
                    break;
                case SymbolType.Parameter:
                    WriteParameterTo((ParameterSymbol)symbol, writer);
                    break;
                case SymbolType.Type:
                    WriteTypeTo((TypeSymbol)symbol, writer);
                    break;
                default:
                    throw new Exception($"Unexpected symbol {symbol.SymbolType}");
            }
        }
        private static void WriteFunctionTo(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword(NodeType.FuncKeyword);
            writer.WriteSpace();

            if (symbol.ReturnType != TypeSymbol.Void)
            {
                symbol.ReturnType.WriteTo(writer);
                writer.WriteSpace();
            }

            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(NodeType.OpenParenthesisToken);

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                ParameterSymbol? p = symbol.Parameters[i];
                if (i > 0)
                {
                    writer.WritePunctuation(NodeType.CommaToken);
                    writer.WriteSpace();
                }

                p.WriteTo(writer);
            }

            writer.WritePunctuation(NodeType.ClosedParenthesisToken);
        }

        //private static void WriteGlobalVariableTo(GlobalVariableSymbol symbol, TextWriter writer)
        //{
        //    writer.WriteKeyword(symbol.IsReadOnly ? SyntaxType.CONST_KEYWORD : SyntaxType.VAR_KEYWORD);
        //    writer.WriteSpace();
        //    symbol.Type?.WriteTo(writer);
        //    writer.WriteSpace();
        //    writer.WriteIdentifier(symbol.Name);
        //}
        private static void WriteLocalVariableTo(LocalVariableSymbol symbol, TextWriter writer)
        {
            if (symbol.IsReadOnly)
            {
                writer.WriteKeyword(NodeType.ConstKeyword);
            }

            writer.WriteSpace();
            symbol.Type?.WriteTo(writer);
            writer.WriteSpace();
            writer.WriteIdentifier(symbol.Name);
        }
        private static void WriteParameterTo(ParameterSymbol symbol, TextWriter writer)
        {
            symbol.Type.WriteTo(writer);
            writer.WriteSpace();
            writer.WriteIdentifier(symbol.Name);
        }
        private static void WriteTypeTo(TypeSymbol symbol, TextWriter writer) => writer.WriteIdentifier(symbol.Name);
    }

}
