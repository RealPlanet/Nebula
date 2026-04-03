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
        EndOnNotificationStatement,
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
        ArrayInitializationExpression,
        ObjectInitializationExpression,
        ObjectFieldInitializationExpression,
        CompoundAssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        DeclarationAssignmentExpression,
        ObjectFieldAssignmentExpression,
        ObjectFieldAccessExpression,
        ObjectCallExpression,
        ArrayAssignmentExpression,
        ArrayAccessExpression,
        CallExpression,
        ConversionExpression,
        IsDefinedExpression,
    }
}
