using Nebula.Commons.Text;
using System.Collections;
using System.Collections.Generic;

namespace Nebula.Commons.Reporting
{
    public enum ReportType
    {
        Information,
        Warning,
        Error,
    }

    public sealed class Report
        : IEnumerable<ReportMessage>
    {
        public int Count => Errors.Count + Messages.Count + Warnings.Count;
        public bool HasErrors => Errors.Count > 0;
        public bool HasWarnings => Warnings.Count > 0;

        public void Append(Report other)
        {
            Messages.AddRange(other.Messages);
            Warnings.AddRange(other.Warnings);
            Errors.AddRange(other.Errors);
        }

        public void PushCode(ReportMessageCodes code, string message, TextLocation where)
        {
            ReportMessage m = new(code, message, where);
            switch (m.Type)
            {
                case ReportType.Information:
                    Messages.Add(m);
                    break;
                case ReportType.Warning:
                    Warnings.Add(m);
                    break;
                case ReportType.Error:
                    Errors.Add(m);
                    break;
            }
        }

        public void PushInformation(string message, TextLocation where)
        {
            Messages.Add(new(ReportType.Information, message, where));
        }

        public void PushWarning(string message, TextLocation where)
        {
            Warnings.Add(new(ReportType.Warning, message, where));
        }

        public void PushError(string message, TextLocation where)
        {
            Errors.Add(new(ReportType.Error, message, where));
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
                    Messages.Add(message);
                    break;
                case ReportType.Warning:
                    Warnings.Add(message);
                    break;
                case ReportType.Error:
                    Errors.Add(message);
                    break;
            }
        }

        public void Clear()
        {
            Messages.Clear();
            Warnings.Clear();
            Errors.Clear();
        }

        public IEnumerator<ReportMessage> GetEnumerator()
        {
            foreach (ReportMessage error in Errors)
                yield return error;

            foreach (ReportMessage error in Warnings)
                yield return error;

            foreach (ReportMessage error in Messages)
                yield return error;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<ReportMessage> Messages { get; } = new();
        public List<ReportMessage> Warnings { get; } = new();
        public List<ReportMessage> Errors { get; } = new();
    }
}
