#pragma once
#include <pixeval/detail/extensions.sdk.core.inl.hpp>

namespace pixeval::extensions::detail
{
        inline hresult PIXEV_CALL setting_query_interface(void* self, const guid& iid, void** result)
        {
            if (result == nullptr)
                return E_POINTER;

            auto& setting = setting_owner(self);
            *result = nullptr;
            if (!setting_supports_guid(setting.kind(), iid))
                return E_NOINTERFACE;

            *result = self;
            setting_add_ref(self);
            return S_OK;
        }

        inline ulong PIXEV_CALL setting_add_ref(void* self)
        {
            return abi::add_ref(object_from_native<setting_object>(self)->references);
        }

        inline ulong PIXEV_CALL setting_release(void* self)
        {
            return abi::release_ref(object_from_native<setting_object>(self)->references);
        }

        inline hresult PIXEV_CALL setting_on_extension_loaded(void* self)
        {
            static_cast<SettingsExtensionBase&>(setting_owner(self)).on_extension_loaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_extension_unloaded(void* self)
        {
            static_cast<SettingsExtensionBase&>(setting_owner(self)).on_extension_unloaded();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_icon(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<std::int32_t>(static_cast<SettingsExtensionBase&>(setting_owner(self)).icon());
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_label(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<SettingsExtensionBase&>(setting_owner(self)).label(), result);
        }

        inline hresult PIXEV_CALL setting_get_description(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<SettingsExtensionBase&>(setting_owner(self)).description(), result);
        }

        inline hresult PIXEV_CALL setting_get_description_uri(void* self, utf16_char** result)
        {
            return abi::copy_optional_utf16(static_cast<SettingsExtensionBase&>(setting_owner(self)).description_uri(), result);
        }

        inline hresult PIXEV_CALL setting_get_token(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<SettingsExtensionBase&>(setting_owner(self)).token(), result);
        }

        inline hresult PIXEV_CALL setting_get_placeholder(void* self, utf16_char** result)
        {
            return abi::copy_optional_utf16(static_cast<SettingsExtensionBase&>(setting_owner(self)).placeholder(), result);
        }

