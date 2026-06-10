#pragma once

#include <pixeval/extensions.common.g.hpp>

namespace pixeval::extensions
{
    class NativeComPtr
    {
    public:
        NativeComPtr() = default;

        explicit NativeComPtr(void* native, bool add_ref = true) noexcept : native_{native}
        {
            if (add_ref)
                abi::retain_unknown(native_);
        }

        NativeComPtr(const NativeComPtr& other) noexcept : native_{other.native_}
        {
            abi::retain_unknown(native_);
        }

        NativeComPtr(NativeComPtr&& other) noexcept : native_{std::exchange(other.native_, nullptr)}
        {
        }

        NativeComPtr& operator=(const NativeComPtr& other) noexcept
        {
            if (this == &other)
                return *this;

            abi::retain_unknown(other.native_);
            abi::release_unknown(native_);
            native_ = other.native_;
            return *this;
        }

        NativeComPtr& operator=(NativeComPtr&& other) noexcept
        {
            if (this == &other)
                return *this;

            abi::release_unknown(native_);
            native_ = std::exchange(other.native_, nullptr);
            return *this;
        }

        virtual ~NativeComPtr()
        {
            abi::release_unknown(native_);
        }

        [[nodiscard]] bool has_value() const noexcept
        {
            return native_ != nullptr;
        }

        [[nodiscard]] void* native() const noexcept
        {
            return native_;
        }

    protected:
        [[nodiscard]] void** vtable() const noexcept
        {
            return abi::vtable_from_native(native_);
        }

    private:
        void* native_ = nullptr;
    };

    struct Logger
    {
        Logger() = default;

        explicit Logger(void* native, bool add_ref = true) noexcept : native_{native}
        {
            if (add_ref)
                abi::retain_unknown(native_);
        }

        Logger(const Logger& other) noexcept : native_{other.native_}
        {
            abi::retain_unknown(native_);
        }

        Logger(Logger&& other) noexcept : native_{std::exchange(other.native_, nullptr)}
        {
        }

        Logger& operator=(const Logger& other) noexcept
        {
            if (this == &other)
                return *this;

            abi::retain_unknown(other.native_);
            abi::release_unknown(native_);
            native_ = other.native_;
            return *this;
        }

        Logger& operator=(Logger&& other) noexcept
        {
            if (this == &other)
                return *this;

            abi::release_unknown(native_);
            native_ = std::exchange(other.native_, nullptr);
            return *this;
        }

        ~Logger()
        {
            abi::release_unknown(native_);
        }

        [[nodiscard]] bool has_value() const noexcept
        {
            return native_ != nullptr;
        }

        hresult log(
            LogLevel level,
            std::u16string_view message,
            std::source_location location = std::source_location::current()) const
        {
            if (native_ == nullptr)
                return S_OK;

            auto member_name = abi::ascii_to_u16string(location.function_name());
            auto file_path = abi::ascii_to_u16string(location.file_name());
            std::u16string message_copy{message};

            auto unknown = static_cast<abi::unknown_object*>(native_);
            auto vtable = reinterpret_cast<void**>(unknown->vtable);
            auto log_fn = reinterpret_cast<hresult(PIXEV_CALL*)(
                void*,
                LogLevel,
                const utf16_char*,
                void*,
                const utf16_char*,
                const utf16_char*,
                std::int32_t)>(vtable[static_cast<std::int32_t>(abi::logger_slot::log)]);

            return log_fn(
                native_,
                level,
                message_copy.c_str(),
                nullptr,
                member_name.c_str(),
                file_path.c_str(),
                static_cast<std::int32_t>(location.line()));
        }

    private:
        void* native_ = nullptr;
    };

    class TaskCompletionSource : public NativeComPtr
    {
    public:
        using NativeComPtr::NativeComPtr;

