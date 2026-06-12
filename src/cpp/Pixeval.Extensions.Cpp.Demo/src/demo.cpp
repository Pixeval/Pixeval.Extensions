#include <pixeval/extensions.hpp>

using namespace pixeval::extensions;

namespace pixeval::extensions::demo_assets
{
    extern const std::uint8_t logo_png[];
    extern const std::size_t logo_png_size;
}

namespace
{
    struct EntryValues
    {
        Symbol icon;
        std::u16string label;
        std::u16string description;
        std::u16string token;
        std::optional<std::u16string> placeholder;
    };

    std::vector<std::uint8_t> logo_icon()
    {
        return {
            pixeval::extensions::demo_assets::logo_png,
            pixeval::extensions::demo_assets::logo_png + pixeval::extensions::demo_assets::logo_png_size};
    }

    EntryValues entry(
        Symbol icon,
        std::u16string label,
        std::u16string description,
        std::u16string token = {},
        std::optional<std::u16string> placeholder = std::nullopt)
    {
        return {
            .icon = icon,
            .label = std::move(label),
            .description = std::move(description),
            .token = std::move(token),
            .placeholder = std::move(placeholder)};
    }

    template <typename TBase>
    class EntryBacked : public TBase
    {
    public:
        explicit EntryBacked(EntryValues values)
            : values_{std::move(values)}
        {
        }

        [[nodiscard]] Symbol icon() const override { return values_.icon; }
        [[nodiscard]] std::u16string label() const override { return values_.label; }
        [[nodiscard]] std::u16string description() const override { return values_.description; }

    protected:
        EntryValues values_;
    };

    template <typename TBase>
    class SettingBacked : public EntryBacked<TBase>
    {
    public:
        using EntryBacked<TBase>::EntryBacked;

        [[nodiscard]] std::u16string token() const override { return this->values_.token; }
        [[nodiscard]] std::optional<std::u16string> placeholder() const override { return this->values_.placeholder; }
    };

    template <typename TBase>
    class FormatProviderBacked : public TBase
    {
    public:
        FormatProviderBacked(std::u16string extension, std::u16string description)
            : extension_{std::move(extension)},
              description_{std::move(description)}
        {
        }

        [[nodiscard]] std::u16string format_extension() const override { return extension_; }
        [[nodiscard]] std::u16string format_description() const override { return description_; }

    private:
        std::u16string extension_;
        std::u16string description_;
    };

