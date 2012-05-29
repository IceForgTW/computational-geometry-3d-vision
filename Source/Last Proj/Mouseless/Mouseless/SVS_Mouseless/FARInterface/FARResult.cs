using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FARInterface
{
    public class FARResult
    {
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private object[] _Params;

        public object[] Params
        {
            get { return _Params; }
            set { _Params = value; }
        }

        public FARResult()
        {
        }
    }
}