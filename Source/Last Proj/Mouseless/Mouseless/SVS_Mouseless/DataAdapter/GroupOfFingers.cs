using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAdapter
{
    /// <summary>
    /// GroupOfFingers consists of 1-5 fingertips
    /// </summary>
    public class GroupOfFingers
    {
        private Fingertip[] _Fingertips;        // list of fingertips
        private int _N;                         // Number of fingertips

        public Fingertip[] Fingertips
        {
            get { return _Fingertips; }
            set { _Fingertips = value; }
        }

        public int N
        {
            get { return _N; }
            set { _N = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Fingertips">list of fingertips</param>
        /// <param name="N">number of fingertips</param>
        public GroupOfFingers(Fingertip[] Fingertips, int N)
        {
            _Fingertips = Fingertips;
            _N = N;
        }


        static public GroupOfFingers[] FromArray()
        {
            GroupOfFingers[] rsl = new GroupOfFingers[10];
            return rsl;
        }
    }
}