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
    class JsonData2
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

        //
        // Methods
        //
        public void Add(object obj)
        {
            this.obj = obj;
        }

        /**
         * 文字列に変換
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            ObjToString(sb, this.obj);

            return sb.ToString();
        }

        /**
         * １階層分のリストを文字列に変換する
         * 
         * @input sb:    変換した結果を格納する先
         * @input value: 変換元のデータ(string、array、dictionaryのどれか)
         * @input nest:  階層をネストするたびに文字列の先頭に付加するスペース
         */
        private static void ObjToString(StringBuilder sb, object node)
        {
            int cnt = 0;

            if (node is string)
            {
                sb.AppendFormat("\"{0}\"", (string)node);
            }
            else if (node is object[])
            {
                sb.Append("[");
                foreach (object value in (node as object[]))
                {
                    if (cnt > 0)
                    {
                        sb.Append(",");
                    }
                    if (value is string)
                    {
                        sb.Append(value);
                    }
                    else if(value is List<object> || value is object[] || value is Dictionary<string, object>)
                    {
                        ObjToString(sb, value);
                    }
                    else
                    {
                        sb.Append(value);
                    }
                    cnt++;
                }
                sb.Append("]");
            }
            else if (node is List<object>)
            {
                sb.Append("[");
                foreach (object value in (node as List<object>))
                {
                    if (cnt > 0)
                    {
                        sb.Append(",");
                    }
                    if (value is string)
                    {
                        sb.Append(value);
                    }
                    else if (value is List<object> || value is object[] || value is Dictionary<string, object>)
                    {
                        ObjToString(sb, value);
                    }
                    else
                    {
                        sb.Append(value);
                    }
                    cnt++;
                }
                sb.Append("]");
            }
            else if (node is Dictionary<string, object>)
            {
                sb.Append("{");
                foreach (KeyValuePair<string, object> kvp in (node as Dictionary<string, object>))
                {
                    if (cnt > 0)
                    {
                        sb.Append(",");
                    }

                    // "キー名":
                    sb.AppendFormat("\"{0}\":", kvp.Key);


                    if (kvp.Value is List<object> || kvp.Value is Dictionary<string, object>)
                    {
                        ObjToString(sb, kvp.Value);
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
            }
        }
    }
}
