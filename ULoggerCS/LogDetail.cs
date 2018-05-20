using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULoggerCS
{
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

        public abstract string toString();
        public abstract string dataTypeString();
        public abstract byte[] toBinary();
    }
}
