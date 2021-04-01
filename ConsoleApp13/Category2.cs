using System;

namespace ConsoleApp13
{
    public class Category2
    {
        public int    Id            { get; set; }
        public Guid   Guid          { get; set; }
        public string Name          { get; set; }
        public Guid?  Category1Guid { get; set; }
    }
}