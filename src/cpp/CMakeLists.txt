cmake_minimum_required(VERSION 3.24)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/../Pixeval.Extensions.IDL/metadata.json" PIXEV_EXTENSIONS_METADATA)
string(JSON PIXEV_EXTENSIONS_VERSION GET "${PIXEV_EXTENSIONS_METADATA}" version)
project(PixevalExtensionsCpp VERSION "${PIXEV_EXTENSIONS_VERSION}" LANGUAGES CXX)

include(CMakePackageConfigHelpers)
include(GNUInstallDirs)

set(PIXEV_CPP_PACKAGE_CONFIG_DIR "${CMAKE_INSTALL_DATADIR}/PixevalExtensionsCpp")

add_subdirectory(Pixeval.Extensions.Cpp.SDK)
add_subdirectory(Pixeval.Extensions.Cpp.Demo)

set(PIXEV_CPP_PACKAGE_CONFIG "${CMAKE_CURRENT_BINARY_DIR}/PixevalExtensionsCppConfig.cmake")
set(PIXEV_CPP_PACKAGE_VERSION_CONFIG "${CMAKE_CURRENT_BINARY_DIR}/PixevalExtensionsCppConfigVersion.cmake")

configure_package_config_file(
    "${CMAKE_CURRENT_SOURCE_DIR}/cmake/PixevalExtensionsCppConfig.cmake.in"
    "${PIXEV_CPP_PACKAGE_CONFIG}"
    INSTALL_DESTINATION "${PIXEV_CPP_PACKAGE_CONFIG_DIR}"
)

write_basic_package_version_file(
    "${PIXEV_CPP_PACKAGE_VERSION_CONFIG}"
    VERSION "${PROJECT_VERSION}"
    COMPATIBILITY SameMajorVersion
)

install(
    FILES
        "${PIXEV_CPP_PACKAGE_CONFIG}"
        "${PIXEV_CPP_PACKAGE_VERSION_CONFIG}"
    DESTINATION "${PIXEV_CPP_PACKAGE_CONFIG_DIR}"
)

set(PIXEV_REPOSITORY_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../..")
if(EXISTS "${PIXEV_REPOSITORY_ROOT}/LICENSE")
    install(FILES "${PIXEV_REPOSITORY_ROOT}/LICENSE" DESTINATION "${PIXEV_CPP_PACKAGE_CONFIG_DIR}")
    set(CPACK_RESOURCE_FILE_LICENSE "${PIXEV_REPOSITORY_ROOT}/LICENSE")
endif()

set(CPACK_PACKAGE_NAME "pixeval-extensions-cpp")
set(CPACK_PACKAGE_VENDOR "Pixeval")
set(CPACK_PACKAGE_CONTACT "Pixeval")
set(CPACK_PACKAGE_VERSION "${PROJECT_VERSION}")
set(CPACK_PACKAGE_FILE_NAME "pixeval-extensions-cpp-sdk-${PROJECT_VERSION}")
set(CPACK_GENERATOR "ZIP")
set(CPACK_INCLUDE_TOPLEVEL_DIRECTORY ON)
include(CPack)

if(TARGET package)
    add_dependencies(package Pixeval.Extensions.Cpp.SDK.Generated)
endif()
