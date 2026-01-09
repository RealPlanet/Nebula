#include <string>
#include <vector>

#include "DiagnosticReport.h"

using namespace nebula::shared;

void DiagnosticReport::ReportError(const std::string& message)
{
    m_Errors.emplace_back(ReportType::Error, message);
}

void DiagnosticReport::ReportWarning(const std::string& message)
{
    m_Warnings.emplace_back(ReportType::Warning, message);
}

void DiagnosticReport::ReportInformation(const std::string& message)
{
    m_Reports.emplace_back(ReportType::Information, message);
}

const std::vector<Report>& DiagnosticReport::Errors() const
{
    return m_Errors;
}

const std::vector<Report>& DiagnosticReport::Warnings() const
{
    return m_Warnings;
}

const std::vector<Report>& DiagnosticReport::Information() const
{
    return m_Reports;
}

Report::Report(ReportType type, const std::string& message)
    : m_Type{type}, m_Message{message}
{
}

const std::string& Report::Message() const
{
    return m_Message;
}

ReportType Report::Type() const
{
    return m_Type;
}
