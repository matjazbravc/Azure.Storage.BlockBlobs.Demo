using System;
using System.IO;
using System.Threading.Tasks;

namespace Client.ConsoleApp.Extensions
{
    public static class FileInfoExtensions
    {
        public static async Task<byte[]> GetFileContentAsync(this FileInfo file, long offset, int length)
        {
            await using var stream = file.OpenRead();
            stream.Seek(offset, SeekOrigin.Begin);

            var contents = new byte[length];
            var len = await stream.ReadAsync(contents.AsMemory(0, contents.Length));
            if (len == length)
            {
                return contents;
            }

            var rest = new byte[len];
            Array.Copy(contents, rest, len);
            return rest;
        }
    }
}
