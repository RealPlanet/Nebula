namespace Nebula.Commons.Syntax
{
    public enum NodeType
    {
        Error = -1,

        // Trivia
        SkippedTextTrivia,
        WhiteSpaceTrivia,
        LinebreakTrivia,
        SingleLineCommentTrivia,
        MultiLineCommentTrivia,

        IdentifierToken,        // Any sequence of characters
        NumberToken,            // 46545646
        StringToken,            // "
        CommaToken,             // ,
        DotToken,               // .
        SemicolonToken,         // ;
        //ColonToken,             // :
        DoubleColonToken,       // ::
        OpenBracketToken,       // {
        ClosedBracketToken,     // }
        OpenSquareBracketToken, // [
        ClosedSquareBracketToken, // ]
        OpenParenthesisToken,   // (
        ClosedParenthesisToken, // )
        EqualsToken,            // =
        PlusEqualsToken,        // +=
        MinusEqualsToken,       // -=
        StarEqualsToken,        // *=
        SlashEqualsToken,       // /=
        AmpersandEqualsToken,   // &=
        PipeEqualsToken,        // |=
        HatEqualsToken,         // ^=
        StarToken,              // *
        SlashToken,             // /
        PlusToken,              // +
        MinusToken,             // -
        ModuloToken,            // %
        BangToken,              // !
        BangEqualsToken,        // !=
        DoubleEqualsToken,      // ==
        LessToken,              // <
        LessOrEqualsToken,      // <=
        GreaterToken,           // >
        GreaterOrEqualsToken,   // >=
        AmpersandToken,         // &
        DoubleAmpersandToken,   // &&
        TildeToken,             // ~
        HatToken,               // ^
        PipeToken,              // |
        DoublePipeToken,        // ||

        TrueKeyword,
        FalseKeyword,
        FuncKeyword,
        AsyncKeword,
        NamespaceKeyword,
        ImportKeyword,
        BundleKeyword,
        ConstKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        DoKeyword,
        ForKeyword,
        BreakKeyword,
        ContinueKeyword,
        ReturnKeyword,
        WaitKeyword,
        WaitNotificationKeyword,
        EndOnNotificationKeyword,
        NotifyKeyword,
        NativeKeyword,

        BlockStatement,         // Generic sequence of nodes surrounded by an open and close { }
        ExpressionStatement,
        IfStatement,
        ReturnStatement,
        WaitStatement,
        WaitNotificationStatement,
        NotifyStatement,
        ContinueStatement,
        BreakStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        ImportStatement,

        NamespaceDeclaration,
        VariableDeclaration,
        VariableDeclarationCollection,
        FunctionDeclaration,
        NativeFunctionDeclaration,
        BundleDeclaration,
        BundleFieldDeclaration,

        UnaryExpression,
        BinaryExpression,
        AssignmentExpression,
        ObjectVariableAccessExpression,
        ArrayAccessExpression,
        NameExpression,
        ParenthesizedExpression,
        CallExpression,
        ObjectCallExpression,
        LiteralExpression,
        InitializationExpression,


        EndOfFileToken,
        TypeClause,
        RankSpecifier,
        ElseClause,
        Parameter,

        LastNode,
    }
}
