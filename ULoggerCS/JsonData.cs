using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULoggerCS.Utility;

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

    /**
     * JSON形式のデータを作成するクラス
     * 
     * JSONなのでツリー構造を作ることができる。ツリー構造を作るには、JsonDataのobjに ArrayやDictionaryのデータを設定して、
     * それらの各要素に JsonData を追加してやる。
     * 例:
     * JsonData(Array) +---- JsonData(Array) +---- String
     *                 |                     +---- String
     *                 |                     +---- String
     *                 +---- JsonData(Dictionary) +---- <Key,Value>
     *                                            +---- <Key,Value>
     *                                            +---- <Key,Value>
     */
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
                        if (cnt > 0)
                        {
                            sb.Append(",");
                        }
                        if (value is JsonData)
                        {
                            JsonData json = (JsonData)value;
                            ObjToString(sb, json);
                        }
                        else
                        {
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
                        if (cnt > 0)
                        {
                            sb.Append(",");
                        }

                        // "キー名":
                        sb.AppendFormat("\"{0}\":", kvp.Key);

                        if (kvp.Value is JsonData)
                        {
                            JsonData json = (JsonData)kvp.Value;
                            ObjToString(sb, json);
                        }
                        else 
                        {
                            if (kvp.Value is string)
                            {
                                sb.AppendFormat("\"{0}\"", kvp.Value);
                            }
                            else
                            {
                                sb.Append(kvp.Value);
                            }
                            
                        }
                        cnt++;
                    }
                    sb.Append("}");
                    break;
            }
        }

        /**
         * バイナリに変換する
         * ※バイナリにするメリットがないため未使用
         */
#if false
        public byte[] ToBinary()
        {
            var list = new UByteList();

            ObjToBinary(list, this);

            return list.ToArray();
        }

        private static byte[] ObjToBinary(UByteList list, JsonData jsonData)
        {
            int cnt = 0;

            switch (jsonData.dataType)
            {
                case JsonDataType.String:
                    list.AddByte((byte)JsonDataType.String);
                    list.AddSizeString((string)jsonData.obj);
                    break;
                case JsonDataType.Array:
                    list.AddByte((byte)JsonDataType.Array);
                    list.AddInt32(((object[])jsonData.Obj).Length);

                    foreach (object value in (jsonData.Obj as object[]))
                    {
                        if (value is JsonData)
                        {
                            JsonData json = (JsonData)value;
                            ObjToBinary(list, json);
                        }
                        else
                        {
                            list.AddObject(value);
                        }
                        cnt++;
                    }
                    break;
                case JsonDataType.Dictionary:
                    // データ種別
                    list.AddByte((byte)JsonDataType.Dictionary);

                    // 辞書型の項目数
                    list.AddInt32(((Dictionary<string, object>)jsonData.Obj).Count);

                    foreach (KeyValuePair<string, object> kvp in (jsonData.Obj as Dictionary<string, object>))
                    {
                        // キー文字列
                        list.AddSizeString(kvp.Key);
                        
                        if (kvp.Value is JsonData)
                        {
                            // 配下のJsonを再帰呼び出し
                            JsonData json = (JsonData)kvp.Value;
                            ObjToBinary(list,json);
                        }
                        else
                        {
                            list.AddObject(kvp.Value);
                        }
                        cnt++;
                    }
                    break;
            }

            return list.ToArray();
        }
#endif
    }
}
