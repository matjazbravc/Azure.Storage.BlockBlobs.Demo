namespace Common.Contacts.Models
{
    using System;

    public class BlockMetadata
    {
        public BlockMetadata(int id, int blockSize, long fileLength)
        {
            Id = id;
            Name = Convert.ToBase64String(BitConverter.GetBytes(Id));
            Index = Id * ((long)blockSize);
            Size = (int)Math.Min(fileLength - Index, blockSize);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public long Index { get; set; }

        public int Size { get; set; }
    }
}