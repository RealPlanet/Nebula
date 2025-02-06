using Nebula.Commons.Reporting.Strings;

namespace Nebula.Commons.Reporting
{
    using ReportMessageTemplate = (ReportMessageCodes Code, string MessageTemplate);

    public enum ReportMessageCodes
    {
        /// <summary>Default code to use with custom messages</summary>
        Unknown = -1,
        /// <summary></summary>
        I0000 = ReportMessageProvider.InfoOffset + 0,

        /// <summary>NamespaceNotSet</summary>
        W0000 = ReportMessageProvider.WarningOffset + 0,
        /// <summary>UnreachableCodeDetected</summary>
        W0001 = ReportMessageProvider.WarningOffset + 1,

        /// <summary></summary>
        E0000 = ReportMessageProvider.ErrorOffset + 0,
        /// <summary></summary>
        E0001 = ReportMessageProvider.ErrorOffset + 1,
        /// <summary></summary>
        E0002 = ReportMessageProvider.ErrorOffset + 2,
        /// <summary></summary>
        E0003 = ReportMessageProvider.ErrorOffset + 3,
        /// <summary></summary>
        E0004 = ReportMessageProvider.ErrorOffset + 4,
        /// <summary></summary>
        E0005 = ReportMessageProvider.ErrorOffset + 5,
        /// <summary></summary>
        E0006 = ReportMessageProvider.ErrorOffset + 6,
        /// <summary></summary>
        E0007 = ReportMessageProvider.ErrorOffset + 7,
        /// <summary></summary>
        E0008 = ReportMessageProvider.ErrorOffset + 8,
        /// <summary></summary>
        E0009 = ReportMessageProvider.ErrorOffset + 9,
        /// <summary></summary>
        E000A = ReportMessageProvider.ErrorOffset + 10,
        /// <summary></summary>
        E000B = ReportMessageProvider.ErrorOffset + 11,
        /// <summary></summary>
        E000C = ReportMessageProvider.ErrorOffset + 12,
        /// <summary></summary>
        E000D = ReportMessageProvider.ErrorOffset + 13,
        /// <summary></summary>
        E000E = ReportMessageProvider.ErrorOffset + 14,
        /// <summary></summary>
        E000F = ReportMessageProvider.ErrorOffset + 15,
        /// <summary></summary>
        E0010 = ReportMessageProvider.ErrorOffset + 16,
        /// <summary></summary>
        E0011 = ReportMessageProvider.ErrorOffset + 17,
        /// <summary></summary>
        E0012 = ReportMessageProvider.ErrorOffset + 18,
        /// <summary></summary>
        E0013 = ReportMessageProvider.ErrorOffset + 19,
        /// <summary></summary>
        E0014 = ReportMessageProvider.ErrorOffset + 20,
        /// <summary></summary>
        E0015 = ReportMessageProvider.ErrorOffset + 21,
        /// <summary></summary>
        E0016 = ReportMessageProvider.ErrorOffset + 22,
        /// <summary></summary>
        E0017 = ReportMessageProvider.ErrorOffset + 23,
        /// <summary></summary>
        E0018 = ReportMessageProvider.ErrorOffset + 24,
        /// <summary></summary>
        E0019 = ReportMessageProvider.ErrorOffset + 25,
        /// <summary></summary>
        E001A = ReportMessageProvider.ErrorOffset + 26,
        /// <summary></summary>
        E001B = ReportMessageProvider.ErrorOffset + 27,
        /// <summary>VariableAlreadyDeclared</summary>
        E001C = ReportMessageProvider.ErrorOffset + 28,
        /// <summary></summary>
        E001D = ReportMessageProvider.ErrorOffset + 29,
        /// <summary></summary>
        E001E = ReportMessageProvider.ErrorOffset + 30,
        /// <summary></summary>
        E001F = ReportMessageProvider.ErrorOffset + 31,
        /// <summary>BinderFunctionAlreadyExists</summary>
        E0020 = ReportMessageProvider.ErrorOffset + 32,
        /// <summary></summary>
        E0021 = ReportMessageProvider.ErrorOffset + 33,
        /// <summary></summary>
        E0022 = ReportMessageProvider.ErrorOffset + 34,
        /// <summary></summary>
        E0023 = ReportMessageProvider.ErrorOffset + 35,
        /// <summary></summary>
        E0024 = ReportMessageProvider.ErrorOffset + 36,
        /// <summary></summary>
        E0025 = ReportMessageProvider.ErrorOffset + 37,
        /// <summary></summary>
        E0026 = ReportMessageProvider.ErrorOffset + 38,
        /// <summary></summary>
        E0027 = ReportMessageProvider.ErrorOffset + 39,
        /// <summary></summary>
        E0028 = ReportMessageProvider.ErrorOffset + 40,
    }

