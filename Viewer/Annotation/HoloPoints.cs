using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer
{
    public sealed class HoloPoints
    {
        private static readonly HoloPoints instance = new HoloPoints();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HoloPoints()
        {
        }

        private HoloPoints()
        {
        }

        public static HoloPoints Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
