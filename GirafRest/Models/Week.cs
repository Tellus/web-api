using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GirafRest.Models.DTOs;

namespace GirafRest.Models
{
    /// <summary>
    /// A week defines the schedule of some citizen in the course of the week.
    /// </summary>
    public class Week : WeekBase
    {
        /// <summary>
        /// The year of the week.
        /// </summary>
        public int WeekYear { get; set; }
        /// <summary>
        /// The number of the week, 0 - 52 (53).
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// DO NOT DELETE
        /// </summary>
        public Week() : base()
        {

        }

        public Week(Pictogram thumbnail) : base(thumbnail)
        {
            
        }
    }
}