    class DemoBoolSetting final : public SettingBacked<BoolSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] bool default_value() const override { return true; }
        void on_value_changed(bool value) override { value_ = value; }

    private:
        bool value_ = default_value();
    };

    class DemoIntSetting final : public SettingBacked<IntSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] std::int32_t default_value() const override { return 3; }
        [[nodiscard]] std::int32_t min_value() const override { return 0; }
        [[nodiscard]] std::int32_t max_value() const override { return 10; }
        void on_value_changed(std::int32_t value) override { value_ = value; }

    private:
        std::int32_t value_ = default_value();
    };

    class DemoDoubleSetting final : public SettingBacked<DoubleSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] double default_value() const override { return 1.5; }
        [[nodiscard]] double min_value() const override { return 0.25; }
        [[nodiscard]] double max_value() const override { return 4.0; }
        [[nodiscard]] double step_value() const override { return 0.25; }
        void on_value_changed(double value) override { value_ = value; }

    private:
        double value_ = default_value();
    };

    class DemoStringSetting final : public SettingBacked<StringSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] std::u16string default_value() const override { return u"Pixeval C++ SDK Demo"; }
        void on_value_changed(std::u16string value) override { value_ = std::move(value); }

    private:
        std::u16string value_ = default_value();
    };

    class DemoStringsArraySetting final : public SettingBacked<StringsArraySettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] std::vector<std::u16string> default_value() const override { return {u"native", u"cpp", u"sdk"}; }
        void on_value_changed(std::vector<std::u16string> value) override { value_ = std::move(value); }

    private:
        std::vector<std::u16string> value_ = default_value();
    };

    class DemoEnumSetting final : public SettingBacked<EnumSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] std::int32_t default_value() const override { return 2; }
        [[nodiscard]] std::vector<std::pair<std::u16string, std::int32_t>> enum_key_values() const override
        {
            return {{u"Disabled", 0}, {u"Preview", 1}, {u"Enabled", 2}};
        }
        void on_value_changed(std::int32_t value) override { value_ = value; }

    private:
        std::int32_t value_ = default_value();
    };

    class DemoColorSetting final : public SettingBacked<ColorSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] std::uint32_t default_value() const override { return 0xFF3366CCu; }
        void on_value_changed(std::uint32_t value) override { value_ = value; }

    private:
        std::uint32_t value_ = default_value();
    };

    class DemoDateTimeOffsetSetting final : public SettingBacked<DateTimeOffsetSettingsExtensionBase>
    {
    public:
        using SettingBacked::SettingBacked;

        [[nodiscard]] DateTimeOffsetValue default_value() const override { return {638745120000000000LL, 480}; }
        void on_value_changed(DateTimeOffsetValue value) override { value_ = value; }

    private:
        DateTimeOffsetValue value_ = default_value();
    };

    class DemoDownloader final : public DownloaderExtensionBase
    {
    public:
        void download(ProgressNotifier notifier, std::u16string, std::u16string) override
        {
            notifier.progress_changed(100.0);
            notifier.completed();
        }
    };

    class DemoStaticFormat final : public FormatProviderBacked<StaticImageFormatProviderExtensionBase>
    {
    public:
        using FormatProviderBacked::FormatProviderBacked;
        [[nodiscard]] task<void> format_image(Stream, std::u16string) override { co_return; }
    };

    class DemoAnimatedFormat final : public FormatProviderBacked<AnimatedImageFormatProviderExtensionBase>
    {
    public:
        using FormatProviderBacked::FormatProviderBacked;
        [[nodiscard]] task<void> format_image(std::vector<std::pair<Stream, std::int32_t>>, std::u16string) override { co_return; }
    };

    class DemoNovelFormat final : public FormatProviderBacked<NovelFormatProviderExtensionBase>
    {
    public:
        using FormatProviderBacked::FormatProviderBacked;
        [[nodiscard]] task<void> format_novel(std::u16string, std::u16string, std::vector<std::pair<std::u16string, Stream>>) override { co_return; }
    };

    class DemoImageTransformer final : public EntryBacked<ImageTransformerCommandExtensionBase>
    {
    public:
        using EntryBacked::EntryBacked;
        [[nodiscard]] task<void> transform_async(Stream original_stream, Stream destination_stream) override
        {
            if (original_stream.has_value() && destination_stream.has_value())
                (void) original_stream.copy_to(destination_stream);
            co_return;
        }
    };

    class DemoTextTransformer final : public EntryBacked<TextTransformerCommandExtensionBase>
    {
    public:
        using EntryBacked::EntryBacked;

        [[nodiscard]] task<std::u16string> transform_async(std::u16string original_string, TextTransformerType) override
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
            : icon_{logo_icon()},
              extensions_{
                  &enabled_,
                  &level_,
                  &scale_,
                  &nickname_,
                  &tags_,
                  &mode_,
                  &color_,
                  &schedule_,
                  &downloader_,
                  &static_format_,
                  &animated_format_,
                  &novel_format_,
                  &viewer_command_,
                  &image_transformer_,
                  &text_transformer_}
        {
        }

        [[nodiscard]] std::u16string_view extension_name() const override { return u"Pixeval C++ SDK Demo"; }
        [[nodiscard]] std::u16string_view author_name() const override { return u"Pixeval.Extensions C++ SDK"; }
        [[nodiscard]] std::u16string_view extension_link() const override { return u"https://github.com/Pixeval/Pixeval.Extensions"; }
        [[nodiscard]] std::u16string_view help_link() const override { return u"https://github.com/Pixeval/Pixeval.Extensions/tree/master/src/cpp"; }
        [[nodiscard]] std::u16string_view description() const override { return u"A native C++ extension implemented through the Pixeval C++ SDK."; }
        [[nodiscard]] std::u16string_view version() const override { return u"0.2.0"; }
        [[nodiscard]] std::span<PixevalComObject* const> extensions() noexcept override { return {extensions_.data(), extensions_.size()}; }
        [[nodiscard]] std::span<const std::uint8_t> icon() const noexcept override { return {icon_.data(), icon_.size()}; }

        void initialize(HostContext& context) override
        {
            context.logger.log(LogLevel::Information, u"C++ SDK demo initialized.");
        }

    private:
        std::vector<std::uint8_t> icon_;

        DemoBoolSetting enabled_{
            entry(Symbol::CheckmarkCircle, u"C++ SDK Enabled", u"Turns the C++ SDK demo setting on or off.", u"cpp.sdk.enabled")};
        DemoIntSetting level_{
            entry(Symbol::NumberSymbol, u"C++ SDK Level", u"Integer setting sample.", u"cpp.sdk.level", u"0 - 10")};
        DemoDoubleSetting scale_{
            entry(Symbol::Resize, u"C++ SDK Scale", u"Double setting sample.", u"cpp.sdk.scale", u"0.25 - 4.0")};
        DemoStringSetting nickname_{
            entry(Symbol::Text, u"C++ SDK Nickname", u"String setting sample.", u"cpp.sdk.nickname", u"Enter a nickname")};
        DemoStringsArraySetting tags_{
            entry(Symbol::Tag, u"C++ SDK Tags", u"String array setting sample.", u"cpp.sdk.tags", u"One tag per row")};
        DemoEnumSetting mode_{
            entry(Symbol::Options, u"C++ SDK Mode", u"Enum setting sample.", u"cpp.sdk.mode")};
        DemoColorSetting color_{
            entry(Symbol::Color, u"C++ SDK Color", u"Color setting sample.", u"cpp.sdk.color")};
        DemoDateTimeOffsetSetting schedule_{
            entry(Symbol::Calendar, u"C++ SDK Schedule", u"DateTimeOffset setting sample.", u"cpp.sdk.schedule")};

        DemoDownloader downloader_;
        DemoStaticFormat static_format_{u".cpp-static", u"C++ SDK static image format provider sample."};
        DemoAnimatedFormat animated_format_{u".cpp-animated", u"C++ SDK animated image format provider sample."};
        DemoNovelFormat novel_format_{u".cpp-novel", u"C++ SDK novel format provider sample."};
        EntryBacked<ViewerCommandExtensionBase> viewer_command_{
            entry(Symbol::Eye, u"C++ SDK Viewer Command", u"Viewer command extension sample.")};
        DemoImageTransformer image_transformer_{
            entry(Symbol::ImageEdit, u"C++ SDK Image Transformer", u"Image transformer command sample.")};
        DemoTextTransformer text_transformer_{
            entry(Symbol::TextEditStyle, u"C++ SDK Text Transformer", u"Text transformer command sample.")};

        std::vector<PixevalComObject*> extensions_;
    };

    DemoHost g_host;
}

PIXEV_EXTENSION_HOST(g_host)