        hresult set_completed() const
        {
            if (!has_value())
                return S_OK;

            auto complete_fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*)>(
                vtable()[static_cast<std::int32_t>(abi::task_completion_source_slot::set_completed)]);
            return complete_fn(native());
        }
    };

    template <typename TResult = void>
    class task
    {
    public:
        struct promise_type
        {
            std::optional<TResult> result;
            std::exception_ptr exception;
            std::coroutine_handle<> continuation;

            task get_return_object() noexcept
            {
                return task{std::coroutine_handle<promise_type>::from_promise(*this)};
            }

            std::suspend_always initial_suspend() noexcept
            {
                return {};
            }

            auto final_suspend() noexcept
            {
                struct awaiter
                {
                    [[nodiscard]] bool await_ready() noexcept
                    {
                        return false;
                    }

                    std::coroutine_handle<> await_suspend(std::coroutine_handle<promise_type> handle) noexcept
                    {
                        auto continuation = handle.promise().continuation;
                        return continuation ? continuation : std::noop_coroutine();
                    }

                    void await_resume() noexcept
                    {
                    }
                };

                return awaiter{};
            }

            template <typename TValue>
            void return_value(TValue&& value)
            {
                result.emplace(std::forward<TValue>(value));
            }

            void unhandled_exception() noexcept
            {
                exception = std::current_exception();
            }
        };

        using handle_type = std::coroutine_handle<promise_type>;

        task() noexcept = default;

        explicit task(handle_type handle) noexcept : handle_{handle}
        {
        }

        task(const task&) = delete;
        task& operator=(const task&) = delete;

        task(task&& other) noexcept : handle_{std::exchange(other.handle_, {})}
        {
        }

        task& operator=(task&& other) noexcept
        {
            if (this == &other)
                return *this;

            if (handle_)
                handle_.destroy();

            handle_ = std::exchange(other.handle_, {});
            return *this;
        }

        ~task()
        {
            if (handle_)
                handle_.destroy();
        }

        [[nodiscard]] bool await_ready() const noexcept
        {
            return !handle_ || handle_.done();
        }

        std::coroutine_handle<> await_suspend(std::coroutine_handle<> continuation) noexcept
        {
            handle_.promise().continuation = continuation;
            return handle_;
        }

        TResult await_resume()
        {
            if (!handle_)
                return {};

            auto& promise = handle_.promise();
            if (promise.exception)
                std::rethrow_exception(promise.exception);

            if (!promise.result)
                return {};

            return std::move(*promise.result);
        }

    private:
        handle_type handle_{};
    };

    template <>
    class task<void>
    {
    public:
        struct promise_type
        {
            std::exception_ptr exception;
            std::coroutine_handle<> continuation;

            task get_return_object() noexcept
            {
                return task{std::coroutine_handle<promise_type>::from_promise(*this)};
            }

            std::suspend_always initial_suspend() noexcept
            {
                return {};
            }

            auto final_suspend() noexcept
            {
                struct awaiter
                {
                    [[nodiscard]] bool await_ready() noexcept
                    {
                        return false;
                    }

                    std::coroutine_handle<> await_suspend(std::coroutine_handle<promise_type> handle) noexcept
                    {
                        auto continuation = handle.promise().continuation;
                        return continuation ? continuation : std::noop_coroutine();
                    }

                    void await_resume() noexcept
                    {
                    }
                };

                return awaiter{};
            }

            void return_void() noexcept
            {
            }

            void unhandled_exception() noexcept
            {
                exception = std::current_exception();
            }
        };

        using handle_type = std::coroutine_handle<promise_type>;

        task() noexcept = default;

        explicit task(handle_type handle) noexcept : handle_{handle}
        {
        }

        task(const task&) = delete;
        task& operator=(const task&) = delete;

        task(task&& other) noexcept : handle_{std::exchange(other.handle_, {})}
        {
        }

        task& operator=(task&& other) noexcept
        {
            if (this == &other)
                return *this;

            if (handle_)
                handle_.destroy();

            handle_ = std::exchange(other.handle_, {});
            return *this;
        }

        ~task()
        {
            if (handle_)
                handle_.destroy();
        }

        [[nodiscard]] bool await_ready() const noexcept
        {
            return !handle_ || handle_.done();
        }

        std::coroutine_handle<> await_suspend(std::coroutine_handle<> continuation) noexcept
        {
            handle_.promise().continuation = continuation;
            return handle_;
        }

        void await_resume()
        {
            if (!handle_)
                return;

            if (auto exception = handle_.promise().exception)
                std::rethrow_exception(exception);
        }

    private:
        handle_type handle_{};
    };

    class ProgressNotifier : public NativeComPtr
    {
    public:
        using NativeComPtr::NativeComPtr;

        hresult progress_changed(double progress) const
        {
            if (!has_value())
                return S_OK;

            auto progress_fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, double)>(
                vtable()[static_cast<std::int32_t>(abi::progress_notifier_slot::progress_changed)]);
            return progress_fn(native(), progress);
        }

        hresult completed() const
        {
            if (!has_value())
                return S_OK;

            auto completed_fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*)>(
                vtable()[static_cast<std::int32_t>(abi::progress_notifier_slot::completed)]);
            return completed_fn(native());
        }
    };

    class Stream : public NativeComPtr
    {
    public:
        using NativeComPtr::NativeComPtr;

        [[nodiscard]] bool can_read() const
        {
            abi::bool_abi value = abi::bool_false;
            if (has_value())
            {
                auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, abi::bool_abi*)>(
                    vtable()[static_cast<std::int32_t>(abi::stream_slot::get_can_read)]);
                (void) fn(native(), &value);
            }
            return abi::bool_from_abi(value);
        }

        [[nodiscard]] bool can_write() const
        {
            abi::bool_abi value = abi::bool_false;
            if (has_value())
            {
                auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, abi::bool_abi*)>(
                    vtable()[static_cast<std::int32_t>(abi::stream_slot::get_can_write)]);
                (void) fn(native(), &value);
            }
            return abi::bool_from_abi(value);
        }

        [[nodiscard]] std::int64_t position() const
        {
            std::int64_t value = 0;
            if (has_value())
            {
                auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, std::int64_t*)>(
                    vtable()[static_cast<std::int32_t>(abi::stream_slot::get_position)]);
                (void) fn(native(), &value);
            }
            return value;
        }

        hresult set_position(std::int64_t value) const
        {
            if (!has_value())
                return S_OK;

            auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, std::int64_t)>(
                vtable()[static_cast<std::int32_t>(abi::stream_slot::set_position)]);
            return fn(native(), value);
        }

        [[nodiscard]] std::int64_t length() const
        {
            std::int64_t value = 0;
            if (has_value())
            {
                auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, std::int64_t*)>(
                    vtable()[static_cast<std::int32_t>(abi::stream_slot::get_length)]);
                (void) fn(native(), &value);
            }
            return value;
        }

        hresult copy_to(const Stream& destination, std::int32_t buffer_size = -1) const
        {
            if (!has_value())
                return S_OK;

            auto fn = reinterpret_cast<hresult(PIXEV_CALL*)(void*, void*, std::int32_t)>(
                vtable()[static_cast<std::int32_t>(abi::stream_slot::copy_to)]);
            return fn(native(), destination.native(), buffer_size);
        }
    };
}