namespace Nebula.Core.Binding
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
        BundleFieldAssignmentExpression,
        ArrayAssignmentExpression,
        CallExpression,
        ConversionExpression,
    }
}
