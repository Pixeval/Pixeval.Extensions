#include <pixeval/extensions.hpp>

using namespace pixeval::extensions;

namespace pixeval::extensions::demo_assets
{
    extern const std::uint8_t logo_png[];
    extern const std::size_t logo_png_size;
}

namespace
{
    std::vector<std::uint8_t> logo_icon()
    {
        return {
            pixeval::extensions::demo_assets::logo_png,
            pixeval::extensions::demo_assets::logo_png + pixeval::extensions::demo_assets::logo_png_size};
    }

    EntryMetadata entry(
        Symbol icon,
        std::u16string label,
        std::u16string description,
        std::u16string token,
        std::optional<std::u16string> placeholder = std::nullopt)
    {
        return EntryMetadata{
            .icon = icon,
            .label = std::move(label),
            .description = std::move(description),
            .description_uri = std::nullopt,
            .token = std::move(token),
            .placeholder = std::move(placeholder)};
    }

    class LoggingBoolSetting final : public BoolSetting
    {
    public:
        using BoolSetting::BoolSetting;

        void on_bool_value_changed(bool value) override
        {
            BoolSetting::on_bool_value_changed(value);
        }
    };

    class DemoDownloader final : public DownloaderExtension
    {
    public:
        using DownloaderExtension::DownloaderExtension;

        void download(ProgressNotifier notifier, std::u16string_view, std::u16string_view) override
        {
            notifier.progress_changed(100.0);
            notifier.completed();
        }
    };

    class DemoTextTransformer final : public TextTransformerCommandExtension
    {
    public:
        using TextTransformerCommandExtension::TextTransformerCommandExtension;

        [[nodiscard]] task<std::u16string> transform_text(std::u16string original_string, TextTransformerType) override
        {
            std::u16string result = u"C++ transformed: ";
            result.append(original_string);
            co_return result;
        }
    };

    class DemoHost final : public HostBase
    {
    public:
        DemoHost()
            : HostBase(HostMetadata{
                  .extension_name = u"Pixeval C++ SDK Demo",
                  .author_name = u"Pixeval.Extensions C++ SDK",
                  .extension_link = u"https://github.com/Pixeval/Pixeval.Extensions",
                  .help_link = u"https://github.com/Pixeval/Pixeval.Extensions/tree/master/src/cpp",
                  .description = u"A native C++ extension implemented through the Pixeval C++ SDK.",
                  .version = u"0.2.0",
                  .icon = logo_icon()})
        {
            add_extension(enabled_);
            add_extension(level_);
            add_extension(scale_);
            add_extension(nickname_);
            add_extension(tags_);
            add_extension(mode_);
            add_extension(color_);
            add_extension(schedule_);
            add_extension(downloader_);
            add_extension(static_format_);
            add_extension(animated_format_);
            add_extension(novel_format_);
            add_extension(viewer_command_);
            add_extension(image_transformer_);
            add_extension(text_transformer_);
        }

        void initialize(HostContext& context) override
        {
            context.logger.log(LogLevel::Information, u"C++ SDK demo initialized.");
        }

    private:
        LoggingBoolSetting enabled_{
            entry(Symbol::CheckmarkCircle, u"C++ SDK Enabled", u"Turns the C++ SDK demo setting on or off.", u"cpp.sdk.enabled"),
            true};

        IntSetting level_{
            entry(Symbol::NumberSymbol, u"C++ SDK Level", u"Integer setting sample.", u"cpp.sdk.level", u"0 - 10"),
            3,
            0,
            10,
            1};

        DoubleSetting scale_{
            entry(Symbol::Resize, u"C++ SDK Scale", u"Double setting sample.", u"cpp.sdk.scale", u"0.25 - 4.0"),
            1.5,
            0.25,
            4.0,
            0.25};

        StringSetting nickname_{
            entry(Symbol::Text, u"C++ SDK Nickname", u"String setting sample.", u"cpp.sdk.nickname", u"Enter a nickname"),
            u"Pixeval C++ SDK Demo"};

        StringsArraySetting tags_{
            entry(Symbol::Tag, u"C++ SDK Tags", u"String array setting sample.", u"cpp.sdk.tags", u"One tag per row"),
            {u"native", u"cpp", u"sdk"}};

        EnumSetting mode_{
            entry(Symbol::Options, u"C++ SDK Mode", u"Enum setting sample.", u"cpp.sdk.mode"),
            2,
            {{u"Disabled", 0}, {u"Preview", 1}, {u"Enabled", 2}}};

        ColorSetting color_{
            entry(Symbol::Color, u"C++ SDK Color", u"Color setting sample.", u"cpp.sdk.color"),
            0xFF3366CCu};

        DateTimeOffsetSetting schedule_{
            entry(Symbol::Calendar, u"C++ SDK Schedule", u"DateTimeOffset setting sample.", u"cpp.sdk.schedule"),
            DateTimeOffsetValue{638745120000000000LL, 480}};

        DemoDownloader downloader_{
            entry(Symbol::ArrowDownload, u"C++ SDK Downloader", u"Downloader extension sample.", u"cpp.sdk.downloader")};

        StaticImageFormatProviderExtension static_format_{
            u".cpp-static",
            u"C++ SDK static image format provider sample."};

        AnimatedImageFormatProviderExtension animated_format_{
            u".cpp-animated",
            u"C++ SDK animated image format provider sample."};

        NovelFormatProviderExtension novel_format_{
            u".cpp-novel",
            u"C++ SDK novel format provider sample."};

        ViewerCommandExtension viewer_command_{
            entry(Symbol::Eye, u"C++ SDK Viewer Command", u"Viewer command extension sample.", u"cpp.sdk.viewerCommand")};

        ImageTransformerCommandExtension image_transformer_{
            entry(Symbol::ImageEdit, u"C++ SDK Image Transformer", u"Image transformer command sample.", u"cpp.sdk.imageTransformer")};

        DemoTextTransformer text_transformer_{
            entry(Symbol::TextEditStyle, u"C++ SDK Text Transformer", u"Text transformer command sample.", u"cpp.sdk.textTransformer")};
    };

    DemoHost g_host;
}

PIXEV_EXTENSION_HOST(g_host)
