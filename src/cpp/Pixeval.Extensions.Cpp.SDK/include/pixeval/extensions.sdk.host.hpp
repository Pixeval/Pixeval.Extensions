#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class HostBase
    {
    public:
        HostBase();
        virtual ~HostBase() = default;

        HostBase(const HostBase&) = delete;
        HostBase& operator=(const HostBase&) = delete;

        [[nodiscard]] void* native_instance() noexcept
        {
            return &object_;
        }

        [[nodiscard]] virtual std::u16string_view extension_name() const = 0;
        [[nodiscard]] virtual std::u16string_view author_name() const = 0;
        [[nodiscard]] virtual std::u16string_view extension_link() const = 0;
        [[nodiscard]] virtual std::u16string_view help_link() const = 0;
        [[nodiscard]] virtual std::u16string_view description() const = 0;
        [[nodiscard]] virtual std::u16string_view sdk_version() const { return abi::sdk_version; }
        [[nodiscard]] virtual std::u16string_view version() const = 0;
        [[nodiscard]] virtual std::span<PixevalComObject* const> extensions() noexcept = 0;
        [[nodiscard]] virtual std::span<const std::uint8_t> icon() const noexcept = 0;

        virtual void initialize(HostContext&) {}

    private:
        HostContext context_;
        detail::host_object object_;

        friend class detail::HostBaseAccess;
    };

    namespace detail
    {
        class HostBaseAccess
        {
        public:
            [[nodiscard]] static HostContext& context(HostBase& host) noexcept { return host.context_; }
        };
    }
}
