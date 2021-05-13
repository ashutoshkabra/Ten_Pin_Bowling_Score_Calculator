#region Using Namespaces

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

#endregion

namespace Score_Calculator.Models
{
    public class GameStatus
    {
        public string[] frameProgressScores { get; set; }

        public bool gameCompleted { get; set; }
    }
}