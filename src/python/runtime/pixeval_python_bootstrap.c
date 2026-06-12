#define PY_SSIZE_T_CLEAN
#ifndef _GNU_SOURCE
#define _GNU_SOURCE
#endif
#include <Python.h>

#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <wchar.h>

#if defined(_WIN32)
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#define PIXEV_EXPORT __declspec(dllexport)
#define PIXEV_CALL __stdcall
#else
#include <dlfcn.h>
#include <limits.h>
#include <stdlib.h>
#include <string.h>
#define PIXEV_EXPORT __attribute__((visibility("default")))
#define PIXEV_CALL
#endif

#define PIXEV_S_OK ((int32_t) 0)
#define PIXEV_E_POINTER ((int32_t) 0x80004003u)
#define PIXEV_E_FAIL ((int32_t) 0x80004005u)

#if defined(_WIN32)
static HMODULE g_module;

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID reserved)
{
    (void) reserved;
    if (reason == DLL_PROCESS_ATTACH)
        g_module = module;
    return TRUE;
}
#endif

static int get_module_directory(wchar_t* buffer, size_t buffer_count)
{
    if (buffer == NULL || buffer_count == 0)
        return 0;

#if defined(_WIN32)
    DWORD length = GetModuleFileNameW(g_module, buffer, (DWORD) buffer_count);
    if (length == 0 || length >= buffer_count)
        return 0;

    for (wchar_t* current = buffer + length; current != buffer; --current)
    {
        if (*current == L'\\' || *current == L'/')
        {
            *current = L'\0';
            return 1;
        }
    }

    return 0;
#else
    Dl_info info;
    if (dladdr((void*) get_module_directory, &info) == 0 || info.dli_fname == NULL)
        return 0;

    char path[PATH_MAX];
    strncpy(path, info.dli_fname, sizeof(path) - 1);
    path[sizeof(path) - 1] = '\0';

    char* slash = strrchr(path, '/');
    if (slash == NULL)
        return 0;
    *slash = '\0';

    size_t written = mbstowcs(buffer, path, buffer_count - 1);
    if (written == (size_t) -1)
        return 0;
    buffer[written] = L'\0';
    return 1;
#endif
}

static int combine_path(wchar_t* destination, size_t destination_count, const wchar_t* directory, const wchar_t* file_name)
{
    if (destination == NULL || directory == NULL || file_name == NULL)
        return 0;

#if defined(_WIN32)
    int written = _snwprintf_s(destination, destination_count, _TRUNCATE, L"%ls\\%ls", directory, file_name);
    return written > 0;
#else
    int written = swprintf(destination, destination_count, L"%ls/%ls", directory, file_name);
    return written > 0 && (size_t) written < destination_count;
#endif
}

static int copy_path(wchar_t* destination, size_t destination_count, const wchar_t* path)
{
    if (destination == NULL || destination_count == 0 || path == NULL)
        return 0;

    size_t length = wcslen(path);
    if (length >= destination_count)
        return 0;

    wmemcpy(destination, path, length + 1);
    return 1;
}

static int is_absolute_path(const wchar_t* path)
{
    if (path == NULL || path[0] == L'\0')
        return 0;

#if defined(_WIN32)
    if (path[0] == L'\\' || path[0] == L'/')
        return 1;

    wchar_t drive = path[0];
    int has_drive = (drive >= L'A' && drive <= L'Z') || (drive >= L'a' && drive <= L'z');
    return has_drive && path[1] == L':' && (path[2] == L'\\' || path[2] == L'/');
#else
    return path[0] == L'/';
#endif
}

static int resolve_home_path(wchar_t* destination, size_t destination_count, const wchar_t* module_directory, const wchar_t* configured_home)
{
    if (configured_home == NULL || configured_home[0] == L'\0')
        return copy_path(destination, destination_count, module_directory);

    if (is_absolute_path(configured_home))
        return copy_path(destination, destination_count, configured_home);

    return combine_path(destination, destination_count, module_directory, configured_home);
}

