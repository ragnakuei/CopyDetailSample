using System.Collections.Generic;

namespace CopyDetailSample
{
    public class BoxDto
    {
        public Category1 Category1 { get; set; }

        public IEnumerable<Category2> Category2s { get; set; }

        public IEnumerable<Category3> Category3s { get; set; }
    }
}