#pragma once

#include <atomic>
#include <coroutine>
#include <cstdint>
#include <cstring>
#include <cstdlib>
#include <exception>
#include <limits>
#include <optional>
#include <source_location>
#include <span>
#include <string>
#include <string_view>
#include <utility>
#include <vector>

#if defined(_WIN32)
#include <Windows.h>
#include <objbase.h>
#define PIXEV_CALL STDMETHODCALLTYPE
#define PIXEV_EXPORT extern "C" __declspec(dllexport)
#else
#define PIXEV_CALL
#define PIXEV_EXPORT extern "C" __attribute__((visibility("default")))
#endif

namespace pixeval::extensions
{
#if defined(_WIN32)
    using hresult = HRESULT;
    using ulong = ULONG;
#else
    using hresult = std::int32_t;
    using ulong = std::uint32_t;
    constexpr hresult S_OK = 0;
    constexpr hresult E_POINTER = static_cast<hresult>(0x80004003);
    constexpr hresult E_NOINTERFACE = static_cast<hresult>(0x80004002);
    constexpr hresult E_OUTOFMEMORY = static_cast<hresult>(0x8007000E);
    constexpr hresult E_FAIL = static_cast<hresult>(0x80004005);
#endif

    using utf16_char = char16_t;

    enum class SeekOrigin : std::int32_t
    {
        Begin = 0,
        Current = 1,
        End = 2
    };

    struct DateTimeOffsetValue
    {
        std::int64_t utc_date_time_ticks;
        std::int32_t minutes_offset;
    };

    struct guid
    {
        std::uint32_t data1;
        std::uint16_t data2;
        std::uint16_t data3;
        std::uint8_t data4[8];
    };

    [[nodiscard]] inline bool operator==(const guid& left, const guid& right) noexcept
    {
        return std::memcmp(&left, &right, sizeof(guid)) == 0;
    }

    [[nodiscard]] inline bool operator!=(const guid& left, const guid& right) noexcept
    {
        return !(left == right);
    }

    namespace abi
    {
        using query_interface_fn = hresult(PIXEV_CALL*)(void*, const guid&, void**);
        using add_ref_fn = ulong(PIXEV_CALL*)(void*);
        using release_fn = ulong(PIXEV_CALL*)(void*);

        struct unknown_vtable
        {
            query_interface_fn query_interface;
            add_ref_fn add_ref;
            release_fn release;
        };

        struct unknown_object
        {
            unknown_vtable* vtable;
        };

        [[nodiscard]] inline void** vtable_from_native(void* value) noexcept
        {
            return value == nullptr ? nullptr : reinterpret_cast<void**>(static_cast<unknown_object*>(value)->vtable);
        }

        [[nodiscard]] inline void* allocate_bytes(std::size_t bytes) noexcept
        {
            if (bytes == 0)
                return nullptr;
#if defined(_WIN32)
            return CoTaskMemAlloc(bytes);
#else
            return std::malloc(bytes);
#endif
        }

        inline void free_bytes(void* value) noexcept
        {
            if (value == nullptr)
                return;
#if defined(_WIN32)
            CoTaskMemFree(value);
#else
            std::free(value);
#endif
        }

        [[nodiscard]] inline hresult copy_utf16(std::u16string_view value, utf16_char** result) noexcept
        {
            if (result == nullptr)
                return E_POINTER;

            *result = nullptr;
            auto bytes = (value.size() + 1) * sizeof(utf16_char);
            auto buffer = static_cast<utf16_char*>(allocate_bytes(bytes));
            if (buffer == nullptr)
                return E_OUTOFMEMORY;

            std::memcpy(buffer, value.data(), value.size() * sizeof(utf16_char));
            buffer[value.size()] = u'\0';
            *result = buffer;
            return S_OK;
        }

        [[nodiscard]] inline hresult copy_optional_utf16(const std::optional<std::u16string>& value, utf16_char** result) noexcept
        {
            if (result == nullptr)
                return E_POINTER;

            *result = nullptr;
            if (!value)
                return S_OK;

            return copy_utf16(*value, result);
        }

        [[nodiscard]] inline std::u16string to_u16string(const utf16_char* value)
        {
            if (value == nullptr)
                return {};

            std::size_t length = 0;
            while (value[length] != u'\0')
                ++length;

            return std::u16string{value, length};
        }

        [[nodiscard]] inline std::u16string ascii_to_u16string(const char* value)
        {
            if (value == nullptr)
                return {};

            std::u16string result;
            while (*value != '\0')
            {
                result.push_back(static_cast<char16_t>(static_cast<unsigned char>(*value)));
                ++value;
            }

            return result;
        }

        [[nodiscard]] inline ulong add_ref(std::atomic<ulong>& references) noexcept
        {
            return references.fetch_add(1, std::memory_order_relaxed) + 1;
        }

        [[nodiscard]] inline ulong release_ref(std::atomic<ulong>& references) noexcept
        {
            auto current = references.load(std::memory_order_relaxed);
            while (current > 0)
            {
                if (references.compare_exchange_weak(
                        current,
                        current - 1,
                        std::memory_order_relaxed,
                        std::memory_order_relaxed))
                {
                    return current - 1;
                }
            }

            return 0;
        }

        inline void retain_unknown(void* value) noexcept
        {
            if (value == nullptr)
                return;

            auto unknown = static_cast<unknown_object*>(value);
            unknown->vtable->add_ref(value);
        }

        inline void release_unknown(void* value) noexcept
        {
            if (value == nullptr)
                return;

            auto unknown = static_cast<unknown_object*>(value);
            unknown->vtable->release(value);
        }
    }
}
