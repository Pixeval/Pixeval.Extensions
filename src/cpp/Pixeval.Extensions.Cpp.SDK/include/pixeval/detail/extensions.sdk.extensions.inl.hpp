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
            static_cast<ExtensionBase&>(extension_owner(self)).on_extension_loaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_on_extension_unloaded(void* self)
        {
            static_cast<ExtensionBase&>(extension_owner(self)).on_extension_unloaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_download(void* self, void* notifier, const utf16_char* uri, const utf16_char* destination)
        {
            static_cast<DownloaderExtensionBase&>(extension_owner(self)).download(ProgressNotifier{notifier}, abi::to_u16string(uri), abi::to_u16string(destination));
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_format_extension(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<FormatProviderExtensionBase&>(extension_owner(self)).format_extension(), result);
        }

        inline hresult PIXEV_CALL extension_get_format_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<FormatProviderExtensionBase&>(extension_owner(self)).format_description(), result);
        }

        inline hresult PIXEV_CALL extension_format_static_image(void* self, void* task, void* image_stream, const utf16_char* destination_path)
        {
            auto& extension = static_cast<StaticImageFormatProviderExtensionBase&>(extension_owner(self));
            run_async(
                self,
                task,
                [&extension, image = Stream{image_stream}, destination = abi::to_u16string(destination_path)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_image(std::move(image), std::move(destination));
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

            std::vector<std::pair<Stream, std::int32_t>> images;
            images.reserve(static_cast<std::size_t>(images_count));
            for (std::int32_t i = 0; i < images_count; ++i)
                images.emplace_back(std::move(streams[i]), delays[i]);

            auto& extension = static_cast<AnimatedImageFormatProviderExtensionBase&>(extension_owner(self));
            run_async(
                self,
                task,
                [&extension, images = std::move(images), destination = abi::to_u16string(destination_path)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_image(std::move(images), std::move(destination));
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

            std::vector<std::pair<std::u16string, Stream>> images;
            images.reserve(static_cast<std::size_t>(images_count));
            for (std::int32_t i = 0; i < images_count; ++i)
                images.emplace_back(std::move(names[i]), std::move(streams[i]));

            auto& extension = static_cast<NovelFormatProviderExtensionBase&>(extension_owner(self));
            run_async(
                self,
                task,
                [&extension,
                 novel = abi::to_u16string(novel_input),
                 destination = abi::to_u16string(destination_path),
                 images = std::move(images)]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.format_novel(std::move(novel), std::move(destination), std::move(images));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_icon(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<std::int32_t>(static_cast<EntryExtensionBase&>(extension_owner(self)).icon());
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_label(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<EntryExtensionBase&>(extension_owner(self)).label(), result);
        }

        inline hresult PIXEV_CALL extension_get_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<EntryExtensionBase&>(extension_owner(self)).description(), result);
        }

        inline hresult PIXEV_CALL extension_transform_image(void* self, void* task, void* original_stream, void* destination_stream)
        {
            auto& extension = static_cast<ImageTransformerCommandExtensionBase&>(extension_owner(self));
            run_async(
                self,
                task,
                [&extension, original = Stream{original_stream}, destination = Stream{destination_stream}]() mutable -> ::pixeval::extensions::task<void>
                {
                    co_await extension.transform_async(std::move(original), std::move(destination));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_transform_text(void* self, void* task, const utf16_char* original_string, TextTransformerType type)
        {
            auto& extension = static_cast<TextTransformerCommandExtensionBase&>(extension_owner(self));
            run_async(
                self,
                task,
                [&extension, original = abi::to_u16string(original_string), type]() mutable -> ::pixeval::extensions::task<void>
                {
                    extension.set_transform_result_value(co_await extension.transform_async(std::move(original), type));
                });
            return S_OK;
        }

        inline hresult PIXEV_CALL extension_get_transform_result(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<TextTransformerCommandExtensionBase&>(extension_owner(self)).transform_result_value(), result);
        }
}
