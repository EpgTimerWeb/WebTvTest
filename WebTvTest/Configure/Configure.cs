using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WebTvTest
{
    public class Configure
    {
        private static Configure _instance = null;
        public static Configure Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Configure();
                return _instance;
            }
            set { _instance = value; }
        }

        public Configure()
        {
            WebPort = 8080;
        }
        public int WebPort { set; get; }
        public void SaveToXmlFile(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write, FileShare.None))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Configure));
                    xs.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false);
            }
        }
        public static Configure LoadFromXmlFile(string path)
        {
            Configure config = null;
            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException();
                using (FileStream fs = new FileStream(path,
                   FileMode.Open,
                   FileAccess.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Configure));
                    object obj = xs.Deserialize(fs);
                    config = (Configure)obj;
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Config Error: {0}", ex.Message);
            }
            finally
            {
                if (config.WebPort == 0) config.WebPort = 8080;
            }
            return config;
        }
    }
}
