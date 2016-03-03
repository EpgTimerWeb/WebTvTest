using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace WebTvTest.RecTask
{
    public class Message
    {
        private static string SerializeParameters(Dictionary<string, object> obj)
        {
            var sb = new StringBuilder();
            foreach (var item in obj)
            {
                if (item.Value == null) continue;
                var value = "";
                switch (Type.GetTypeCode(item.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        value = ((bool)item.Value).ToString().ToLower();
                        break;
                    case TypeCode.String:
                    case TypeCode.Char:
                        value = ((string)item.Value);
                        value = value.Replace("\\", "\\\\");
                        value = value.Replace("\n", "\\n");
                        value = value.Replace("\r", "\\r");
                        value = value.Replace("\t", "\\t");
                        value = "\"" + ((string)item.Value) + "\"";
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        value = item.Value.ToString();
                        break;
                    default:
                        throw new ArgumentException();
                }
                sb.AppendFormat("{0}:{1}\n", item.Key, value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// RecTaskに送信するメッセージを作成します
        /// </summary>
        /// <param name="name">RecTask関数</param>
        /// <param name="obj">パラメータ</param>
        /// <returns>作成されたメッセージ</returns>
        public static string Serialize(string name, Dictionary<string, object> obj = null)
        {
            var sb = new StringBuilder();
            sb.Append(name);
            if (obj != null)
            {
                sb.Append("\n");
                sb.Append(SerializeParameters(obj));
            }
            return sb.ToString();
        }

        private static object ParseVal(string value)
        {
            object res = null;
            value = Util.RemoveStartSpace(value);
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
                value = value.Replace("\\\\", "\\");
                value = value.Replace("\\n", "\n");
                value = value.Replace("\\r", "\r");
                value = value.Replace("\\t", "\t");
                res = value;
            }
            else if (value.ToLower() == "true" || value.ToLower() == "false")
            {
                res = value.ToLower() == "true";
            }
            else
            {
                long tmp = 0;
                if (long.TryParse(value, out tmp))
                {
                    if (tmp < int.MaxValue)
                    {
                        res = (int)tmp;
                    }
                    else
                    {
                        res = tmp;
                    }
                }
            }
            return res;
        }

        private static void _Parse(string name, string value, ref Dictionary<string, object> res)
        {
            if (name.Contains(".") && name.IndexOf(".") < name.Length - 1)
            {
                var parentName = name.Substring(0, name.IndexOf("."));
                var children = name.Substring(name.IndexOf(".") + 1);
                var res2 = res.ContainsKey(parentName) && res[parentName] is System.Collections.IDictionary
                    ? (Dictionary<string, object>)res[parentName] : new Dictionary<string, object>();
                _Parse(children, value, ref res2);
                res[parentName] = res2;
            }
            else
            {
                res[name] = ParseVal(value);
            }
        }

        private static Dictionary<string, object> ParseStep2(Dictionary<string, object> input)
        {
            var res = new Dictionary<string, object>();
            var remove = new List<string>();
            foreach (var item in input)
            {
                if (item.Key.EndsWith("Count") && item.Key.Length > 5 && item.Value is int)
                {
                    var arrayName = item.Key.Substring(0, item.Key.Length - 5);
                    var arrayCount = (int)item.Value;
                    var isValid = true;
                    for (int i = 0; i < arrayCount; i++)
                    {
                        Debug.Print("check {0}", i);
                        if (!input.ContainsKey(arrayName + i))
                        {
                            isValid = false;
                            continue;
                        }
                    }
                    if (!isValid)
                    {
                        res[item.Key] = item.Value;
                        continue;
                    }
                    var array = new List<object>();
                    for (int i = 0; i < arrayCount; i++)
                    {
                        var arrayItem = input[arrayName + i];
                        array.Add(arrayItem is System.Collections.IDictionary ? 
                            ParseStep2((Dictionary<string, object>)arrayItem) : arrayItem);
                        remove.Add(arrayName + i);
                    }
                    res[arrayName] = array;
                    remove.Add(item.Key);
                }
                else if (item.Value is System.Collections.IDictionary)
                {
                    res[item.Key] = ParseStep2((Dictionary<string, object>)item.Value);
                }
                else
                {
                    res[item.Key] = item.Value;
                }
            }
            foreach (var r in remove)
                res.Remove(r);
            return res;
        }
        /// <summary>
        /// RecTaskからのメッセージを解析します
        /// </summary>
        /// <param name="Input">RecTaskからのメッセージ</param>
        /// <returns>解析結果</returns>
        public static Dictionary<string, object> Parse(string input)
        {
            var lines = input.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var res = new Dictionary<string, object>();
            foreach (var line in lines)
            {
                if (!line.Contains(":")) continue;
                var name = line.Substring(0, line.IndexOf(":"));
                if (name.Length + 1 >= line.Length) continue;
                var value = line.Substring(line.IndexOf(":") + 1);
                _Parse(name, value, ref res);
            }
            return ParseStep2(res);
        }
    }
}
