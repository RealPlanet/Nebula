#pragma once

/*
*   General token used by the C# compiler for emission
*   and the debug script parser for loading
*   --------------------------------------------------
*   Defined here to ensure both are always aligned for each compilation
*/

namespace nebula::shared
{
    // Single character tokens

    constexpr auto EMIT_OPENBODY_TOKEN = "{";
    constexpr auto EMIT_CLOSEBODY_TOKEN = "}";
    constexpr auto ATTRIBUTE_PREFIX = ";";
    constexpr auto OPEN_PARENTHESIS = "(";
    constexpr auto CLOSE_PARENTHESIS = ")";
    constexpr auto GENERIC_MARKER_PREFIX = ".";
    constexpr auto COMMENT_PREFIX = "#";
}


