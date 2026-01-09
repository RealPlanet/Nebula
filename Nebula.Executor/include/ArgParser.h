#pragma once

#ifndef _H_ARG_PARSER_
#define _H_ARG_PARSER_

#include <map>
#include <string>
#include <string_view>
#include <sstream>

#ifdef _PL_ARGPARSER_IMPL_

#include <algorithm> 
#include <cctype>
#include <vector>
#include <format>

namespace planet::argparser::internal {

	// Trim from the start (in place)
	inline void ltrim(std::string& s) {
		s.erase(s.begin(), std::find_if(s.begin(), s.end(), [](unsigned char ch) {
			return !std::isspace(ch);
			}));
	}

	// Trim from the end (in place)
	inline void rtrim(std::string& s) {
		s.erase(std::find_if(s.rbegin(), s.rend(), [](unsigned char ch) {
			return !std::isspace(ch);
			}).base(), s.end());
	}

	inline void trim(std::string& s) {
		rtrim(s);
		ltrim(s);
	}
}

#endif // !_PL_ARGPARSER_IMPL_

namespace planet::argparser
{
	using ArgCallbackFunc = void(*)(const std::string&);
	using ArgErrorCallbackFunc = void(*)(const std::string&);

	class ArgParser {
	public:
		ArgParser(ArgErrorCallbackFunc errCallback)
			:_errCallback{ errCallback } {
		}

		void RegisterArgument(std::string_view arg, const ArgCallbackFunc& callback);
		bool Parse(int argc, char** argv);
	private:
		std::map<std::string, ArgCallbackFunc> _valueCallbacks;
		std::map<std::string, ArgCallbackFunc> _toggleCallbacks;
		ArgErrorCallbackFunc _errCallback;
	};

#ifdef _PL_ARGPARSER_IMPL_
	void ArgParser::RegisterArgument(std::string_view arg, const ArgCallbackFunc& callback) {
		bool isValueArg = false;
		if (arg.ends_with('='))
		{
			isValueArg = true;
			arg = arg.substr(0, arg.size() - 1);
		}

		std::istringstream f(arg.data());
		std::string s;
		while (std::getline(f, s, '|')) {
			internal::trim(s);

			if (!isValueArg)
			{
				_toggleCallbacks[s] = callback;
			}
			else
			{
				_valueCallbacks[s] = callback;
			}
		}
	}

	bool ArgParser::Parse(int argc, char** argv) {
		for (int i = 1; i < argc; i++) {
			std::string arg = argv[i];
			bool foundKey = false;
			// Parsing some switches
			if (arg.starts_with('-'))
			{
				size_t aCount = arg.size() - 1;

				if (aCount == 0)
				{
					_errCallback("No switches after -");
					return false;
				}

				if (aCount > 1)
				{
					std::vector<ArgCallbackFunc> callbackFunctions;
					for (int j = 1; j < arg.size(); j++) {
						std::string strToggle = std::string{ arg[j] };
						auto it = _toggleCallbacks.find(strToggle);
						if (it == _toggleCallbacks.end())
						{
							std::string errorMessage = std::format("The toggle '{}' does not exist!", strToggle);
							_errCallback(errorMessage);
							return false;
						}

						callbackFunctions.push_back(it->second);
					}

					for (auto it : callbackFunctions) { it(""); }
					continue;
				}

				arg = std::string{ arg[1] };
				foundKey = true;
			}
		
			if (arg.starts_with("--"))
			{
				arg = arg.substr(2);
				foundKey = true;
			}

			if (!foundKey)
			{
				_errCallback(std::format("Unknown key '{}'", arg));
				return false;
			}

			auto it = _toggleCallbacks.find(arg);
			if (it != _toggleCallbacks.end())
			{
				it->second("");
				continue;
			}

			auto vit = _valueCallbacks.find(arg);
			if (vit != _valueCallbacks.end())
			{
				if (i + 1 >= argc)
				{
					_errCallback(std::format("The switch '{}' is missing an argument!", arg[0]));
				}

				i++;
				vit->second(argv[i]);
				continue;
			}

			_errCallback(std::format("The switch '{}' does not exist!", arg[1]));
			return false;
		}

		return true;
	}

#endif // !_PL_ARGPARSER_IMPL_
}

#endif //

