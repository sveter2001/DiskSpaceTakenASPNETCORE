using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiskSpaceWebUI.Models
{
    public class DirOrFile
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Weight { get; set; }
        public long DiskSpaceTaken { get; set; }

    }
}
