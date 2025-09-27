namespace Nebula.Core.Compilation.AST.Tree.Base
{
    public enum AbstractNodeType
    {
        // Statement
        //SequencePointStatement,
        NopStatement,
        BlockStatement,
        ExpressionStatement,
        VariableDeclarationCollection,
        VariableDeclaration,
        NamespaceDeclaration,
        WaitStatement,
        WaitNotificationStatement,
        NotifyStatement,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        ConditionalGotoStatement,
        GotoStatement,
        LabelStatement,
        ReturnStatement,

        // Expression
        ErrorExpression,
        LiteralExpression,
        DefaultInitializationExpression,
        CompoundAssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        ObjectFieldAssignmentExpression,
        ObjectCallExpression,
        ArrayAssignmentExpression,
        ArrayAccessExpression,
        CallExpression,
        ConversionExpression,
    }
}
