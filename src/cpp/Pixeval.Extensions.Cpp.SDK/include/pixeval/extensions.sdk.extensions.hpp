#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class ComExtensionBase : public ExtensionBase
    {
    public:
        ComExtensionBase(EntryMetadata metadata, detail::extension_kind kind);
        ComExtensionBase(std::u16string format_extension, std::u16string format_description, detail::extension_kind kind);

        [[nodiscard]] void* native_instance() noexcept override
        {
            return &object_;
        }

        void add_ref_native() noexcept override;

        [[nodiscard]] const EntryMetadata& metadata() const noexcept
        {
            return metadata_;
        }

        [[nodiscard]] const std::u16string& format_extension() const noexcept
        {
            return format_extension_;
        }

        [[nodiscard]] const std::u16string& format_description() const noexcept
        {
            return format_description_;
        }

        [[nodiscard]] detail::extension_kind kind() const noexcept
        {
            return kind_;
        }

        virtual void on_extension_loaded() {}
        virtual void on_extension_unloaded() {}
        virtual void download(ProgressNotifier, std::u16string_view, std::u16string_view) {}
        [[nodiscard]] virtual task<void> format_static_image(Stream, std::u16string)
        {
            co_return;
        }
        [[nodiscard]] virtual task<void> format_animated_image(std::vector<Stream>, std::vector<std::int32_t>, std::u16string)
        {
            co_return;
        }
        [[nodiscard]] virtual task<void> format_novel(std::u16string, std::u16string, std::vector<std::u16string>, std::vector<Stream>)
        {
            co_return;
        }
        [[nodiscard]] virtual task<void> transform_image(Stream original_stream, Stream destination_stream)
        {
            if (original_stream.has_value() && destination_stream.has_value())
                (void) original_stream.copy_to(destination_stream);
            co_return;
        }
        [[nodiscard]] virtual task<std::u16string> transform_text(std::u16string original_string, TextTransformerType)
        {
            co_return original_string;
        }

    private:
        friend hresult PIXEV_CALL detail::extension_transform_text(void*, void*, const utf16_char*, TextTransformerType);
        friend hresult PIXEV_CALL detail::extension_get_transform_result(void*, utf16_char**);

        EntryMetadata metadata_;
        std::u16string format_extension_;
        std::u16string format_description_;
        detail::extension_kind kind_;
        detail::extension_object object_;
        std::u16string transform_result_;
    };

    class DownloaderExtension : public ComExtensionBase
    {
    public:
        explicit DownloaderExtension(EntryMetadata metadata)
            : ComExtensionBase(std::move(metadata), detail::extension_kind::downloader)
        {
        }
    };

    class StaticImageFormatProviderExtension : public ComExtensionBase
    {
    public:
        StaticImageFormatProviderExtension(std::u16string format_extension, std::u16string format_description)
            : ComExtensionBase(std::move(format_extension), std::move(format_description), detail::extension_kind::static_image_format_provider)
        {
        }
    };

    class AnimatedImageFormatProviderExtension : public ComExtensionBase
    {
    public:
        AnimatedImageFormatProviderExtension(std::u16string format_extension, std::u16string format_description)
            : ComExtensionBase(std::move(format_extension), std::move(format_description), detail::extension_kind::animated_image_format_provider)
        {
        }
    };

    class NovelFormatProviderExtension : public ComExtensionBase
    {
    public:
        NovelFormatProviderExtension(std::u16string format_extension, std::u16string format_description)
            : ComExtensionBase(std::move(format_extension), std::move(format_description), detail::extension_kind::novel_format_provider)
        {
        }
    };

    class ViewerCommandExtension : public ComExtensionBase
    {
    public:
        explicit ViewerCommandExtension(EntryMetadata metadata)
            : ComExtensionBase(std::move(metadata), detail::extension_kind::viewer_command)
        {
        }
    };

    class ImageTransformerCommandExtension : public ComExtensionBase
    {
    public:
        explicit ImageTransformerCommandExtension(EntryMetadata metadata)
            : ComExtensionBase(std::move(metadata), detail::extension_kind::image_transformer_command)
        {
        }
    };

    class TextTransformerCommandExtension : public ComExtensionBase
    {
    public:
        explicit TextTransformerCommandExtension(EntryMetadata metadata)
            : ComExtensionBase(std::move(metadata), detail::extension_kind::text_transformer_command)
        {
        }
    };
}
