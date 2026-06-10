#pragma once
#include <pixeval/extensions.sdk.types.hpp>

namespace pixeval::extensions
{
    class SettingBase : public ExtensionBase
    {
    public:
        SettingBase(EntryMetadata metadata, detail::setting_kind kind);

        [[nodiscard]] void* native_instance() noexcept override
        {
            return &object_;
        }

        void add_ref_native() noexcept override;

        [[nodiscard]] const EntryMetadata& metadata() const noexcept
        {
            return metadata_;
        }

        [[nodiscard]] detail::setting_kind kind() const noexcept
        {
            return kind_;
        }

        virtual void on_extension_loaded() {}
        virtual void on_extension_unloaded() {}
        virtual bool bool_default_value() const { return false; }
        virtual void on_bool_value_changed(bool) {}
        virtual std::int32_t int_default_value() const { return 0; }
        virtual void on_int_value_changed(std::int32_t) {}
        virtual std::int32_t int_min_value() const { return 0; }
        virtual std::int32_t int_max_value() const { return 0; }
        virtual std::int32_t int_step_value() const { return 1; }
        virtual double double_default_value() const { return 0; }
        virtual double double_min_value() const { return 0; }
        virtual double double_max_value() const { return 0; }
        virtual double double_step_value() const { return 1; }
        virtual void on_double_value_changed(double) {}
        virtual std::u16string string_default_value() const { return {}; }
        virtual void on_string_value_changed(std::u16string_view) {}
        virtual std::vector<std::u16string> strings_array_default_value() const { return {}; }
        virtual void on_strings_array_value_changed(std::span<const std::u16string>) {}
        virtual std::vector<std::pair<std::u16string, std::int32_t>> enum_key_values() const { return {}; }
        virtual std::uint32_t color_default_value() const { return 0; }
        virtual void on_color_value_changed(std::uint32_t) {}
        virtual DateTimeOffsetValue date_time_offset_default_value() const { return {0, 0}; }
        virtual void on_date_time_offset_value_changed(DateTimeOffsetValue) {}

    private:
        EntryMetadata metadata_;
        detail::setting_kind kind_;
        detail::setting_object object_;
    };

    class BoolSetting : public SettingBase
    {
    public:
        BoolSetting(EntryMetadata metadata, bool default_value = false)
            : SettingBase(std::move(metadata), detail::setting_kind::boolean),
              default_value_{default_value}
        {
        }

        [[nodiscard]] bool bool_default_value() const override
        {
            return default_value_;
        }

        void on_bool_value_changed(bool value) override
        {
            value_ = value;
        }

        [[nodiscard]] bool value() const noexcept
        {
            return value_.value_or(default_value_);
        }

    private:
        bool default_value_;
        std::optional<bool> value_;
    };

    class IntSetting : public SettingBase
    {
    public:
        IntSetting(EntryMetadata metadata, std::int32_t default_value, std::int32_t min_value, std::int32_t max_value, std::int32_t step_value)
            : SettingBase(std::move(metadata), detail::setting_kind::integer),
              default_value_{default_value},
              min_value_{min_value},
              max_value_{max_value},
              step_value_{step_value}
        {
        }

        [[nodiscard]] std::int32_t int_default_value() const override { return default_value_; }
        [[nodiscard]] std::int32_t int_min_value() const override { return min_value_; }
        [[nodiscard]] std::int32_t int_max_value() const override { return max_value_; }
        [[nodiscard]] std::int32_t int_step_value() const override { return step_value_; }
        void on_int_value_changed(std::int32_t value) override { value_ = value; }
        [[nodiscard]] std::int32_t value() const noexcept { return value_.value_or(default_value_); }

    private:
        std::int32_t default_value_;
        std::int32_t min_value_;
        std::int32_t max_value_;
        std::int32_t step_value_;
        std::optional<std::int32_t> value_;
    };

    class DoubleSetting : public SettingBase
    {
    public:
        DoubleSetting(EntryMetadata metadata, double default_value, double min_value, double max_value, double step_value)
            : SettingBase(std::move(metadata), detail::setting_kind::floating),
              default_value_{default_value},
              min_value_{min_value},
              max_value_{max_value},
              step_value_{step_value}
        {
        }

