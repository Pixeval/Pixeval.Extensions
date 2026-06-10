cmake_minimum_required(VERSION 3.24)

project(PixevalExtensionsPython LANGUAGES C)

find_package(Python3 REQUIRED COMPONENTS Interpreter Development.Embed)

set(PIXEV_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../..")
set(PIXEV_GENERATOR_PROJECT "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/Pixeval.Extensions.Generator.csproj")
set(PIXEV_GENERATOR_DLL "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/bin/$<CONFIG>/net8.0/Pixeval.Extensions.Generator.dll")
file(GLOB_RECURSE PIXEV_GENERATOR_SOURCES CONFIGURE_DEPENDS
    "${PIXEV_ROOT}/src/Pixeval.Extensions.Generator/*.cs"
)
set(PIXEV_IDL_FILES
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/metadata.json"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/commands.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/core.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/format-providers.pidl"
    "${PIXEV_ROOT}/src/Pixeval.Extensions.IDL/settings.pidl"
)

set(PIXEV_PYTHON_GENERATED_DIR "${CMAKE_CURRENT_BINARY_DIR}/generated/pixeval_extensions")
set(PIXEV_PYTHON_GENERATED_COMMON "${PIXEV_PYTHON_GENERATED_DIR}/abi.py")
set(PIXEV_PYTHON_GENERATED_SDK "${PIXEV_PYTHON_GENERATED_DIR}/sdk.py")
set(PIXEV_PYTHON_GENERATED_SYMBOLS "${PIXEV_PYTHON_GENERATED_DIR}/symbols.py")

add_custom_command(
    OUTPUT "${PIXEV_PYTHON_GENERATED_COMMON}" "${PIXEV_PYTHON_GENERATED_SDK}" "${PIXEV_PYTHON_GENERATED_SYMBOLS}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${PIXEV_PYTHON_GENERATED_DIR}"
    COMMAND dotnet build "${PIXEV_GENERATOR_PROJECT}" -c $<CONFIG> -p:Platform=AnyCPU -v:minimal
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" python-symbols "${PIXEV_PYTHON_GENERATED_SYMBOLS}"
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" python-common ${PIXEV_IDL_FILES} "${PIXEV_PYTHON_GENERATED_COMMON}"
    COMMAND dotnet "${PIXEV_GENERATOR_DLL}" python-sdk ${PIXEV_IDL_FILES} "${PIXEV_PYTHON_GENERATED_SDK}"
    DEPENDS "${PIXEV_GENERATOR_PROJECT}" ${PIXEV_GENERATOR_SOURCES} ${PIXEV_IDL_FILES}
    COMMENT "Generating Pixeval Python SDK from PIDL"
    VERBATIM
)

add_custom_target(Pixeval.Extensions.Python.SDK.Generated
    DEPENDS "${PIXEV_PYTHON_GENERATED_COMMON}" "${PIXEV_PYTHON_GENERATED_SDK}" "${PIXEV_PYTHON_GENERATED_SYMBOLS}"
)

add_library(Pixeval.Extensions.Python.Bootstrap SHARED
    runtime/pixeval_python_bootstrap.c
)
add_dependencies(Pixeval.Extensions.Python.Bootstrap Pixeval.Extensions.Python.SDK.Generated)
target_link_libraries(Pixeval.Extensions.Python.Bootstrap PRIVATE Python3::Python)

if(WIN32)
    set_target_properties(Pixeval.Extensions.Python.Bootstrap PROPERTIES
        OUTPUT_NAME "Pixeval.Extensions.Python.Demo"
    )
else()
    set_target_properties(Pixeval.Extensions.Python.Bootstrap PROPERTIES
        OUTPUT_NAME "pixeval_extensions_python_demo"
        PREFIX "lib"
    )
endif()