        inline hresult PIXEV_CALL setting_get_settings_type(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<std::int32_t>(static_cast<SettingsExtensionBase&>(setting_owner(self)).settings_type());
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_bool_default(void* self, abi::bool_abi* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = abi::bool_to_abi(static_cast<BoolSettingsExtensionBase&>(setting_owner(self)).default_value());
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_bool_value_changed(void* self, abi::bool_abi value)
        {
            static_cast<BoolSettingsExtensionBase&>(setting_owner(self)).on_value_changed(abi::bool_from_abi(value));
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_int_default(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            auto& setting = setting_owner(self);
            *result = setting.kind() == setting_kind::enumeration
                ? static_cast<EnumSettingsExtensionBase&>(setting).default_value()
                : static_cast<IntSettingsExtensionBase&>(setting).default_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_int_value_changed(void* self, std::int32_t value)
        {
            auto& setting = setting_owner(self);
            if (setting.kind() == setting_kind::enumeration)
                static_cast<EnumSettingsExtensionBase&>(setting).on_value_changed(value);
            else
                static_cast<IntSettingsExtensionBase&>(setting).on_value_changed(value);
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_int_min(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<IntSettingsExtensionBase&>(setting_owner(self)).min_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_int_max(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<IntSettingsExtensionBase&>(setting_owner(self)).max_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_int_step(void* self, std::int32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<IntSettingsExtensionBase&>(setting_owner(self)).step_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_double_default(void* self, double* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<DoubleSettingsExtensionBase&>(setting_owner(self)).default_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_double_min(void* self, double* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<DoubleSettingsExtensionBase&>(setting_owner(self)).min_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_double_max(void* self, double* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<DoubleSettingsExtensionBase&>(setting_owner(self)).max_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_double_step(void* self, double* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<DoubleSettingsExtensionBase&>(setting_owner(self)).step_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_double_value_changed(void* self, double value)
        {
            static_cast<DoubleSettingsExtensionBase&>(setting_owner(self)).on_value_changed(value);
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_string_default(void* self, utf16_char** result)
        {
            return abi::copy_utf16(static_cast<StringSettingsExtensionBase&>(setting_owner(self)).default_value(), result);
        }

        inline hresult PIXEV_CALL setting_on_string_value_changed(void* self, const utf16_char* value)
        {
            static_cast<StringSettingsExtensionBase&>(setting_owner(self)).on_value_changed(abi::to_u16string(value));
            return S_OK;
        }

        inline hresult copy_string_array(std::span<const std::u16string> values, std::int32_t* count, void*** result)
        {
            if (count == nullptr || result == nullptr)
                return E_POINTER;

            auto array = static_cast<void**>(abi::allocate_bytes(sizeof(void*) * values.size()));
            if (array == nullptr && !values.empty())
                return E_OUTOFMEMORY;

            for (std::size_t i = 0; i < values.size(); ++i)
            {
                auto hr = abi::copy_utf16(values[i], reinterpret_cast<utf16_char**>(&array[i]));
                if (hr != S_OK)
                {
                    for (std::size_t j = 0; j < i; ++j)
                        abi::free_bytes(array[j]);
                    abi::free_bytes(array);
                    return hr;
                }
            }

            *count = static_cast<std::int32_t>(values.size());
            *result = array;
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_strings_array_default(void* self, std::int32_t* return_count, void*** result)
        {
            auto values = static_cast<StringsArraySettingsExtensionBase&>(setting_owner(self)).default_value();
            return copy_string_array(values, return_count, result);
        }

        inline hresult PIXEV_CALL setting_on_strings_array_value_changed(void* self, void** values, std::int32_t value_count)
        {
            std::vector<std::u16string> managed_values;
            managed_values.reserve(static_cast<std::size_t>(value_count));
            for (std::int32_t i = 0; i < value_count; ++i)
                managed_values.push_back(abi::to_u16string(static_cast<const utf16_char*>(values[i])));

            static_cast<StringsArraySettingsExtensionBase&>(setting_owner(self)).on_value_changed(std::move(managed_values));
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_enum_key_values(void* self, void*** return_keys, std::int32_t** return_values, std::int32_t* return_count)
        {
            if (return_keys == nullptr || return_values == nullptr || return_count == nullptr)
                return E_POINTER;

            auto values = static_cast<EnumSettingsExtensionBase&>(setting_owner(self)).enum_key_values();
            std::vector<std::u16string> option_names;
            option_names.reserve(values.size());
            for (const auto& value : values)
                option_names.push_back(value.first);

            auto hr = copy_string_array(option_names, return_count, return_keys);
            if (hr != S_OK)
                return hr;

            auto value_array = static_cast<std::int32_t*>(abi::allocate_bytes(sizeof(std::int32_t) * values.size()));
            if (value_array == nullptr && !values.empty())
            {
                for (std::int32_t i = 0; i < *return_count; ++i)
                    abi::free_bytes((*return_keys)[i]);
                abi::free_bytes(*return_keys);
                *return_keys = nullptr;
                *return_count = 0;
                return E_OUTOFMEMORY;
            }

            for (std::size_t i = 0; i < values.size(); ++i)
                value_array[i] = values[i].second;

            *return_values = value_array;
            *return_count = static_cast<std::int32_t>(values.size());
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_color_default(void* self, std::uint32_t* result)
        {
            if (result == nullptr)
                return E_POINTER;

            *result = static_cast<ColorSettingsExtensionBase&>(setting_owner(self)).default_value();
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_color_value_changed(void* self, std::uint32_t value)
        {
            static_cast<ColorSettingsExtensionBase&>(setting_owner(self)).on_value_changed(value);
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_get_date_time_offset_default(void* self, std::int64_t* return_utc_date_time_ticks, std::int32_t* return_minutes_offset)
        {
            if (return_utc_date_time_ticks == nullptr || return_minutes_offset == nullptr)
                return E_POINTER;

            auto value = static_cast<DateTimeOffsetSettingsExtensionBase&>(setting_owner(self)).default_value();
            *return_utc_date_time_ticks = value.utc_date_time_ticks;
            *return_minutes_offset = value.minutes_offset;
            return S_OK;
        }

        inline hresult PIXEV_CALL setting_on_date_time_offset_value_changed(void* self, std::int64_t value_utc_date_time_ticks, std::int32_t value_minutes_offset)
        {
            static_cast<DateTimeOffsetSettingsExtensionBase&>(setting_owner(self)).on_value_changed({value_utc_date_time_ticks, value_minutes_offset});
            return S_OK;
        }
}
