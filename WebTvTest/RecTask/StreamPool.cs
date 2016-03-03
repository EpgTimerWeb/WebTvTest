using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTvTest.RecTask
{
    public class StreamPool
    {
        private static StreamPool _instance = null;
        public static StreamPool Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StreamPool();
                return _instance;
            }
            set { _instance = value; }
        }
    }
}
