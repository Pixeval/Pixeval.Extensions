namespace Pixeval.Extensions.Generator;

internal static class TypeNames
{
    public static class Pidl
    {
        public const string Bool = "bool";
        public const string Byte = "byte";
        public const string Short = "short";
        public const string UShort = "ushort";
        public const string Int = "int";
        public const string UInt = "uint";
        public const string Long = "long";
        public const string ULong = "ulong";
        public const string Double = "double";
        public const string String = "string";
        public const string Stream = "stream";
        public const string DateTimeOffset = "dateTimeOffset";
        public const string Void = "void";
        public const string IStream = "IStream";
        public const string IProgressNotifier = "IProgressNotifier";
        public const string ITaskCompletionSource = "ITaskCompletionSource";
        public const string IExtension = "IExtension";
        public const string IExtensionsHost = "IExtensionsHost";
        public const string Symbol = nameof(FluentIcons.Common.Symbol);
        public const string ArraySuffix = "[]";
        public const string NullableSuffix = "?";
        public const string NullableString = String + NullableSuffix;
        public const string NullableArraySuffix = ArraySuffix + NullableSuffix;
        public const string EnumTypeSuffix = "Type";
        public const string ByteArray = Byte + ArraySuffix;
        public const string NullableByteArray = Byte + NullableArraySuffix;
        public const string StringArray = String + ArraySuffix;
        public const string StreamArray = Stream + ArraySuffix;
        public const string NullableStreamArray = Stream + NullableArraySuffix;
        public const string IStreamArray = IStream + ArraySuffix;
        public const string NullableIStreamArray = IStream + NullableArraySuffix;
    }

    public static class CSharp
    {
        public const string Int = "int";
        public const string IReadOnlyList = nameof(System.Collections.Generic.IReadOnlyList<object>);
        public const string IReadOnlyDictionary = nameof(System.Collections.Generic.IReadOnlyDictionary<object, object>);
        public const string Stream = nameof(System.IO.Stream);
        public const string Task = nameof(System.Threading.Tasks.Task);
        public const string TaskCompletionSource = nameof(System.Threading.Tasks.TaskCompletionSource);
        public const string DateTimeOffset = nameof(System.DateTimeOffset);
        public const string Void = "void";

        public static string ArrayOf(string elementType)
        {
            return elementType + Pidl.ArraySuffix;
        }

        public static string NullableArrayOf(string elementType)
        {
            return elementType + Pidl.NullableArraySuffix;
        }

        public static string IReadOnlyListOf(string elementType)
        {
            return $"{IReadOnlyList}<{elementType}>";
        }

        public static string IReadOnlyDictionaryOf(string keyType, string valueType)
        {
            return $"{IReadOnlyDictionary}<{keyType}, {valueType}>";
        }

        public static string TaskOf(string resultType)
        {
            return $"{Task}<{resultType}>";
        }
    }

    public static class Cpp
    {
        public const string AbiBool = "abi::bool_abi";
        public const string Bool = "bool";
        public const string Guid = "guid";
        public const string HResult = "hresult";
        public const string UInt8 = "std::uint8_t";
        public const string Int16 = "std::int16_t";
        public const string UInt16 = "std::uint16_t";
        public const string Int32 = "std::int32_t";
        public const string UInt32 = "std::uint32_t";
        public const string Int64 = "std::int64_t";
        public const string UInt64 = "std::uint64_t";
        public const string Double = "double";
        public const string Void = "void";
        public const string VoidPointer = "void*";
        public const string VoidPointerPointer = "void**";
        public const string UInt8Pointer = "std::uint8_t*";
        public const string Int32Pointer = "std::int32_t*";
        public const string UInt32Pointer = "std::uint32_t*";
        public const string StdPair = "std::pair";
        public const string StdSizeT = "std::size_t";
        public const string StdVector = "std::vector";
        public const string Utf16CharPointer = "utf16_char*";
        public const string ConstUtf16CharPointer = "const utf16_char*";
        public const string U16String = "std::u16string";
        public const string OptionalU16String = $"std::optional<{U16String}>";
        public const string U16StringView = "std::u16string_view";
        public const string Stream = "Stream";
        public const string ProgressNotifier = "ProgressNotifier";
        public const string TaskCompletionSource = "TaskCompletionSource";
        public const string DateTimeOffsetValue = "DateTimeOffsetValue";
        public const string PixevalComObject = "PixevalComObject";
        public const string Nullptr = "nullptr";
        public const string NullOpt = "std::nullopt";
        public const string DecltypeNullptr = $"decltype({Nullptr})";

        public static string PointerTo(string type)
        {
            return type + "*";
        }

        public static string TaskOf(string resultType)
        {
            return $"task<{resultType}>";
        }

        public static string PairOf(string firstType, string secondType)
        {
            return $"{StdPair}<{firstType}, {secondType}>";
        }

        public static string VectorOf(string elementType)
        {
            return $"{StdVector}<{elementType}>";
        }
    }

    public static class Python
    {
        public const string Bool = "bool";
        public const string Bytes = "bytes";
        public const string Float = "float";
        public const string Int = "int";
        public const string None = "None";
        public const string Str = "str";
        public const string StrOrNone = "str | None";
        public const string TupleIntInt = "tuple[int, int]";
        public const string AbiHResult = "_abi.HRESULT";
        public const string AbiInterfaceSet = "_abi.InterfaceSet";
        public const string SdkComObject = "SdkComObject";
        public const string AbiComObject = "_abi.ComObject";
        public const string AbiStream = "_abi.Stream";
        public const string AbiProgressNotifier = "_abi.ProgressNotifier";
        public const string AbiTaskCompletionSource = "_abi.TaskCompletionSource";
        public const string AbiVoidPointer = "_abi.VOIDP";
        public const string AbiInt32 = "_abi.INT32";
        public const string AbiUInt32 = "_abi.UINT32";
        public const string AbiInt64 = "_abi.INT64";
        public const string AbiDouble = "_abi.DOUBLE";
        public const string CTypesCFuncPtr = "ctypes._CFuncPtr";
        public const string Symbol = "Symbol";

        public static string PointerTo(string type)
        {
            return $"ctypes.POINTER({type})";
        }

        public static string SequenceOf(string elementType)
        {
            return $"Sequence[{elementType}]";
        }

        public static string ListOf(string elementType)
        {
            return $"list[{elementType}]";
        }
    }

    public static class Literal
    {
        public const string Null = "null";
        public const string Default = "default";
        public const string True = "true";
        public const string False = "false";
    }
}
