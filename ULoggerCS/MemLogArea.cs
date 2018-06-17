using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * ULogViewでメモリ情報に展開されたログデータ
 */
namespace ULoggerCS
{
    class MemLogArea
    {
        //
        // Properties
        //
        private UInt32 id;              // エリアID
        private string name;            // エリア名
        private UInt32 color;           // エリアの色
        private double timeStart;       // 開始時間(最初のログ時間)
        private double timeEnd;         // 終了時間(最後のログ時間)
        private List<MemLogArea> childArea;     // 配下のエリア(areaTypeがDirの場合のみ使用)
        private List<MemLogData> logs;      // 配下のログ(areaTypeがDataの場合のみ使用)
        private MemLogArea parentArea;      // 親のエリア

        public MemLogArea ParentArea
        {
            get { return parentArea; }
            set { parentArea = value; }
        }


        public UInt32 ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public UInt32 Color
        {
            get { return color; }
            set { color = value; }
        }
        public double TimeEnd
        {
            get { return timeEnd; }
            set { timeEnd = value; }
        }
        public double TimeStart
        {
            get { return timeStart; }
            set { timeStart = value; }
        }
        public List<MemLogArea> ChildArea
        {
            get { return childArea; }
            set { childArea = value; }
        }
        public List<MemLogData> Logs
        {
            get { return logs; }
            set { logs = value; }
        }

        //
        // Constructor
        //
        public MemLogArea(UInt32 id, string name, UInt32 color, MemLogArea parentArea)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.parentArea = parentArea;

            timeStart = 0;
            timeEnd = 0;
            childArea = null;
            logs = new List<MemLogData>();
        }

        //
        // Methods
        //
        public void AddChildArea()
        {

        }

    }  // class MemLogArea

    //
    // メモリ情報エリアを管理するクラス
    // このクラスを使用してエリアを追加する
    // 
    class MemLogAreaManager
    {
        //
        // Properties 
        //
    }
}
