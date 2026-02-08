using Nebula.Commons.Text;
using System.Collections;
using System.Collections.Generic;

namespace Nebula.Commons.Reporting
{

    public sealed class Report
        : IEnumerable<ReportMessage>
    {
        public int Count => Errors.Count + Messages.Count + Warnings.Count;
        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;


        public IReadOnlyList<ReportMessage> Messages => _messages;
        private readonly List<ReportMessage> _messages = new();
        public IReadOnlyList<ReportMessage> Warnings => _warnings;
        private readonly List<ReportMessage> _warnings = new();
        public IReadOnlyList<ReportMessage> Errors => _errors;
        private readonly List<ReportMessage> _errors = new();

        public void Append(Report other)
        {
            _messages.AddRange(other.Messages);
            _warnings.AddRange(other.Warnings);
            _errors.AddRange(other.Errors);
        }

        public void PushInformation(string message, TextLocation where, string source = "")
        {
            _messages.Add(new(ReportType.Information, source, message, where));
        }

        public void PushWarning(string message, TextLocation where, string source = "")
        {
            _warnings.Add(new(ReportType.Warning, source, message, where));
        }

        public void PushError(string message, TextLocation where, string source = "")
        {
            _errors.Add(new(ReportType.Error, source, message, where));
        }

        public void PushWarning(string message)
        {
            PushWarning(message, default);
        }

        public void PushError(string message)
        {
            PushError(message, default);
        }

        public void Push(ReportMessage message)
        {
            switch (message.Type)
            {
                case ReportType.Information:
                    _messages.Add(message);
                    break;
                case ReportType.Warning:
                    _warnings.Add(message);
                    break;
                case ReportType.Error:
                    _errors.Add(message);
                    break;
            }
        }

        public void Clear()
        {
            _messages.Clear();
            _warnings.Clear();
            _errors.Clear();
        }

        public IEnumerator<ReportMessage> GetEnumerator()
        {
            foreach (ReportMessage error in Errors)
            {
                yield return error;
            }

            foreach (ReportMessage error in Warnings)
            {
                yield return error;
            }

            foreach (ReportMessage error in Messages)
            {
                yield return error;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
