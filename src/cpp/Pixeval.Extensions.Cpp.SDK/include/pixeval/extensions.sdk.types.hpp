#pragma once
#include <pixeval/symbol.hpp>
#include <pixeval/extensions.common.hpp>

namespace pixeval::extensions
{
    struct HostMetadata
    {
        std::u16string extension_name;
        std::u16string author_name;
        std::u16string extension_link;
        std::u16string help_link;
        std::u16string description;
        std::u16string sdk_version = std::u16string{abi::sdk_version};
        std::u16string version;
        std::vector<std::uint8_t> icon;
    };

    struct EntryMetadata
    {
        Symbol icon = Symbol::Settings;
        std::u16string label;
        std::u16string description;
        std::optional<std::u16string> description_uri;
        std::u16string token;
        std::optional<std::u16string> placeholder;
    };

    struct HostContext
    {
        std::u16string culture_name;
        std::u16string temp_directory;
        std::u16string extension_directory;
        Logger logger;
    };

    class PixevalComObject;
    class HostBase;

    namespace detail
    {
        class HostBaseAccess;
        using supports_interface_fn = bool (*)(const guid&) noexcept;

        struct host_object
        {
            void** vtable = nullptr;
            std::atomic<ulong> references{1};
            ::pixeval::extensions::HostBase* owner = nullptr;
        };

        struct pixeval_com_object
        {
            void** vtable = nullptr;
            std::atomic<ulong> references{1};
            ::pixeval::extensions::PixevalComObject* owner = nullptr;
        };

        template <typename TObject>
        [[nodiscard]] inline TObject* object_from_native(void* self) noexcept
        {
            return static_cast<TObject*>(self);
        }

        hresult PIXEV_CALL host_query_interface(void* self, const guid& iid, void** result);
        ulong PIXEV_CALL host_add_ref(void* self);
        ulong PIXEV_CALL host_release(void* self);
        hresult PIXEV_CALL host_get_extension_name(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_author_name(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_extension_link(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_help_link(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_description(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_sdk_version(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_version(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_extensions(void* self, std::int32_t* return_count, void*** result);
        hresult PIXEV_CALL host_get_icon(void* self, std::int32_t* return_count, std::uint8_t** result);
        hresult PIXEV_CALL host_initialize(void* self, const utf16_char* culture_name, const utf16_char* temp_directory, const utf16_char* extension_directory, void* logger);

        hresult PIXEV_CALL pixeval_query_interface(void* self, const guid& iid, void** result);
        ulong PIXEV_CALL pixeval_add_ref(void* self);
        ulong PIXEV_CALL pixeval_release(void* self);
        [[nodiscard]] PixevalComObject& pixeval_owner(void* self) noexcept;

        inline void free_string_array(void** values, std::int32_t count) noexcept
        {
            if (values == nullptr)
                return;

            for (std::int32_t i = 0; i < count; ++i)
                abi::free_bytes(values[i]);
            abi::free_bytes(values);
        }

        inline hresult copy_string_array(const std::vector<std::u16string>& values, std::int32_t* count, void*** result)
        {
            if (count == nullptr || result == nullptr)
                return E_POINTER;

            auto array = static_cast<void**>(abi::allocate_bytes(sizeof(void*) * values.size()));
            if (array == nullptr && !values.empty())
                return E_OUTOFMEMORY;

            for (std::size_t i = 0; i < values.size(); ++i)
            {
                auto hr = abi::copy_utf16(values[i], reinterpret_cast<utf16_char**>(&array[i]));
                if (hr != S_OK)
                {
                    free_string_array(array, static_cast<std::int32_t>(i));
                    return hr;
                }
            }

            *count = static_cast<std::int32_t>(values.size());
            *result = array;
            return S_OK;
        }

        inline hresult copy_int32_array(const std::vector<std::int32_t>& values, std::int32_t* count, std::int32_t** result)
        {
            if (result == nullptr)
                return E_POINTER;

            auto array = static_cast<std::int32_t*>(abi::allocate_bytes(sizeof(std::int32_t) * values.size()));
            if (array == nullptr && !values.empty())
                return E_OUTOFMEMORY;

            for (std::size_t i = 0; i < values.size(); ++i)
                array[i] = values[i];

            if (count != nullptr)
                *count = static_cast<std::int32_t>(values.size());
            *result = array;
            return S_OK;
        }

        inline hresult copy_byte_array(const std::vector<std::uint8_t>& values, std::int32_t* count, std::uint8_t** result)
        {
            if (count == nullptr || result == nullptr)
                return E_POINTER;

            auto array = static_cast<std::uint8_t*>(abi::allocate_bytes(values.size()));
            if (array == nullptr && !values.empty())
                return E_OUTOFMEMORY;

            for (std::size_t i = 0; i < values.size(); ++i)
                array[i] = values[i];

            *count = static_cast<std::int32_t>(values.size());
            *result = array;
            return S_OK;
        }

        inline std::vector<std::u16string> read_string_array(void** values, std::int32_t count)
        {
            std::vector<std::u16string> result;
            result.reserve(static_cast<std::size_t>(count));
            for (std::int32_t i = 0; i < count; ++i)
                result.push_back(abi::to_u16string(static_cast<const utf16_char*>(values[i])));
            return result;
        }

        inline std::vector<std::int32_t> read_int32_array(const std::int32_t* values, std::int32_t count)
        {
            std::vector<std::int32_t> result;
            result.reserve(static_cast<std::size_t>(count));
            for (std::int32_t i = 0; i < count; ++i)
                result.push_back(values[i]);
            return result;
        }

        inline std::vector<Stream> read_stream_array(void** values, std::int32_t count)
        {
            std::vector<Stream> result;
            result.reserve(static_cast<std::size_t>(count));
            for (std::int32_t i = 0; i < count; ++i)
                result.emplace_back(values[i]);
            return result;
        }

        template <typename TOperation>
        inline void run_async(void* self, void* task, TOperation&& operation)
        {
            struct detached_task
            {
                struct promise_type
                {
                    detached_task get_return_object() noexcept
                    {
                        return {};
                    }

                    std::suspend_never initial_suspend() noexcept
                    {
                        return {};
                    }

                    std::suspend_never final_suspend() noexcept
                    {
                        return {};
                    }

                    void return_void() noexcept
                    {
                    }

                    void unhandled_exception() noexcept
                    {
                    }
                };
            };

            auto runner = [](NativeComPtr keep_alive, TaskCompletionSource source, TOperation operation) -> detached_task
            {
                try
                {
                    co_await operation();
                }
                catch (...)
                {
                }

                (void) source.set_completed();
            };

            runner(NativeComPtr{self}, TaskCompletionSource{task}, std::forward<TOperation>(operation));
        }
    }

    class PixevalComObject
    {
    public:
        explicit PixevalComObject(void** vtable, detail::supports_interface_fn supports_interface);
        virtual ~PixevalComObject() = default;

        [[nodiscard]] void* native_instance() noexcept
        {
            return &object_;
        }

        void add_ref_native() noexcept;

        [[nodiscard]] bool supports_interface(const guid& iid) const noexcept
        {
            return supports_interface_(iid);
        }

    private:
        detail::supports_interface_fn supports_interface_;
        detail::pixeval_com_object object_;
    };
}
