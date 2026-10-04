#pragma once

#ifndef _H_NEBULA_DEBUG_FILE_
#define _H_NEBULA_DEBUG_FILE_

#include <memory>
#include <string>
#include <vector>
#include <unordered_set>
#include <unordered_map>
#include <cstdint>

namespace nebula::debugger::symbols
{
	using TypeId = size_t;

	struct LineInformation
	{
		size_t lineNumber{};
		size_t firstOpcodeOfLine{};
	};

	struct ValueInformation
	{
		std::string name{};
		TypeId typeId{};
	};

	struct TypeInformation
	{
		enum class eKind
		{
			Unkown,
			Object,
			Array,
			Primitive,
		};

		eKind kind;
		TypeId arrayTypeId;
		std::string objectNamespace{};
		std::string objectName{};
		std::vector<ValueInformation> members{};
		// lookup id in the parent debug file type information
		TypeId Id;
		// Readable name of this type
		std::string name{};
		// variant id of the type (ie int, float, object ... )
		short identifier;
	};

	struct FunctionInformation
	{
	public:
		static const size_t NoLineInfo{ (size_t)-1 };

		std::string name{ };
		// 0-based line number
		size_t lineNumber{ 0 };
		// 0-based line number
		size_t endLineNumber{ 0 };
		size_t instructionCount{ 0 };

		std::vector<ValueInformation> parameters{};
		std::vector<ValueInformation> locals{};
		std::vector<LineInformation> lines{};

	public:
		size_t GetLineFromOpcode(size_t opcode) const
		{
			if (opcode < 0)
			{
				return NoLineInfo;
			}

			size_t lastPossibleLine = 0;
			for (auto& kvp : lines)
			{
				size_t lineno = kvp.lineNumber;
				size_t startingOpcodeOfLine = kvp.firstOpcodeOfLine;

				if (startingOpcodeOfLine == opcode) {
					return lineno;
				}

				if (startingOpcodeOfLine > opcode) {
					return lastPossibleLine;
				}

				lastPossibleLine = lineno;
			}

			return NoLineInfo;
		}

		size_t GetOpcodeFromLine(size_t targetLine) const
		{
			size_t count = 0;
			size_t lastOpcode{ 0 };
			for (auto& line : lines)
			{
				auto lineNo = line.lineNumber;
				if (lineNo == targetLine)
				{
					return line.firstOpcodeOfLine;
				}

				count++;
				if (lineNo <= targetLine)
				{
					lastOpcode = line.firstOpcodeOfLine;
					continue;
				}

				break;
			}

			return lastOpcode;
		}
	};

	struct DebugSymbols
	{
		std::string sourceFilePath{};
		std::string md5Hash{};
		std::string namespace_{};

		std::vector<ValueInformation> globals{};
		std::unordered_map<TypeId, TypeInformation> types{};
		std::unordered_map<std::string, FunctionInformation> functions{};
		std::unordered_set<std::string> nativeFunctions{};

		inline const FunctionInformation* GetFunction(const std::string& name) const
		{
			auto it = functions.find(name);
			if (it != functions.end())
			{
				return &it->second;
			}

			return nullptr;
		}

		inline const TypeInformation* GetType(TypeId id) const
		{
			auto it = types.find(id);
			if (it != types.end())
			{
				return &it->second;
			}

			return nullptr;
		}

		inline const TypeInformation* GetType(const std::string& className) const
		{
			for(auto& [ typeId, type ] : types)
			{
				if (type.objectName == className)
				{
					return &type;
				}
			}

			return nullptr;
		}
	};
}

#endif // !_H_SCRIPT_DBG_INFORMATION_
