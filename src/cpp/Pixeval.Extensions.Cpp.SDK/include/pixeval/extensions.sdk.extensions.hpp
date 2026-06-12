#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class ComExtensionBase : public ExtensionObjectBase
    {
    public:
        explicit ComExtensionBase(detail::extension_kind kind);

        [[nodiscard]] void* native_instance() noexcept override
        {
            return &object_;
        }

        void add_ref_native() noexcept override;

        [[nodiscard]] detail::extension_kind kind() const noexcept
        {
            return kind_;
        }

    private:
        detail::extension_kind kind_;
        detail::extension_object object_;
    };
}
