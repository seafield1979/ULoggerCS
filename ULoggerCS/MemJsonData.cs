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

    class MemJsonData
    {
        // 
        // Properties
        //
        Dictionary<string, Object> dicData;

        //
        // Constructor
        //
        public MemJsonData()
        {
            dicData = new Dictionary<string, object>();
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
        public void Deserialize(string jsonStr)
        {
            char[] chars = jsonStr.ToCharArray();
            int offset = 0;

            dicData = GetJson(chars, ref offset);
        }

        /**
         * １階層分のJSONデシリアライズを行う
         * 
         * @input chars : シリアライズ対象のJSON形式のキャラ配列
         * @input offset : chars の参照位置
         */
        private Dictionary<string, object> GetJson(char[] chars,ref int offset)
        {
            string keyStr = null;
            string valueStr = null;
            EJsonReadMode readMode = EJsonReadMode.None;
            var cbuf = new List<char>();
            var dic1 = new Dictionary<string, object>();
            int arrayCnt = 0;
            object value = null;

            for (; offset < chars.Length; offset++) 
            {
                char c = chars[offset];

                if (c == ' ')
                {
                    // スペースはスキップ
                    continue;
                }

                switch (readMode)
                {
                    case EJsonReadMode.None:
                        if (c == '[')
                        {
                            readMode = EJsonReadMode.Array;
                        }
                        else if (c == '{')
                        {
                            readMode = EJsonReadMode.DicKey;
                        }
                        break;
                    case EJsonReadMode.DicKey:
                        switch(c)
                        {
                            case '"':
                                offset++;
                                keyStr = GetString(chars, ref offset);
                                break;
                            case ':':
                                readMode = EJsonReadMode.DicValue;
                                break;
                            case ',':
                                // 値を取得前なので、キーのみ
                                if (keyStr != null)
                                {
                                    dic1[keyStr] = "";
                                }
                                break;
                            case ']':
                                return dic1;
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
                                    return dic1;
                                }
                            }
                        }
                        switch (c)
                        {
                            case '"':
                                offset++;
                                valueStr = GetString(chars, ref offset);
                                break;
                            case '{':
                            case '[':
                                // 別のブロックを見つけたので処理をネスト
                                value = GetJson(chars, ref offset);
                                break;
                            case ',':
                            case '}':
                                if (cbuf.Count > 0 && valueStr == null)
                                {
                                    value = new string(cbuf.ToArray());
                                }
                                dic1[keyStr] = value;
                                readMode = EJsonReadMode.DicKey;

                                if (c == '}')
                                {
                                    return dic1;
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
                                valueStr = GetString(chars, ref offset);
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
                                dic1[arrayCnt.ToString()] = value;
                                value = null;
                                arrayCnt++;
                                if (c == ']')
                                {
                                    offset++;
                                    return dic1;
                                }
                                break;
                            default:
                                cbuf.Add(c);
                                break;
                        }
                        break;
                }
            }
            return dic1;
        }


        /**
         * ダブルクオートで囲まれた文字列を取得する
         * 
         * @input chars : 取得先のキャラ配列
         * @input offset : chars の参照オフセット
         * @output : 取得した文字列
         */
        private static string GetString(char[] chars, ref int offset )
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
                    }
                }
                if (c == '"')
                {
                    return new string(cbuf.ToArray());
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

    }
}
