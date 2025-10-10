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
        InitializationExpression,
        CompoundAssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        ObjectFieldAssignmentExpression,
        ObjectFieldAccessExpression,
        ObjectCallExpression,
        ObjectAllocationExpression,
        ArrayAssignmentExpression,
        ArrayAccessExpression,
        CallExpression,
        ConversionExpression,
    }
}
