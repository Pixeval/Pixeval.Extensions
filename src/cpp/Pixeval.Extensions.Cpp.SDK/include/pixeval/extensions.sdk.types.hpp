#pragma once
#include <pixeval/symbol.hpp>
#include <pixeval/extensions.common.hpp>

namespace pixeval::extensions
{
    struct HostMetadata
    {
        std::u16string extension_name;
        std::u16string author_name;
        std::u16string extension_link;
        std::u16string help_link;
        std::u16string description;
        std::u16string sdk_version = std::u16string{abi::sdk_version};
        std::u16string version;
        std::vector<std::uint8_t> icon;
    };

    struct EntryMetadata
    {
        Symbol icon = Symbol::Settings;
        std::u16string label;
        std::u16string description;
        std::optional<std::u16string> description_uri;
        std::u16string token;
        std::optional<std::u16string> placeholder;
    };

    struct HostContext
    {
        std::u16string culture_name;
        std::u16string temp_directory;
        std::u16string extension_directory;
        Logger logger;
    };

    class ExtensionObjectBase
    {
    public:
        virtual ~ExtensionObjectBase() = default;

        [[nodiscard]] virtual void* native_instance() noexcept = 0;
        virtual void add_ref_native() noexcept = 0;
    };

    class HostBase;
    class ComExtensionBase;
    class SettingBase;

    namespace detail
    {
        class HostBaseAccess;

        enum class setting_kind
        {
            boolean,
            integer,
            floating,
            text,
            text_array,
            enumeration,
            color,
            date_time_offset
        };

        enum class extension_kind
        {
            downloader,
            static_image_format_provider,
            animated_image_format_provider,
            novel_format_provider,
            viewer_command,
            image_transformer_command,
            text_transformer_command
        };

        struct host_object
        {
            void** vtable = nullptr;
            std::atomic<ulong> references{1};
            ::pixeval::extensions::HostBase* owner = nullptr;
        };

        struct extension_object
        {
            void** vtable = nullptr;
            std::atomic<ulong> references{1};
            ::pixeval::extensions::ComExtensionBase* owner = nullptr;
        };

        struct setting_object
        {
            void** vtable = nullptr;
            std::atomic<ulong> references{1};
            ::pixeval::extensions::SettingBase* owner = nullptr;
        };

        template <typename TObject>
        [[nodiscard]] inline TObject* object_from_native(void* self) noexcept
        {
            return static_cast<TObject*>(self);
        }

        [[nodiscard]] inline SettingsType settings_type(setting_kind kind) noexcept
        {
            switch (kind)
            {
            case setting_kind::boolean: return SettingsType::Bool;
            case setting_kind::integer: return SettingsType::Int;
            case setting_kind::floating: return SettingsType::Double;
            case setting_kind::text: return SettingsType::String;
            case setting_kind::text_array: return SettingsType::StringsArray;
            case setting_kind::enumeration: return SettingsType::Enum;
            case setting_kind::color: return SettingsType::Color;
            case setting_kind::date_time_offset: return SettingsType::DateTimeOffset;
            }

            return SettingsType::String;
        }

        [[nodiscard]] inline bool setting_supports_guid(setting_kind kind, const guid& iid) noexcept
        {
            switch (kind)
            {
            case setting_kind::boolean:
                return abi::supports_bool_settings_extension(iid);
            case setting_kind::integer:
                return abi::supports_int_settings_extension(iid);
            case setting_kind::floating:
                return abi::supports_double_settings_extension(iid);
            case setting_kind::text:
                return abi::supports_string_settings_extension(iid);
            case setting_kind::text_array:
                return abi::supports_strings_array_settings_extension(iid);
            case setting_kind::enumeration:
                return abi::supports_enum_settings_extension(iid);
            case setting_kind::color:
                return abi::supports_color_settings_extension(iid);
            case setting_kind::date_time_offset:
                return abi::supports_date_time_offset_settings_extension(iid);
            }

            return false;
        }

        [[nodiscard]] inline bool extension_supports_guid(extension_kind kind, const guid& iid) noexcept
        {
            switch (kind)
            {
            case extension_kind::downloader:
                return abi::supports_downloader_extension(iid);
            case extension_kind::static_image_format_provider:
                return abi::supports_static_image_format_provider_extension(iid);
            case extension_kind::animated_image_format_provider:
                return abi::supports_animated_image_format_provider_extension(iid);
            case extension_kind::novel_format_provider:
                return abi::supports_novel_format_provider_extension(iid);
            case extension_kind::viewer_command:
                return abi::supports_viewer_command_extension(iid);
            case extension_kind::image_transformer_command:
                return abi::supports_image_transformer_command_extension(iid);
            case extension_kind::text_transformer_command:
                return abi::supports_text_transformer_command_extension(iid);
            }

            return false;
        }

