using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    /**
     * Json形式のデータ
     * 
     * key = string, value = string
     * { "key" : "value" }
     * 
     * key = string, value = numeric value
     * { "key" : value }
     * 
     * dictionary
     * { "key1" : "value1", "key2" : "value2" ...}
     * 
     * array
     * [ value1, value2, value3, ... ]
     * 
     * tree
     * { "key1" : {"key1-1" : "value", "key1-2" : "value" }, "key2" : {"key1-1" : "value", "key1-2" : "value" }}
     */

    /**
     * Dictionaryのデータを読み込む際のモード
     */
    enum EJsonReadMode: int
    {
        None = 0,
        DicKey,     // 辞書型のキーを読み込み中
        DicValue,       // 辞書型の値を読み込み中
        Array           // 配列の値を読み込み中
    }

    //
    // Enums
    //
    enum DataType : byte
    {
        String = 0,
        Array,
        Dictionary
    }

    class MemJsonData
    {
        // 
        // Properties
        //
        private Object obj;

        public Object Obj
        {
            get { return obj; }
            set { obj = value; }
        }

        private DataType dataType;

        public DataType DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        //
        // Constructor
        //
        public MemJsonData()
        {
            
        }

        //
        // Methods
        //
        /**
         * JSON形式の文字列をメモリ上のツリー構造に展開する
         * 
         * @input jsonStr : JSON形式の文字列
         * @output : メモリ上のツリー構造のオブジェクト
         */
        public static MemJsonData Deserialize(string jsonStr)
        {
            char[] chars = jsonStr.ToCharArray();
            int offset = 0;

            return GetJson(chars, ref offset);
        }

        /**
         * １階層分のJSONデシリアライズを行う
         * 
         * @input chars : シリアライズ対象のJSON形式のキャラ配列
         * @input offset : chars の参照位置
         */
        private static MemJsonData GetJson(char[] chars,ref int offset)
        {
            string keyStr = null;
            EJsonReadMode readMode = EJsonReadMode.None;
            var cbuf = new List<char>();
            object value = null;
            MemJsonData topData = new MemJsonData();
            
            for (; offset < chars.Length; offset++) 
            {
                char c = chars[offset];

                if (c == ' ' || c == '\n')
                {
                    // スペースと改行はスキップ
                    continue;
                }

                switch (readMode)
                {
                    case EJsonReadMode.None:
                        if (c == '"')
                        {
                            topData.dataType = DataType.String;
                            topData.Obj = GetString(chars, ref offset, false);
                        }
                        else if(c == '[')
                        {
                            topData.dataType = DataType.Array;
                            topData.Obj = new List<object>();
                            readMode = EJsonReadMode.Array;
                        }
                        else if (c == '{')
                        {
                            topData.dataType = DataType.Dictionary;
                            topData.Obj = new Dictionary<string, object>();
                            readMode = EJsonReadMode.DicKey;
                        }
                        break;
                    case EJsonReadMode.DicKey:
                        switch(c)
                        {
                            case '"':
                                offset++;
                                keyStr = GetString(chars, ref offset, false);
                                break;
                            case ':':
                                readMode = EJsonReadMode.DicValue;
                                break;
                            case ',':
                                // 値を取得前なので、キーのみ
                                if (keyStr != null)
                                {
                                    ((Dictionary<string, object>)topData.Obj)[keyStr] = "";
                                }
                                break;
                            case ']':
                                return topData;
                        }
                        break;
                    case EJsonReadMode.DicValue:
                        if (keyStr == null)
                        {
                            // キーがない場合は値を取得しても意味がないのでスキップ
                            for(; offset < chars.Length; offset++)
                            {
                                c = chars[offset];
                                if (c == ',')
                                {
                                    readMode = EJsonReadMode.DicKey;
                                }
                                else if (c == '}')
                                {
                                    return topData;
                                }
                            }
                        }
                        switch (c)
                        {
                            case '"':
                                offset++;
                                value = GetString(chars, ref offset, true);
                                break;
                            case '{':
                            case '[':
                                // 別のブロックを見つけたので処理をネスト
                                value = GetJson(chars, ref offset);
                                break;
                            case ',':
                            case '}':
                                if (cbuf.Count > 0 && value == null)
                                {
                                    value = new string(cbuf.ToArray());
                                }
                                ((Dictionary<string, object>)topData.Obj)[keyStr] = value;
                                value = null;
                                readMode = EJsonReadMode.DicKey;

                                if (c == '}')
                                {
                                    return topData;
                                }
                                break;
                            default:
                                cbuf.Add(c);
                                break;
                        }
                        break;
                    case EJsonReadMode.Array:
                        switch (c)
                        {
                            case '"':
                                offset++;
                                value = GetString(chars, ref offset, true);
                                break;
                            case '{':
                            case '[':
                                // 別のブロックを見つけたので処理をネスト
                                value = GetJson(chars, ref offset);
                                break;
                            case ']':
                            case ',':
                                if (cbuf.Count > 0 && value == null)
                                {
                                    value = new string(cbuf.ToArray());
                                    cbuf.Clear();
                                }
                                ((List<object>)topData.Obj).Add(value);
                                value = null;
                                if (c == ']')
                                {
                                    return topData;
                                }
                                break;
                            default:
                                cbuf.Add(c);
                                break;
                        }
                        break;
                }
            }
            return topData;
        }


        /**
         * ダブルクオートで囲まれた文字列を取得する
         * 
         * @input chars : 取得先のキャラ配列
         * @input offset : chars の参照オフセット
         * @input dqOn : ダブルクオートを残すかどうか(true:残す / false:残さない)
         * @output : 取得した文字列
         */
        private static string GetString(char[] chars, ref int offset, bool dqOn )
        {
            var cbuf = new List<char>();
            bool isEscape = false;

            for (int i = offset; i < chars.Length; i++)
            {
                char c = chars[offset];
                if (isEscape)
                {
                    switch(c)
                    {
                        case '\\':
                            cbuf.Add(c);
                            break;
                        case 'n':
                            cbuf.Add('\n');
                            break;
                        case 't':
                            cbuf.Add('\t');
                            break;
                        case '"':
                            cbuf.Add('"');
                            break;
                    }
                    isEscape = false;
                }
                if (c == '"')
                {
                    if (dqOn) {
                        // ダブルクオートを残す
                        return "\"" + (new string(cbuf.ToArray())) + "\"";
                    }
                    else
                    {
                        return new string(cbuf.ToArray());
                    }
                }
                else if (c == '\\')
                {
                    isEscape = true;
                }
                else
                {
                    cbuf.Add(c);
                }
                offset++;
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ObjToString(sb, this, "");

            return sb.ToString();
        }

        /**
         * １階層分のリストを文字列に変換する
         * 
         * @input sb:    変換した結果を格納する先
         * @input value: 変換元のデータ(string、array、dictionaryのどれか)
         * @input nest:  階層をネストするたびに文字列の先頭に付加するスペース
         */
        private static void ObjToString(StringBuilder sb, MemJsonData memJson, string nest)
        {
            int cnt = 0;

            switch(memJson.dataType)
            {
                case DataType.String:
                    sb.AppendFormat((string)memJson.Obj);
                    break;
                case DataType.Array:
                    foreach (object value in (memJson.Obj as List<object>))
                    {
                        if (value is MemJsonData)
                        {
                            MemJsonData json = (MemJsonData)value;
                            if (json.DataType == DataType.Dictionary)
                            {
                                sb.AppendFormat("{0}{{\r\n", nest);
                                ObjToString(sb, json, nest + "  ");
                                sb.AppendFormat("{0}}}\r\n", nest);
                            }
                            else if (json.DataType == DataType.Array)
                            {
                                sb.AppendFormat("{0}[\r\n", nest);
                                ObjToString(sb, json, nest + "  ");
                                sb.AppendFormat("{0}]\r\n", nest);
                            }
                        }
                        else if ( value is string)
                        {
                            sb.AppendFormat("{0}[{1}]={2}\r\n", nest, cnt, value);
                        }
                        cnt++;
                    }
                    break;
                case DataType.Dictionary:
                    foreach (KeyValuePair<string, object> kvp in (memJson.Obj as Dictionary<string, object>))
                    {
                        if (kvp.Value is MemJsonData)
                        {
                            MemJsonData json = (MemJsonData)kvp.Value;
                            if (json.DataType == DataType.Dictionary)
                            {
                                sb.AppendFormat("{0}{{\r\n", nest);
                                ObjToString(sb, json, nest + "  ");
                                sb.AppendFormat("{0}}}\r\n", nest);
                            }
                            else if (json.DataType == DataType.Array)
                            {
                                sb.AppendFormat("{0}[\r\n", nest);
                                ObjToString(sb, json, nest + "  ");
                                sb.AppendFormat("{0}]\r\n", nest);
                            }
                        }
                        else if (kvp.Value is string)
                        {
                            sb.AppendFormat("{0}[{1}]={2}\r\n", nest, kvp.Key, kvp.Value);
                        }
                    }
                    break;
            }
        }
    }
}
