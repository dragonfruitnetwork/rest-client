// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.Buffers;
using Newtonsoft.Json;

#endregion

namespace DragonFruit.Data.Serializers.Newtonsoft
{
    /// <summary>
    /// A wrapper for the <see cref="T:System.Buffers.ArrayPool`1" /> that implements <see cref="T:Newtonsoft.Json.IArrayPool`1" />
    /// </summary>
    /// <remarks>
    /// Taken from https://github.com/JamesNK/Newtonsoft.Json/blob/52e257ee57899296d81a868b32300f0b3cfeacbe/Src/Newtonsoft.Json.Tests/DemoTests.cs#L709
    /// </remarks>
    internal class JsonArrayPool : IArrayPool<char>
    {
        public static readonly JsonArrayPool Instance = new JsonArrayPool();

        public char[] Rent(int minimumLength)
        {
            return ArrayPool<char>.Shared.Rent(minimumLength);
        }

        public void Return(char[] array)
        {
            ArrayPool<char>.Shared.Return(array);
        }
    }
}
