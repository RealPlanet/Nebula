#pragma once

#include <string>
#include <memory>
#include <map>

#include "DiagnosticReport.h"

#include "Function.h"
#include "Bundle.h"

namespace nebula
{
	class Script;

	class GlobalVariable
	{
	public:
		GlobalVariable(const std::string& name, DataStackVariantIndex type)
			: m_sMetadataName{ name }, m_eType{ type } {
		}

		const std::string& GetName() const { return m_sMetadataName; }
		DataStackVariantIndex GetType() const { return m_eType; }

	private:
		std::string m_sMetadataName;
		DataStackVariantIndex m_eType;
	};

	using GlobalList = std::vector<GlobalVariable>;

	// A map of functions associated by function name
	using FunctionMap = std::map<std::string, Function, std::less<>>;
	// A map of bundle definition associated by bundle name
	using BundleMap = std::map<std::string, BundleDefinition, std::less<>>;

	// Contains the script and the parsing report with any messages, warnings, errors
	// reported during script load.
	//
	// Script will be null if parsing had any errors.
	struct ScriptLoadResult
	{
		shared::DiagnosticReport ParsingReport;
		Script* Script{ nullptr };
	};

	// A in-memory rapresentation of a compiled 'Nebula' script
	class Script
	{
		friend class ScriptBuilder;
	public:
		static ScriptLoadResult FromFile(const std::string& filePath);
		static ScriptLoadResult FromMemory(const std::string_view& data, const std::string& sourcePath = "");

	public:
		~Script();

		inline const std::string& Namespace() const { return m_Namespace; }
		inline const GlobalList& Globals() const { return m_Globals; }
		inline const FunctionMap& Functions() const { return m_Functions; }
		inline const BundleMap& Bundles() const { return m_Bundles; }
		inline const std::string& GetSourcePath() const { return m_SourcePath; }

	private:
		Script();
		std::string m_SourcePath;
		std::string m_Namespace;
		GlobalList  m_Globals;
		FunctionMap m_Functions;
		BundleMap   m_Bundles;
	};

	class ScriptBuilder
	{
	public:
		ScriptBuilder();
		~ScriptBuilder();

		// Get script pointer and invalidate this builder,
		// (internal ptr) is set to nullptr
		Script* Finalize() noexcept;
		Script* Get() noexcept { return m_InternalScript; }

		bool HasNamespace() const;
		bool SetNamespace(const std::string& ns);

		bool AddFunction(Function&& func);
		bool AddBundle(BundleDefinition&& bundle);
		bool AddGlobal(const std::string& name, DataStackVariantIndex type);

	private:
		Script* m_InternalScript{ nullptr };
	};
}

