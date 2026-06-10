#pragma once
#include <pixeval/extensions.sdk.extensions.hpp>
#include <pixeval/extensions.sdk.settings.hpp>
#include <pixeval/extensions.sdk.host.hpp>

namespace pixeval::extensions
{
    inline ComExtensionBase::ComExtensionBase(EntryMetadata metadata, detail::extension_kind kind)
        : metadata_{std::move(metadata)},
          kind_{kind},
          object_{detail::vtable_for(kind), 1, this}
    {
    }

    inline ComExtensionBase::ComExtensionBase(std::u16string format_extension, std::u16string format_description, detail::extension_kind kind)
        : format_extension_{std::move(format_extension)},
          format_description_{std::move(format_description)},
          kind_{kind},
          object_{detail::vtable_for(kind), 1, this}
    {
    }

    inline void ComExtensionBase::add_ref_native() noexcept
    {
        detail::extension_add_ref(native_instance());
    }

    inline SettingBase::SettingBase(EntryMetadata metadata, detail::setting_kind kind)
        : metadata_{std::move(metadata)},
          kind_{kind},
          object_{detail::vtable_for(kind), 1, this}
    {
    }

    inline void SettingBase::add_ref_native() noexcept
    {
        detail::setting_add_ref(native_instance());
    }

    inline HostBase::HostBase(HostMetadata metadata)
        : metadata_{std::move(metadata)},
          object_{detail::host_vtable, 1, this}
    {
    }

    namespace detail
    {
        [[nodiscard]] inline HostBase& host_owner(void* self) noexcept
        {
            return *object_from_native<host_object>(self)->owner;
        }

        [[nodiscard]] inline SettingBase& setting_owner(void* self) noexcept
        {
            return *object_from_native<setting_object>(self)->owner;
        }

        [[nodiscard]] inline ComExtensionBase& extension_owner(void* self) noexcept
        {
            return *object_from_native<extension_object>(self)->owner;
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
}
