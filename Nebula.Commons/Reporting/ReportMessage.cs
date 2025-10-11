using Nebula.Commons.Text;

namespace Nebula.Commons.Reporting
{
    /// <summary>
    /// TODO :: This class should provide a identity location to avoid null reference exception (Location.Text not being set)
    /// </summary>
    public sealed class ReportMessage
    {
        public bool IsWarning => Type == ReportType.Warning;
        public bool IsError => Type == ReportType.Error;

        public ReportType Type { get; }

        public ReportMessageCodes Code { get; }

        public string Message { get; }
        public TextLocation Location { get; }

        public ReportMessage(ReportType type, string message, TextLocation view)
            : this(ReportMessageCodes.Unknown, type, message, view)
        {

        }

        public ReportMessage(ReportMessageCodes code, string message, TextLocation view)
            : this(code, TypeFromCode(code), message, view)
        {


        }

        public ReportMessage(ReportMessageCodes code, ReportType type, string message, TextLocation view)
        {
            Code = code;
            Type = type;
            Message = message;
            Location = view;
        }

        private static ReportType TypeFromCode(ReportMessageCodes code)
        {
            string codeString = code.ToString();
            ReportType type = ReportType.Information;
            if (codeString[0] == 'W')
            {
                type = ReportType.Warning;
            }
            else if (codeString[0] == 'E')
            {
                type = ReportType.Error;
            }

            return type;
        }

        public override string ToString() => Message;
    }
}
