add_library(Pixeval.Extensions.Cpp.SDK INTERFACE)
add_library(Pixeval.Extensions.Cpp.SDK::Pixeval.Extensions.Cpp.SDK ALIAS Pixeval.Extensions.Cpp.SDK)
add_library(Pixeval.Extensions.Cpp::SDK ALIAS Pixeval.Extensions.Cpp.SDK)

set_target_properties(Pixeval.Extensions.Cpp.SDK PROPERTIES
    EXPORT_NAME SDK
)

set(PIXEV_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../../..")
set(PIXEV_GENERATOR_PROJECT "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/Pixeval.Extensions.Generator.csproj")
set(PIXEV_GENERATOR_DLL "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/bin/$<CONFIG>/net8.0/Pixeval.Extensions.Generator.dll")
file(GLOB_RECURSE PIXEV_GENERATOR_SOURCES CONFIGURE_DEPENDS
    "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/*.cs"
)
file(GLOB_RECURSE PIXEV_CPP_STATIC_HEADERS CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/include/*.hpp"
)
list(FILTER PIXEV_CPP_STATIC_HEADERS EXCLUDE REGEX "\\.g\\.hpp$")
list(FILTER PIXEV_CPP_STATIC_HEADERS EXCLUDE REGEX "/symbol\\.hpp$")
set(PIXEV_IDL_FILES
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/metadata.json"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/commands.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/core.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/format-providers.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/settings.pidl"
)
set(PIXEV_CPP_GENERATED_INCLUDE_DIR "${CMAKE_CURRENT_BINARY_DIR}/generated/include")
set(PIXEV_CPP_GENERATED_COMMON "${PIXEV_CPP_GENERATED_INCLUDE_DIR}/pixeval/extensions.common.g.hpp")
set(PIXEV_CPP_GENERATED_SDK "${PIXEV_CPP_GENERATED_INCLUDE_DIR}/pixeval/extensions.sdk.g.hpp")
set(PIXEV_CPP_GENERATED_SYMBOLS "${PIXEV_CPP_GENERATED_INCLUDE_DIR}/pixeval/symbol.hpp")

add_custom_command(
    OUTPUT "${PIXEV_CPP_GENERATED_COMMON}" "${PIXEV_CPP_GENERATED_SDK}" "${PIXEV_CPP_GENERATED_SYMBOLS}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${PIXEV_CPP_GENERATED_INCLUDE_DIR}/pixeval"
    COMMAND dotnet build "${PIXEV_GENERATOR_PROJECT}" -c $<CONFIG> -p:Platform=AnyCPU -v:minimal
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" cpp-symbols "${PIXEV_CPP_GENERATED_SYMBOLS}"
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" cpp-common ${PIXEV_IDL_FILES} "${PIXEV_CPP_GENERATED_COMMON}"
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" cpp-sdk ${PIXEV_IDL_FILES} "${PIXEV_CPP_GENERATED_SDK}"
    DEPENDS "${PIXEV_GENERATOR_PROJECT}" ${PIXEV_GENERATOR_SOURCES} ${PIXEV_CPP_STATIC_HEADERS} ${PIXEV_IDL_FILES}
    COMMENT "Generating Pixeval C++ SDK headers from PIDL"
    VERBATIM
)

add_custom_target(Pixeval.Extensions.Cpp.SDK.Generated
    DEPENDS "${PIXEV_CPP_GENERATED_COMMON}" "${PIXEV_CPP_GENERATED_SDK}" "${PIXEV_CPP_GENERATED_SYMBOLS}"
)

add_dependencies(Pixeval.Extensions.Cpp.SDK Pixeval.Extensions.Cpp.SDK.Generated)

target_compile_features(Pixeval.Extensions.Cpp.SDK INTERFACE cxx_std_20)
target_include_directories(Pixeval.Extensions.Cpp.SDK INTERFACE
    "$<BUILD_INTERFACE:${PIXEV_CPP_GENERATED_INCLUDE_DIR}>"
    "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>"
    "$<INSTALL_INTERFACE:${CMAKE_INSTALL_INCLUDEDIR}>"
)

target_compile_definitions(Pixeval.Extensions.Cpp.SDK INTERFACE
    "$<$<PLATFORM_ID:Windows>:WIN32_LEAN_AND_MEAN;NOMINMAX>"
)
target_link_libraries(Pixeval.Extensions.Cpp.SDK INTERFACE
    "$<$<PLATFORM_ID:Windows>:ole32>"
)

install(
    TARGETS Pixeval.Extensions.Cpp.SDK
    EXPORT PixevalExtensionsCppTargets
)

install(
    DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/include/"
    DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}"
    PATTERN "*.g.hpp" EXCLUDE
    PATTERN "symbol.hpp" EXCLUDE
)

install(
    FILES
        "${PIXEV_CPP_GENERATED_SYMBOLS}"
        "${PIXEV_CPP_GENERATED_COMMON}"
        "${PIXEV_CPP_GENERATED_SDK}"
    DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/pixeval"
)

install(
    EXPORT PixevalExtensionsCppTargets
    NAMESPACE Pixeval.Extensions.Cpp::
    FILE PixevalExtensionsCppTargets.cmake
    DESTINATION "${PIXEV_CPP_PACKAGE_CONFIG_DIR}"
)