    /// <summary>
    /// Provides tuples of each report message with their own code
    /// </summary>
    public static class ReportMessageProvider
    {
        public const int InfoOffset = 0;
        public const int WarningOffset = 5000;
        public const int ErrorOffset = 10000;

        #region Warnings
        public static ReportMessageTemplate WarningNamespaceNotSet =>
    (ReportMessageCodes.W0000, WarningMessages.NamespaceNotSet);
        public static ReportMessageTemplate WarningUnreachableCodeDetected =>
    (ReportMessageCodes.W0001, WarningMessages.UnreachableCodeDetected);
        #endregion

        #region Errors
        public static ReportMessageTemplate ErrorUnexpectedGlobalStatement =>
    (ReportMessageCodes.E0000, ErrorMessages.UnexpectedGlobalStatement);
        public static ReportMessageTemplate ErrorUnterminatedStringLiteral =>
    (ReportMessageCodes.E0001, ErrorMessages.UnterminatedStringLiteral);
        public static ReportMessageTemplate ErrorVoidFunctionCannotReturnValue =>
    (ReportMessageCodes.E0002, ErrorMessages.VoidFunctionCannotReturnValue);
        public static ReportMessageTemplate ErrorNotAllPathsReturn =>
    (ReportMessageCodes.E0003, ErrorMessages.NotAllPathsReturn);
        public static ReportMessageTemplate ErrorFunctionExpectsReturn =>
    (ReportMessageCodes.E0004, ErrorMessages.FunctionExpectsReturn);
        public static ReportMessageTemplate ErrorNamespaceAlreadySet =>
    (ReportMessageCodes.E0005, ErrorMessages.NamespaceAlreadySet);
        public static ReportMessageTemplate ErrorNamespaceMustBeFirstOfAny =>
    (ReportMessageCodes.E0006, ErrorMessages.NamespaceMustBeFirstOfAny);
        public static ReportMessageTemplate ErrorBundleAlreadyExists =>
    (ReportMessageCodes.E0007, ErrorMessages.BundleAlreadyExists);
        public static ReportMessageTemplate ErrorBadCharacterInput =>
    (ReportMessageCodes.E0008, ErrorMessages.BadCharacterInput);
        public static ReportMessageTemplate ErrorUnterminatedMultilineComment =>
    (ReportMessageCodes.E0009, ErrorMessages.UnterminatedMultilineComment);
        public static ReportMessageTemplate ErrorCannotReassignReadonlyVariable =>
    (ReportMessageCodes.E000A, ErrorMessages.CannotReassignReadonlyVariable);
        public static ReportMessageTemplate ErrorNativeFunctionAlreadyExists =>
    (ReportMessageCodes.E000B, ErrorMessages.NativeFunctionAlreadyExists);
        public static ReportMessageTemplate ErrorParameterAlreadyDeclared =>
    (ReportMessageCodes.E000C, ErrorMessages.ParameterAlreadyDeclared);
        public static ReportMessageTemplate ErrorFieldAlreadyDeclared =>
    (ReportMessageCodes.E000D, ErrorMessages.FieldAlreadyDeclared);
        public static ReportMessageTemplate ErrorExpressionDoesNotEvaluateToType =>
    (ReportMessageCodes.E000E, ErrorMessages.ExpressionDoesNotEvaluateToType);
        public static ReportMessageTemplate ErrorNameIsNotAVariable =>
    (ReportMessageCodes.E000F, ErrorMessages.NameIsNotAVariable);
        public static ReportMessageTemplate ErrorIdentifierIsNotAFunction =>
    (ReportMessageCodes.E0010, ErrorMessages.IdentifierIsNotAFunction);
        public static ReportMessageTemplate ErrorFieldDoesNotExist =>
    (ReportMessageCodes.E0011, ErrorMessages.FieldDoesNotExist);
        public static ReportMessageTemplate ErrorBundleDoesNotExist =>
    (ReportMessageCodes.E0012, ErrorMessages.BundleDoesNotExist);
        public static ReportMessageTemplate ErrorVariableDoesNotHaveAnyFields =>
    (ReportMessageCodes.E0013, ErrorMessages.VariableDoesNotHaveAnyFields);
        public static ReportMessageTemplate ErrorVariableDoesNotExists =>
    (ReportMessageCodes.E0014, ErrorMessages.VariableDoesNotExists);
        public static ReportMessageTemplate ErrorBinaryOperatorNotDefined =>
    (ReportMessageCodes.E0015, ErrorMessages.BinaryOperatorNotDefined);
        public static ReportMessageTemplate ErrorUnaryOperatorNotDefined =>
    (ReportMessageCodes.E0016, ErrorMessages.UnaryOperatorNotDefined);
        public static ReportMessageTemplate ErrorValueNotOfType =>
    (ReportMessageCodes.E0017, ErrorMessages.ValueNotOfType);
        public static ReportMessageTemplate ErrorWrongNumberOfArguments =>
    (ReportMessageCodes.E0018, ErrorMessages.WrongNumberOfArguments);
        public static ReportMessageTemplate ErrorFunctionDoesNotExists =>
    (ReportMessageCodes.E0019, ErrorMessages.FunctionDoesNotExists);
        public static ReportMessageTemplate ErrorCannotConvertTypeImplicity =>
    (ReportMessageCodes.E001A, ErrorMessages.CannotConvertTypeImplicity);
        public static ReportMessageTemplate ErrorCannotConvertType =>
    (ReportMessageCodes.E001B, ErrorMessages.CannotConvertType);
        public static ReportMessageTemplate ErrorVariableAlreadyDeclared =>
    (ReportMessageCodes.E001C, ErrorMessages.VariableAlreadyDeclared);
        public static ReportMessageTemplate ErrorAllPathsMustReturn =>
    (ReportMessageCodes.E001D, ErrorMessages.AllPathsMustReturn);
        public static ReportMessageTemplate ErrorExpressionMustHaveValue =>
    (ReportMessageCodes.E001E, ErrorMessages.ExpressionMustHaveValue);
        public static ReportMessageTemplate ErrorFunctionAlreadyExists =>
    (ReportMessageCodes.E001F, ErrorMessages.FunctionAlreadyExists);
        public static ReportMessageTemplate ErrorBinderFunctionAlreadyExists =>
    (ReportMessageCodes.E0020, ErrorMessages.BinderFunctionAlreadyExists);
        public static ReportMessageTemplate ErrorAttributeRequiresVoidReturnType =>
    (ReportMessageCodes.E0021, ErrorMessages.AttributeRequiresVoidReturnType);
        public static ReportMessageTemplate ErrorAttributeRequiresZeroParameters =>
    (ReportMessageCodes.E0022, ErrorMessages.AttributeRequiresZeroParameters);
        public static ReportMessageTemplate ErrorCannotBindParameter =>
    (ReportMessageCodes.E0023, ErrorMessages.CannotBindParameter);
        public static ReportMessageTemplate ErrorTypeDoesNotExist =>
    (ReportMessageCodes.E0024, ErrorMessages.TypeDoesNotExist);
        public static ReportMessageTemplate ErrorUnexpectedToken =>
    (ReportMessageCodes.E0025, ErrorMessages.UnexpectedToken);
        public static ReportMessageTemplate ErrorInvalidBreakOrContinue =>
    (ReportMessageCodes.E0026, ErrorMessages.InvalidBreakOrContinue);
        public static ReportMessageTemplate ErrorInvalidExpressionStatement =>
    (ReportMessageCodes.E0027, ErrorMessages.InvalidExpressionStatement);
        public static ReportMessageTemplate ErrorFloatNoMarker =>
    (ReportMessageCodes.E0028, ErrorMessages.FloatNoMarker);

        #endregion
    }
}
