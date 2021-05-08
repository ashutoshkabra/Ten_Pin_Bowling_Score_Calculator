#region Using Namespaces

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

#endregion

namespace Score_Calculator.Models
{
    public class BowlingFrame
    {
        public int Throw1 { get; set; }
        public int? Throw2 { get; set; }
        public int? ExtraThrow { get; set; }
    }
}