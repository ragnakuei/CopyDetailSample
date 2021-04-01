using System;

namespace CopyDetailSample
{
    public class Category3
    {
        public int    Id            { get; set; }
        public Guid   Guid          { get; set; }
        public string Name          { get; set; }
        public Guid?  Category2Guid { get; set; }
    }
}