#pragma once

#ifndef _H_NEBULA_SYMBOLS_SERIALIZER_
#define _H_NEBULA_SYMBOLS_SERIALIZER_

#include "json.h"
#include "json_serialization.h"

#include "DebugSymbols.h"

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::TypeInformation>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::TypeInformation& source) = delete;

	static nebula::debugger::symbols::TypeInformation Deserialize(const value& element)
	{
		nebula::debugger::symbols::TypeInformation::eKind kind = nebula::debugger::symbols::TypeInformation::eKind::Unkown;
		std::string strKind = element["Kind"];
		if (strKind == "Primitive")
		{
			kind = nebula::debugger::symbols::TypeInformation::eKind::Primitive;
		}
		else if (strKind == "Object")
		{
			kind = nebula::debugger::symbols::TypeInformation::eKind::Object;
		}
		else if (strKind == "Array")
		{
			kind = nebula::debugger::symbols::TypeInformation::eKind::Array;
		}

		nebula::debugger::symbols::TypeInformation type;
		type.kind = kind;
		type.Id = element["Id"];
		type.name = element["Name"];
		type.identifier = element["Identifier"];

		if (element.contains("ObjectNamespace"))
		{
			type.objectNamespace = element["ObjectNamespace"];
		}

		if (element.contains("ObjectName"))
		{
			type.objectNamespace = element["ObjectName"];
		}

		if (element.contains_array("Members"))
		{
			type.members = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::VariableInformation>>(element["Members"]);
		}

		return type;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::VariableInformation>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::VariableInformation& source) = delete;

	static nebula::debugger::symbols::VariableInformation Deserialize(const value& element)
	{
		nebula::debugger::symbols::VariableInformation variable;
		variable.name = element["Name"];
		variable.typeId = element["TypeId"];
		return variable;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::LineInformation>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::LineInformation& source) = delete;

	static nebula::debugger::symbols::LineInformation Deserialize(const value& element)
	{
		nebula::debugger::symbols::LineInformation line;
		line.lineNumber = element["LineNumber"];
		line.firstOpcodeOfLine = element["StartOpcodeOfLine"];
		return line;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<nebula::debugger::symbols::FunctionInformation>
{
	static ::strata::json::value Serialize(const nebula::debugger::symbols::FunctionInformation& source) = delete;

	static nebula::debugger::symbols::FunctionInformation Deserialize(const value& element)
	{
		nebula::debugger::symbols::FunctionInformation function;
		function.name = element["Name"];
		function.lineNumber = element["LineNumber"];
		function.endLineNumber = element["EndLineNumber"];
		function.instructionCount = element["InstructionCount"];
		function.parameters = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::VariableInformation>>(element["Parameters"]);
		function.locals = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::VariableInformation>>(element["LocalVariables"]);
		function.lines = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::LineInformation>>(element["Lines"]);
		return function;
	};
};

template <>
struct ::strata::json::serialization::JSerializer<::nebula::debugger::symbols::DebugSymbols>
{
	static ::strata::json::value Serialize(const ::nebula::debugger::symbols::DebugSymbols& source) = delete;

	static ::nebula::debugger::symbols::DebugSymbols Deserialize(const strata::json::value& element)
	{
		::nebula::debugger::symbols::DebugSymbols file;
		file.sourceFilePath = element["SourceFilePath"];
		file.md5Hash = element["MD5Hash"];
		file.namespace_ = element["Namespace"];

		file.globals = ::strata::json::serialization::Deserialize<std::vector<nebula::debugger::symbols::VariableInformation>>(element["Globals"]);
		file.types = ::strata::json::serialization::Deserialize<std::unordered_map<size_t, nebula::debugger::symbols::TypeInformation>>(element["Types"]);
		file.functions = ::strata::json::serialization::Deserialize<std::unordered_map<std::string, nebula::debugger::symbols::FunctionInformation>>(element["Functions"]);
		file.nativeFunctions = ::strata::json::serialization::Deserialize<std::unordered_set<std::string>>(element["NativeFunctions"]);

		return file;
	};
};

#endif // !_H_NEBULA_SYMBOLS_SERIALIZER_
