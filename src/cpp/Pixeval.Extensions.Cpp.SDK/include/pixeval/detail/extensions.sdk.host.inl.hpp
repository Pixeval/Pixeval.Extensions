#pragma once
#include <pixeval/detail/extensions.sdk.core.inl.hpp>

namespace pixeval::extensions::detail
{
        inline hresult PIXEV_CALL host_query_interface(void* self, const guid& iid, void** result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = nullptr;
            if (iid != abi::iid_iunknown && iid != abi::iid_extensions_host)
                return E_NOINTERFACE;

            *result = self;
            host_add_ref(self);
            return S_OK;
        }

        inline ulong PIXEV_CALL host_add_ref(void* self)
        {
            return abi::add_ref(object_from_native<host_object>(self)->references);
        }

        inline ulong PIXEV_CALL host_release(void* self)
        {
            return abi::release_ref(object_from_native<host_object>(self)->references);
        }

        inline hresult PIXEV_CALL host_get_extension_name(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).extension_name(), result);
        }

        inline hresult PIXEV_CALL host_get_author_name(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).author_name(), result);
        }

        inline hresult PIXEV_CALL host_get_extension_link(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).extension_link(), result);
        }

        inline hresult PIXEV_CALL host_get_help_link(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).help_link(), result);
        }

        inline hresult PIXEV_CALL host_get_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).description(), result);
        }

        inline hresult PIXEV_CALL host_get_sdk_version(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).sdk_version(), result);
        }

        inline hresult PIXEV_CALL host_get_version(void* self, utf16_char** result)
        {
            return abi::copy_utf16(host_owner(self).version(), result);
        }

        inline hresult PIXEV_CALL host_get_extensions(void* self, std::int32_t* return_count, void*** result)
        {
            if (return_count == nullptr || result == nullptr)
                return E_POINTER;

            auto extensions = host_owner(self).extensions();
            auto extension_count = static_cast<std::int32_t>(extensions.size());
            auto bytes = sizeof(void*) * extensions.size();
            auto array = static_cast<void**>(abi::allocate_bytes(bytes));
            if (array == nullptr && !extensions.empty())
                return E_OUTOFMEMORY;

            for (std::size_t i = 0; i < extensions.size(); ++i)
            {
                array[i] = extensions[i]->native_instance();
                extensions[i]->add_ref_native();
            }

            *return_count = extension_count;
            *result = array;
            return S_OK;
        }

        inline hresult PIXEV_CALL host_get_icon(void* self, std::int32_t* return_count, std::uint8_t** result)
        {
            if (return_count == nullptr || result == nullptr)
                return E_POINTER;

            auto icon = host_owner(self).icon();
            *return_count = 0;
            *result = nullptr;
            if (icon.empty())
                return S_OK;
            if (icon.size() > static_cast<std::size_t>(std::numeric_limits<std::int32_t>::max()))
                return E_FAIL;

            auto buffer = static_cast<std::uint8_t*>(abi::allocate_bytes(icon.size()));
            if (buffer == nullptr)
                return E_OUTOFMEMORY;

            std::memcpy(buffer, icon.data(), icon.size());
            *return_count = static_cast<std::int32_t>(icon.size());
            *result = buffer;
            return S_OK;
        }

        inline hresult PIXEV_CALL host_initialize(void* self, const utf16_char* culture_name, const utf16_char* temp_directory, const utf16_char* extension_directory, void* logger)
        {
            auto& host = host_owner(self);
            auto& context = HostBaseAccess::context(host);
            context.culture_name = abi::to_u16string(culture_name);
            context.temp_directory = abi::to_u16string(temp_directory);
            context.extension_directory = abi::to_u16string(extension_directory);
            context.logger = Logger{logger};
            host.initialize(context);
            return S_OK;
        }
}
