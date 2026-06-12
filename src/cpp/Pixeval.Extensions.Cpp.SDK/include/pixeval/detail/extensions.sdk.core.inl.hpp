#pragma once
#include <pixeval/extensions.sdk.host.hpp>

namespace pixeval::extensions
{
    inline PixevalComObject::PixevalComObject(void** vtable, detail::supports_interface_fn supports_interface)
        : supports_interface_{supports_interface},
          object_{vtable, 1, this}
    {
    }

    inline void PixevalComObject::add_ref_native() noexcept
    {
        detail::pixeval_add_ref(native_instance());
    }

    inline HostBase::HostBase()
        : object_{detail::host_vtable, 1, this}
    {
    }

    namespace detail
    {
        [[nodiscard]] inline HostBase& host_owner(void* self) noexcept
        {
            return *object_from_native<host_object>(self)->owner;
        }

        [[nodiscard]] inline PixevalComObject& pixeval_owner(void* self) noexcept
        {
            return *object_from_native<pixeval_com_object>(self)->owner;
        }

        inline hresult PIXEV_CALL pixeval_query_interface(void* self, const guid& iid, void** result)
        {
            if (result == nullptr)
                return E_POINTER;

            auto& object = pixeval_owner(self);
            *result = nullptr;
            if (!object.supports_interface(iid))
                return E_NOINTERFACE;

            *result = self;
            pixeval_add_ref(self);
            return S_OK;
        }

        inline ulong PIXEV_CALL pixeval_add_ref(void* self)
        {
            return abi::add_ref(object_from_native<pixeval_com_object>(self)->references);
        }

        inline ulong PIXEV_CALL pixeval_release(void* self)
        {
            return abi::release_ref(object_from_native<pixeval_com_object>(self)->references);
        }

    }
}