        hresult PIXEV_CALL host_query_interface(void* self, const guid& iid, void** result);
        ulong PIXEV_CALL host_add_ref(void* self);
        ulong PIXEV_CALL host_release(void* self);
        hresult PIXEV_CALL host_get_extension_name(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_author_name(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_extension_link(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_help_link(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_description(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_sdk_version(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_version(void* self, utf16_char** result);
        hresult PIXEV_CALL host_get_extensions(void* self, std::int32_t* return_count, void*** result);
        hresult PIXEV_CALL host_get_icon(void* self, std::int32_t* return_count, std::uint8_t** result);
        hresult PIXEV_CALL host_initialize(void* self, const utf16_char* culture_name, const utf16_char* temp_directory, const utf16_char* extension_directory, void* logger);

        hresult PIXEV_CALL extension_query_interface(void* self, const guid& iid, void** result);
        ulong PIXEV_CALL extension_add_ref(void* self);
        ulong PIXEV_CALL extension_release(void* self);
        hresult PIXEV_CALL extension_on_extension_loaded(void* self);
        hresult PIXEV_CALL extension_on_extension_unloaded(void* self);
        hresult PIXEV_CALL extension_download(void* self, void* notifier, const utf16_char* uri, const utf16_char* destination);
        hresult PIXEV_CALL extension_get_format_extension(void* self, utf16_char** result);
        hresult PIXEV_CALL extension_get_format_description(void* self, utf16_char** result);
        hresult PIXEV_CALL extension_format_static_image(void* self, void* task, void* image_stream, const utf16_char* destination_path);
        hresult PIXEV_CALL extension_format_animated_image(void* self, void* task, void** images_keys, std::int32_t* images_values, const utf16_char* destination_path, std::int32_t images_count);
        hresult PIXEV_CALL extension_format_novel(void* self, void* task, const utf16_char* novel_input, const utf16_char* destination_path, void** images_keys, void** images_values, std::int32_t images_count);
        hresult PIXEV_CALL extension_get_icon(void* self, std::int32_t* result);
        hresult PIXEV_CALL extension_get_label(void* self, utf16_char** result);
        hresult PIXEV_CALL extension_get_description(void* self, utf16_char** result);
        hresult PIXEV_CALL extension_transform_image(void* self, void* task, void* original_stream, void* destination_stream);
        hresult PIXEV_CALL extension_transform_text(void* self, void* task, const utf16_char* original_string, TextTransformerType type);
        hresult PIXEV_CALL extension_get_transform_result(void* self, utf16_char** result);

        hresult PIXEV_CALL setting_query_interface(void* self, const guid& iid, void** result);
        ulong PIXEV_CALL setting_add_ref(void* self);
        ulong PIXEV_CALL setting_release(void* self);
        hresult PIXEV_CALL setting_on_extension_loaded(void* self);
        hresult PIXEV_CALL setting_on_extension_unloaded(void* self);
        hresult PIXEV_CALL setting_get_icon(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_get_label(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_get_description(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_get_description_uri(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_get_token(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_get_placeholder(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_get_settings_type(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_get_bool_default(void* self, abi::bool_abi* result);
        hresult PIXEV_CALL setting_on_bool_value_changed(void* self, abi::bool_abi value);
        hresult PIXEV_CALL setting_get_int_default(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_on_int_value_changed(void* self, std::int32_t value);
        hresult PIXEV_CALL setting_get_int_min(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_get_int_max(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_get_int_step(void* self, std::int32_t* result);
        hresult PIXEV_CALL setting_get_double_default(void* self, double* result);
        hresult PIXEV_CALL setting_get_double_min(void* self, double* result);
        hresult PIXEV_CALL setting_get_double_max(void* self, double* result);
        hresult PIXEV_CALL setting_get_double_step(void* self, double* result);
        hresult PIXEV_CALL setting_on_double_value_changed(void* self, double value);
        hresult PIXEV_CALL setting_get_string_default(void* self, utf16_char** result);
        hresult PIXEV_CALL setting_on_string_value_changed(void* self, const utf16_char* value);
        hresult PIXEV_CALL setting_get_strings_array_default(void* self, std::int32_t* return_count, void*** result);
        hresult PIXEV_CALL setting_on_strings_array_value_changed(void* self, void** values, std::int32_t value_count);
        hresult PIXEV_CALL setting_get_enum_key_values(void* self, void*** return_keys, std::int32_t** return_values, std::int32_t* return_count);
        hresult PIXEV_CALL setting_get_color_default(void* self, std::uint32_t* result);
        hresult PIXEV_CALL setting_on_color_value_changed(void* self, std::uint32_t value);
        hresult PIXEV_CALL setting_get_date_time_offset_default(void* self, std::int64_t* return_utc_date_time_ticks, std::int32_t* return_minutes_offset);
        hresult PIXEV_CALL setting_on_date_time_offset_value_changed(void* self, std::int64_t value_utc_date_time_ticks, std::int32_t value_minutes_offset);

    }
}