static wchar_t* read_first_line(const wchar_t* path)
{
#if defined(_WIN32)
    FILE* file = NULL;
    if (_wfopen_s(&file, path, L"rb") != 0 || file == NULL)
        return NULL;
#else
    char narrow_path[PATH_MAX];
    size_t converted = wcstombs(narrow_path, path, sizeof(narrow_path) - 1);
    if (converted == (size_t) -1)
        return NULL;
    narrow_path[converted] = '\0';
    FILE* file = fopen(narrow_path, "rb");
    if (file == NULL)
        return NULL;
#endif

    char bytes[4096];
    size_t count = fread(bytes, 1, sizeof(bytes) - 1, file);
    fclose(file);
    if (count == 0)
        return NULL;

    bytes[count] = '\0';
    char* newline = strpbrk(bytes, "\r\n");
    if (newline != NULL)
        *newline = '\0';

    wchar_t* wide = Py_DecodeLocale(bytes, NULL);
    return wide;
}

static int insert_sys_path(const wchar_t* path)
{
    PyObject* sys_path = PySys_GetObject("path");
    if (sys_path == NULL)
        return 0;

    PyObject* item = PyUnicode_FromWideChar(path, -1);
    if (item == NULL)
        return 0;

    int result = PyList_Insert(sys_path, 0, item);
    Py_DECREF(item);
    return result == 0;
}

static int ensure_python_initialized(const wchar_t* module_directory)
{
    if (Py_IsInitialized())
        return 1;

    PyConfig config;
    PyConfig_InitPythonConfig(&config);
    config.isolated = 0;
    config.use_environment = 1;

    PyStatus status = PyConfig_SetString(&config, &config.program_name, L"Pixeval.Extensions.Python");
    if (PyStatus_Exception(status))
        goto fail;

    wchar_t python_home[4096];
    if (!copy_path(python_home, sizeof(python_home) / sizeof(python_home[0]), module_directory))
        goto fail;

    wchar_t home_path[4096];
    if (combine_path(home_path, sizeof(home_path) / sizeof(home_path[0]), module_directory, L"pixeval_python_home.txt"))
    {
        wchar_t* configured_home = read_first_line(home_path);
        if (configured_home != NULL)
        {
            if (!resolve_home_path(python_home, sizeof(python_home) / sizeof(python_home[0]), module_directory, configured_home))
            {
                PyMem_RawFree(configured_home);
                goto fail;
            }

            PyMem_RawFree(configured_home);
        }
    }

    status = PyConfig_SetString(&config, &config.home, python_home);
    if (PyStatus_Exception(status))
        goto fail;

    status = Py_InitializeFromConfig(&config);
    if (PyStatus_Exception(status))
        goto fail;

    PyConfig_Clear(&config);
    return 1;

fail:
    PyConfig_Clear(&config);
    (void) status;
    return 0;
}

PIXEV_EXPORT int32_t PIXEV_CALL GetExtensionsHost(void** result)
{
    if (result == NULL)
        return PIXEV_E_POINTER;

    *result = NULL;

    wchar_t module_directory[4096];
    if (!get_module_directory(module_directory, sizeof(module_directory) / sizeof(module_directory[0])))
        return PIXEV_E_FAIL;

    if (!ensure_python_initialized(module_directory))
        return PIXEV_E_FAIL;

    PyGILState_STATE gil = PyGILState_Ensure();
    int32_t hr = PIXEV_E_FAIL;

    if (!insert_sys_path(module_directory))
        goto cleanup;

    PyObject* module_name = PyUnicode_FromString("pixeval_extension_host");
    if (module_name == NULL)
        goto cleanup;

    PyObject* module = PyImport_Import(module_name);
    Py_DECREF(module_name);
    if (module == NULL)
        goto cleanup;

    PyObject* function = PyObject_GetAttrString(module, "get_extensions_host");
    Py_DECREF(module);
    if (function == NULL)
        goto cleanup;

    PyObject* pointer_value = PyObject_CallNoArgs(function);
    Py_DECREF(function);
    if (pointer_value == NULL)
        goto cleanup;

    void* host = PyLong_AsVoidPtr(pointer_value);
    Py_DECREF(pointer_value);
    if (host == NULL && PyErr_Occurred())
        goto cleanup;

    *result = host;
    hr = PIXEV_S_OK;

cleanup:
    if (hr != PIXEV_S_OK && PyErr_Occurred())
        PyErr_Print();
    PyGILState_Release(gil);
    return hr;
}
