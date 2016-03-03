using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTvTest
{
    public class Manager
    {
        private static Manager _instance = null;
        public static Manager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Manager();
                return _instance;
            }
            set { _instance = value; }
        }

        public Manager()
        {
            Configure = new Configure();
        }
        public Configure Configure { set; get; }
    }
}
