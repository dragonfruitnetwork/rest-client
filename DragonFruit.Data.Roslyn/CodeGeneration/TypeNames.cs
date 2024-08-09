// ReSharper disable InconsistentNaming

namespace StrawberryShake.CodeGeneration;

public static class TypeNames
{
    public const string IEquatable = "global::System.IEquatable";
    public const string Type = "global::System.Type";
    public const string Nullable = "global::System.Nullable";
    public const string JsonElement = "global::System.Text.Json.JsonElement";
    public const string JsonDocument = "global::System.Text.Json.JsonDocument";
    public const string JsonValueKind = "global::System.Text.Json.JsonValueKind";
    public const string JsonWriterOptions = "global::System.Text.Json.JsonWriterOptions";
    public const string Utf8JsonWriter = "global::System.Text.Json.Utf8JsonWriter";

    public const string String = "global::System.String";
    public const string Byte = "global::System.Byte";
    public const string ByteArray = "global::System.Byte[]";
    public const string Array = "global::System.Array";
    public const string Int16 = "global::System.Int16";
    public const string Int32 = "global::System.Int32";
    public const string Int64 = "global::System.Int64";
    public const string UInt16 = "global::System.UInt16";
    public const string UInt32 = "global::System.UInt32";
    public const string UInt64 = "global::System.UInt64";
    public const string Single = "global::System.Single";
    public const string Double = "global::System.Double";
    public const string Decimal = "global::System.Decimal";
    public const string Uri = "global::System.Uri";
    public const string Boolean = "global::System.Boolean";
    public const string Object = "global::System.Object";
    public const string Guid = "global::System.Guid";
    public const string DateTime = "global::System.DateTime";
    public const string TimeSpan = "global::System.TimeSpan";
    public const string EncodingUtf8 = "global::System.Text.Encoding.UTF8";
    public const string List = GenericCollectionsNamespace + "List";
    public const string IEnumerable = GenericCollectionsNamespace + "IEnumerable";
    public const string Concat = "global::System.Linq.Enumerable.Concat";
    public const string IList = GenericCollectionsNamespace + "IList";

    public const string IReadOnlyCollection = GenericCollectionsNamespace + "IReadOnlyCollection";

    public const string IReadOnlyDictionary = GenericCollectionsNamespace + "IReadOnlyDictionary";

    public const string IReadOnlyList = GenericCollectionsNamespace + "IReadOnlyList";
    public const string HashSet = GenericCollectionsNamespace + "HashSet";
    public const string ISet = GenericCollectionsNamespace + "ISet";
    public const string IReadOnlySpan = "global::System.ReadOnlySpan";
    public const string DateTimeOffset = "global::System.DateTimeOffset";
    public const string OrdinalStringComparison = "global::System.StringComparison.Ordinal";
    public const string Func = "global::System.Func";
    public const string Task = "global::System.Threading.Tasks.Task";
    public const string CancellationToken = "global::System.Threading.CancellationToken";
    public const string NotSupportedException = "global::System.NotSupportedException";
    public const string ArgumentNullException = "global::System.ArgumentNullException";
    public const string ArgumentException = "global::System.ArgumentException";

    public const string ArgumentOutOfRangeException = "global::System.ArgumentOutOfRangeException";

    public const string Exception = "global::System.Exception";

    public const string GenericCollectionsNamespace = "global::System.Collections.Generic.";
    public const string Dictionary = "global::System.Collections.Generic.Dictionary";
    public const string KeyValuePair = "global::System.Collections.Generic.KeyValuePair";
}
