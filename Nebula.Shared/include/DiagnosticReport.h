#pragma once

#include <string>
#include <vector>

namespace nebula::shared
{
    enum class ReportType
    {
        Information,
        Warning,
        Error,
    };

    // General purpose container for messages
    class Report
    {
    public:
        Report(ReportType type, const std::string& message);

        const std::string& Message() const;
        ReportType Type() const;
    private:
        ReportType m_Type;
        std::string m_Message;
    };

    class DiagnosticReport
    {
    public:
        void ReportError(const std::string& message);
        void ReportWarning(const std::string& message);
        void ReportInformation(const std::string& message);

        const std::vector<Report>& Errors()				const;
        const std::vector<Report>& Warnings()			const;
        const std::vector<Report>& Information()		const;

    private:
        std::vector<Report> m_Reports;
        std::vector<Report> m_Errors;
        std::vector<Report> m_Warnings;
    };
}

