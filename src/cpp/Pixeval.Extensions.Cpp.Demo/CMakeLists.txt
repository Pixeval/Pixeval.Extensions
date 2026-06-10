set(PIXEV_DEMO_LOGO_SOURCE "${CMAKE_CURRENT_BINARY_DIR}/generated/logo.cpp")
file(MAKE_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/generated")
file(READ "${CMAKE_CURRENT_SOURCE_DIR}/assets/logo.png" PIXEV_DEMO_LOGO_HEX HEX)
string(REGEX REPLACE "([0-9A-Fa-f][0-9A-Fa-f])" "0x\\1, " PIXEV_DEMO_LOGO_BYTES "${PIXEV_DEMO_LOGO_HEX}")
file(WRITE "${PIXEV_DEMO_LOGO_SOURCE}"
    "#include <cstddef>\n"
    "#include <cstdint>\n\n"
    "namespace pixeval::extensions::demo_assets\n"
    "{\n"
    "    extern const std::uint8_t logo_png[] = {${PIXEV_DEMO_LOGO_BYTES}};\n"
    "    extern const std::size_t logo_png_size = sizeof(logo_png);\n"
    "}\n")

add_library(Pixeval.Extensions.CppDemo SHARED
    src/demo.cpp
    "${PIXEV_DEMO_LOGO_SOURCE}"
)

target_link_libraries(Pixeval.Extensions.CppDemo PRIVATE Pixeval.Extensions.Cpp.SDK::Pixeval.Extensions.Cpp.SDK)

if(MSVC)
    target_compile_options(Pixeval.Extensions.CppDemo PRIVATE /W4 /permissive-)
endif()

if(NOT PIXEV_ARCH_DIR)
    string(TOLOWER "${CMAKE_SYSTEM_PROCESSOR}" PIXEV_SYSTEM_PROCESSOR)
    if(PIXEV_SYSTEM_PROCESSOR MATCHES "^(arm64|aarch64)$")
        set(PIXEV_ARCH_DIR arm64)
    elseif(CMAKE_SIZEOF_VOID_P EQUAL 8)
        set(PIXEV_ARCH_DIR x64)
    else()
        set(PIXEV_ARCH_DIR x86)
    endif()
endif()

set_target_properties(Pixeval.Extensions.CppDemo PROPERTIES
    OUTPUT_NAME "Pixeval.Extensions.CppDemo"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/bin/${PIXEV_ARCH_DIR}/$<CONFIG>"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/bin/${PIXEV_ARCH_DIR}/$<CONFIG>"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/bin/${PIXEV_ARCH_DIR}/$<CONFIG>"
)
