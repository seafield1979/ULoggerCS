using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    //
    // Enums
    //
    // ノードのデータ種別
    enum JsonDataType : byte
    {
        String = 0,
        Array,
        Dictionary
    }

    class JsonData
    {
        //
        // Properties
        // 
        // 文字列、配列、辞書型のどれからのオブジェクトが入る
        private Object obj;

        public Object Obj
        {
            get { return obj; }
            set { obj = value; }
        }

        // objに入っているデータの種類
        private JsonDataType dataType;

        public JsonDataType DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }


        //
        // Methods
        //
        public void AddString(string str)
        {
            dataType = JsonDataType.String;
            obj = str;
        }

        public void AddArray(object[] array)
        {
            dataType = JsonDataType.Array;
            obj = array;
        }

        public void AddDictionary(Dictionary<string, object> dic)
        {
            dataType = JsonDataType.Dictionary;
            obj = dic;
        }

        /**
         * 文字列に変換
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ObjToString(sb, this);

            return sb.ToString();
        }

        /**
         * １階層分のリストを文字列に変換する
         * 
         * @input sb:    変換した結果を格納する先
         * @input value: 変換元のデータ(string、array、dictionaryのどれか)
         * @input nest:  階層をネストするたびに文字列の先頭に付加するスペース
         */
        private static void ObjToString(StringBuilder sb, JsonData jsonData)
        {
            int cnt = 0;
            
            switch (jsonData.dataType)
            {
                case JsonDataType.String:
                    sb.AppendFormat("\"{0}\"", (string)jsonData.Obj);
                    break;
                case JsonDataType.Array:
                    sb.Append("[");
                    foreach (object value in (jsonData.Obj as object[]))
                    {
                        if (value is JsonData)
                        {
                            JsonData json = (JsonData)value;
                            if (json.DataType == JsonDataType.Dictionary)
                            {
                                ObjToString(sb, json);
                            }
                            else if (json.DataType == JsonDataType.Array)
                            {
                                ObjToString(sb, json);
                            }
                        }
                        //else if (value is string)
                        else
                        {
                            if (cnt > 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(value);
                        }
                        cnt++;
                    }
                    sb.Append("]");

                    break;
                case JsonDataType.Dictionary:
                    sb.Append("{");
                    foreach (KeyValuePair<string, object> kvp in (jsonData.Obj as Dictionary<string, object>))
                    {
                        if (kvp.Value is JsonData)
                        {
                            if (cnt > 0)
                            {
                                sb.Append(",");
                            }
                            sb.AppendFormat("\"{0}\":", kvp.Key);
                            JsonData json = (JsonData)kvp.Value;
                            if (json.DataType == JsonDataType.Dictionary)
                            {
                                ObjToString(sb, json);
                            }
                            else if (json.DataType == JsonDataType.Array)
                            {
                                ObjToString(sb, json);
                            }
                        }
                        else 
                        {
                            if (cnt > 0)
                            {
                                sb.Append(",");
                            }
                            if (kvp.Value is string)
                            {
                                sb.AppendFormat("\"{0}\":\"{1}\"", kvp.Key, kvp.Value);
                            }
                            else
                            {
                                sb.AppendFormat("\"{0}\":{1}", kvp.Key, kvp.Value);
                            }
                            
                        }
                        cnt++;
                    }
                    sb.Append("}");
                    break;
            }
        }
    }
}
