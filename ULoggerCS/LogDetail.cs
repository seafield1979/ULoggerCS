using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
    //
    // データタイプの種類
    //
    enum DetailDataType : byte
    {
        None = 0,       // 詳細情報なし
        Text,           // テキスト  "hello"
        Array,          // 配列 {0,1,2,3...}
        Dictionary,     // 辞書 {"key1"="value1", "key2="value2", ...}
        JSON            // JSON {"key1"="value1", "dic1"={"key2"="value2",...}, "array1"=[0,1,2,3]}
    }

    //
    // LogDetailはログの詳細を出力するクラスの親クラス。
    // ユーザーは自分のプログラムでログの詳細を出力したいクラスの親クラスにこれを指定します。
    //
    abstract class LogDetail
    {
        // Constructor
        public LogDetail() {
        }

        // Methods

        // オブジェクトのコピーを作成する
        // 派生クラスで実装する。
        public abstract LogDetail CreateCopy();

        //public abstract string ToString();
        public abstract string dataTypeString();
        public abstract byte dataTypeByte();
        public abstract byte[] ToBinary();
    }
}