        [[nodiscard]] double double_default_value() const override { return default_value_; }
        [[nodiscard]] double double_min_value() const override { return min_value_; }
        [[nodiscard]] double double_max_value() const override { return max_value_; }
        [[nodiscard]] double double_step_value() const override { return step_value_; }
        void on_double_value_changed(double value) override { value_ = value; }
        [[nodiscard]] double value() const noexcept { return value_.value_or(default_value_); }

    private:
        double default_value_;
        double min_value_;
        double max_value_;
        double step_value_;
        std::optional<double> value_;
    };

    class StringSetting : public SettingBase
    {
    public:
        StringSetting(EntryMetadata metadata, std::u16string default_value)
            : SettingBase(std::move(metadata), detail::setting_kind::text),
              default_value_{std::move(default_value)}
        {
        }

        [[nodiscard]] std::u16string string_default_value() const override { return default_value_; }
        void on_string_value_changed(std::u16string_view value) override { value_ = std::u16string{value}; }
        [[nodiscard]] const std::u16string& value() const noexcept { return value_.value_or(default_value_); }

    private:
        std::u16string default_value_;
        std::optional<std::u16string> value_;
    };

    class StringsArraySetting : public SettingBase
    {
    public:
        StringsArraySetting(EntryMetadata metadata, std::vector<std::u16string> default_value)
            : SettingBase(std::move(metadata), detail::setting_kind::text_array),
              default_value_{std::move(default_value)}
        {
        }

        [[nodiscard]] std::vector<std::u16string> strings_array_default_value() const override { return default_value_; }
        void on_strings_array_value_changed(std::span<const std::u16string> value) override { value_ = {value.begin(), value.end()}; }
        [[nodiscard]] const std::vector<std::u16string>& value() const noexcept { return value_.empty() ? default_value_ : value_; }

    private:
        std::vector<std::u16string> default_value_;
        std::vector<std::u16string> value_;
    };

    class EnumSetting : public SettingBase
    {
    public:
        EnumSetting(EntryMetadata metadata, std::int32_t default_value, std::vector<std::pair<std::u16string, std::int32_t>> enum_key_values)
            : SettingBase(std::move(metadata), detail::setting_kind::enumeration),
              default_value_{default_value},
              enum_key_values_{std::move(enum_key_values)}
        {
        }

        [[nodiscard]] std::int32_t int_default_value() const override { return default_value_; }
        void on_int_value_changed(std::int32_t value) override { value_ = value; }
        [[nodiscard]] std::vector<std::pair<std::u16string, std::int32_t>> enum_key_values() const override { return enum_key_values_; }
        [[nodiscard]] std::int32_t value() const noexcept { return value_.value_or(default_value_); }

    private:
        std::int32_t default_value_;
        std::vector<std::pair<std::u16string, std::int32_t>> enum_key_values_;
        std::optional<std::int32_t> value_;
    };

    class ColorSetting : public SettingBase
    {
    public:
        ColorSetting(EntryMetadata metadata, std::uint32_t default_value)
            : SettingBase(std::move(metadata), detail::setting_kind::color),
              default_value_{default_value}
        {
        }

        [[nodiscard]] std::uint32_t color_default_value() const override { return default_value_; }
        void on_color_value_changed(std::uint32_t value) override { value_ = value; }
        [[nodiscard]] std::uint32_t value() const noexcept { return value_.value_or(default_value_); }

    private:
        std::uint32_t default_value_;
        std::optional<std::uint32_t> value_;
    };

    class DateTimeOffsetSetting : public SettingBase
    {
    public:
        DateTimeOffsetSetting(EntryMetadata metadata, DateTimeOffsetValue default_value)
            : SettingBase(std::move(metadata), detail::setting_kind::date_time_offset),
              default_value_{default_value}
        {
        }

        [[nodiscard]] DateTimeOffsetValue date_time_offset_default_value() const override { return default_value_; }
        void on_date_time_offset_value_changed(DateTimeOffsetValue value) override { value_ = value; }
        [[nodiscard]] DateTimeOffsetValue value() const noexcept { return value_.value_or(default_value_); }

    private:
        DateTimeOffsetValue default_value_;
        std::optional<DateTimeOffsetValue> value_;
    };
}
