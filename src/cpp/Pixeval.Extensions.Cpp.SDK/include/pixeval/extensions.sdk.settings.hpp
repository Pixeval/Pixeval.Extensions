#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class SettingBase : public ExtensionObjectBase
    {
    public:
        explicit SettingBase(detail::setting_kind kind);

        [[nodiscard]] void* native_instance() noexcept override
        {
            return &object_;
        }

        void add_ref_native() noexcept override;

        [[nodiscard]] detail::setting_kind kind() const noexcept
        {
            return kind_;
        }

    private:
        detail::setting_kind kind_;
        detail::setting_object object_;
    };
}
