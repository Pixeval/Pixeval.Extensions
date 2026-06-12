#pragma once
#include <pixeval/detail/extensions.sdk.host.inl.hpp>

namespace pixeval::extensions
{
    [[nodiscard]] inline hresult export_host(HostBase& host, void** result)
    {
        if (result == nullptr)
            return E_POINTER;

        *result = host.native_instance();
        detail::host_add_ref(*result);
        return S_OK;
    }
}

#define PIXEV_EXTENSION_HOST(host_instance) \
    PIXEV_EXPORT ::pixeval::extensions::hresult PIXEV_CALL GetExtensionsHost(void** result) \
    { \
        return ::pixeval::extensions::export_host((host_instance), result); \
    }
