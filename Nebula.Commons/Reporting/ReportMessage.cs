using Nebula.Commons.Text;

namespace Nebula.Commons.Reporting
{
    /// <summary>
    /// TODO :: This class should provide a identity location to avoid null reference exception (Location.Text not being set)
    /// </summary>
    public class ReportMessage
    {
        public bool IsWarning => Type == ReportType.Warning;
        public bool IsError => Type == ReportType.Error;

        public ReportType Type { get; }

        public string Source { get; }

        public string Message { get; }

        public TextLocation Location { get; }

        public ReportMessage(ReportType type, string source, string message, TextLocation view)
        {
            Type = type;
            Message = message;
            Location = view;
            Source = source;
        }

        public ReportMessage(ReportType type, string message, TextLocation view)
            : this(type, string.Empty, message, view)
        {
        }

        public override string ToString() => Message;
    }
}
