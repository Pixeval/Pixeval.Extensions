#pragma once
#include <pixeval/detail/extensions.sdk.core.inl.hpp>

namespace pixeval::extensions::detail
{
        inline hresult PIXEV_CALL extension_query_interface(void* self, const guid& iid, void** result)
        {
            if (result == nullptr)
                return E_POINTER;

            auto& extension = extension_owner(self);
            *result = nullptr;
            if (!extension_supports_guid(extension.kind(), iid))
                return E_NOINTERFACE;

            *result = self;
            extension_add_ref(self);
            return S_OK;
        }

        inline ulong PIXEV_CALL extension_add_ref(void* self)
        {
            return abi::add_ref(object_from_native<extension_object>(self)->references);
        }

        inline ulong PIXEV_CALL extension_release(void* self)
        {
            return abi::release_ref(object_from_native<extension_object>(self)->references);
        }

        inline hresult PIXEV_CALL extension_on_extension_loaded(void* self)
        {
            extension_owner(self).on_extension_loaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_on_extension_unloaded(void* self)
        {
            extension_owner(self).on_extension_unloaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_download(void* self, void* notifier, const utf16_char* uri, const utf16_char* destination)
        {
            extension_owner(self).download(ProgressNotifier{notifier}, abi::to_u16string(uri), abi::to_u16string(destination));
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_format_extension(void* self, utf16_char** result)
        {
            return abi::copy_utf16(extension_owner(self).format_extension(), result);
        }

        inline hresult PIXEV_CALL extension_get_format_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(extension_owner(self).format_description(), result);
        }

        inline hresult PIXEV_CALL extension_format_static_image(void* self, void* task, void* image_stream, const utf16_char* destination_path)
        {
            auto& extension = extension_owner(self);
            run_async(
                self,
                task,
                [&extension, image = Stream{image_stream}, destination = abi::to_u16string(destination_path)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_static_image(std::move(image), std::move(destination));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_format_animated_image(void* self, void* task, void** images_keys, std::int32_t* images_values, const utf16_char* destination_path, std::int32_t images_count)
        {
            std::vector<Stream> streams;
            streams.reserve(static_cast<std::size_t>(images_count));
            for (std::int32_t i = 0; i < images_count; ++i)
                streams.emplace_back(images_keys[i]);

            std::vector<std::int32_t> delays;
            delays.reserve(static_cast<std::size_t>(images_count));
            for (std::int32_t i = 0; i < images_count; ++i)
                delays.push_back(images_values[i]);

            auto& extension = extension_owner(self);
            run_async(
                self,
                task,
                [&extension, streams = std::move(streams), delays = std::move(delays), destination = abi::to_u16string(destination_path)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_animated_image(std::move(streams), std::move(delays), std::move(destination));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_format_novel(void* self, void* task, const utf16_char* novel_input, const utf16_char* destination_path, void** images_keys, void** images_values, std::int32_t images_count)
        {
            std::vector<std::u16string> names;
            names.reserve(static_cast<std::size_t>(images_count));
            std::vector<Stream> streams;
            streams.reserve(static_cast<std::size_t>(images_count));

            for (std::int32_t i = 0; i < images_count; ++i)
                names.push_back(abi::to_u16string(static_cast<const utf16_char*>(images_keys[i])));

            for (std::int32_t i = 0; i < images_count; ++i)
                streams.emplace_back(images_values[i]);

            auto& extension = extension_owner(self);
            run_async(
                self,
                task,
                [&extension,
                 novel = abi::to_u16string(novel_input),
                 destination = abi::to_u16string(destination_path),
                 names = std::move(names),
                 streams = std::move(streams)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_novel(std::move(novel), std::move(destination), std::move(names), std::move(streams));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_icon(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<std::int32_t>(extension_owner(self).metadata().icon);
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_label(void* self, utf16_char** result)
        {
            return abi::copy_utf16(extension_owner(self).metadata().label, result);
        }

        inline hresult PIXEV_CALL extension_get_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(extension_owner(self).metadata().description, result);
        }

        inline hresult PIXEV_CALL extension_transform_image(void* self, void* task, void* original_stream, void* destination_stream)
        {
            auto& extension = extension_owner(self);
            run_async(
                self,
                task,
                [&extension, original = Stream{original_stream}, destination = Stream{destination_stream}]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.transform_image(std::move(original), std::move(destination));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_transform_text(void* self, void* task, const utf16_char* original_string, TextTransformerType type)
        {
            auto& extension = extension_owner(self);
            run_async(
                self,
                task,
                [&extension, original = abi::to_u16string(original_string), type]() mutable -> ::pixeval::extensions::task<void>
                {
                    extension.transform_result_ = co_await extension.transform_text(std::move(original), type);
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_transform_result(void* self, utf16_char** result)
        {
            return abi::copy_utf16(extension_owner(self).transform_result_, result);
        }
}
