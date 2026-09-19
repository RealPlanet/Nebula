#pragma once

#ifndef _H_SYMBOLS_SERIALIZER_
#define _H_SYMBOLS_SERIALIZER_

#include "json.h"
#include "json_serialization.h"

#include "DebugLine.h"
#include "DebugVariable.h"
#include "DebugFunction.h"
#include "DebugBundleDefinition.h"
#include "DebugFile.h"

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::DebugVariable>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::DebugVariable& source) = delete;

	static nebula::debugger::symbols::DebugVariable Deserialize(const value& element)
	{
		nebula::debugger::symbols::DebugVariable variable;
		variable.name = element["Name"];
		if (element.contains("SourceNamespace"))
		{
			variable.sourceNamespace = element["SourceNamespace"];
		}

		variable.sourceType = element["SourceType"];
		variable.internalType = element["InternalType"];
		return variable;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::DebugLine>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::DebugLine& source) = delete;

	static nebula::debugger::symbols::DebugLine Deserialize(const value& element)
	{
		nebula::debugger::symbols::DebugLine line;
		line.lineNumber = element["LineNumber"];
		line.firstOpcodeOfLine = element["StartOpcodeOfLine"];
		return line;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::DebugFunction>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::DebugFunction& source) = delete;

	static nebula::debugger::symbols::DebugFunction Deserialize(const value& element)
	{
		nebula::debugger::symbols::DebugFunction function;
		function.name = element["Name"];
		function.lineNumber = element["LineNumber"];
		function.endLineNumber = element["EndLineNumber"];
		function.instructionCount = element["InstructionCount"];
		function.parameters = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::DebugVariable>>(element["Parameters"]);
		function.locals = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::DebugVariable>>(element["LocalVariables"]);
		function.lines = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::DebugLine>>(element["Lines"]);
		function.statements = ::strata::json::serialization::Deserialize<std::vector<int>>(element["Statements"]);
		return function;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::DebugBundleDefinition>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::DebugBundleDefinition& source) = delete;

	static nebula::debugger::symbols::DebugBundleDefinition Deserialize(const value& element)
	{
		nebula::debugger::symbols::DebugBundleDefinition bundle;
		bundle.name = element["Name"];
		bundle.fields = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::DebugVariable>>(element["Fields"]);
		return bundle;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::DebugFile>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::DebugFile& source) = delete;

	static nebula::debugger::symbols::DebugFile Deserialize(const strata::json::value& element)
	{
		::nebula::debugger::symbols::DebugFile file;
		file.namespace_ = element["Namespace"];
		file.originalFileName = element["OriginalFileName"];
		file.originalFileFullName = element["OriginalFileFullName"];
		file.md5Hash = element["MD5Hash"];

		file.bundles = ::strata::json::serialization::Deserialize<std::unordered_map<std::string, nebula::debugger::symbols::DebugBundleDefinition>>(element["Bundles"]);
		file.functions = ::strata::json::serialization::Deserialize<std::unordered_map<std::string, nebula::debugger::symbols::DebugFunction>>(element["Functions"]);
		file.nativeFunctions = ::strata::json::serialization::Deserialize<std::unordered_set<std::string>>(element["NativeFunctions"]);

		return file;
	};
};

#endif // !_H_SYMBOLS_SERIALIZER_
