#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class HostBase
    {
    public:
        explicit HostBase(HostMetadata metadata);
        virtual ~HostBase() = default;

        HostBase(const HostBase&) = delete;
        HostBase& operator=(const HostBase&) = delete;

        [[nodiscard]] const HostMetadata& metadata() const noexcept
        {
            return metadata_;
        }

        [[nodiscard]] const HostContext& context() const noexcept
        {
            return context_;
        }

        void add_extension(ExtensionBase& extension)
        {
            extensions_.push_back(&extension);
        }

        [[nodiscard]] void* native_instance() noexcept
        {
            return &object_;
        }

        virtual void initialize(HostContext&) {}

    private:
        HostMetadata metadata_;
        std::vector<ExtensionBase*> extensions_;
        HostContext context_;
        detail::host_object object_;

        friend class detail::HostBaseAccess;
    };

    namespace detail
    {
        class HostBaseAccess
        {
        public:
            [[nodiscard]] static HostMetadata& metadata(HostBase& host) noexcept { return host.metadata_; }
            [[nodiscard]] static std::vector<ExtensionBase*>& extensions(HostBase& host) noexcept { return host.extensions_; }
            [[nodiscard]] static HostContext& context(HostBase& host) noexcept { return host.context_; }
        };
    }
}
