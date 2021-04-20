using System.Collections.Generic;

namespace Common.Contacts.Models
{
    public class FileMetadata
    {
        public string FileName { get; set; }

        public long Size { get; set; }

        public IDictionary<string, string> ToStorageMetadata()
        {
            var result = new Dictionary<string, string>
            {
                ["fileName"] = FileName,
                ["originalSize"] = Size.ToString()
            };
            return result;
        }

        public static FileMetadata FromStorageMetadata(IDictionary<string, string> metadata)
        {
            var result = new FileMetadata
            {
                FileName = metadata["fileName"],
                Size = long.Parse(metadata["originalSize"])
            };
            return result;
        }

        public FileMetadata ToStorage()
        {
            var result = new FileMetadata
            {
                FileName = FileName,
                Size = Size
            };
            return result;
        }
    }
}