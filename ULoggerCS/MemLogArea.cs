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
        private string name;            // エリア名
        private UInt32 color;           // エリアの色
        private double timeStart;       // 開始時間(最初のログ時間)
        private double timeEnd;         // 終了時間(最後のログ時間)
        private string imageName;       // 画像名

        private List<MemLogArea> childArea;     // 配下のエリア(areaTypeがDirの場合のみ使用)
        private List<MemLogData> logs;      // 配下のログ(areaTypeがDataの場合のみ使用)
        private MemLogArea parentArea;      // 親のエリア

        public MemLogArea ParentArea
        {
            get { return parentArea; }
            set { parentArea = value; }
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

        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }
        public double TimeStart
        {
            get { return timeStart; }
            set { timeStart = value; }
        }
        public double TimeEnd
        {
            get { return timeEnd; }
            set { timeEnd = value; }
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
        public MemLogArea()
        {
            this.name = "root";
        }
        public MemLogArea(UInt32 id, string name, UInt32 color, MemLogArea parentArea)
        {
            this.name = name;
            this.color = color;
            this.parentArea = parentArea;

            timeStart = 0;
            timeEnd = 0;
            childArea = null;
            logs = null;

        }

        //
        // Methods
        //
        /**
         * 配下にエリアを追加
         */
        public void AddChildArea(MemLogArea logArea)
        {
            if (childArea == null)
            {
                childArea = new List<MemLogArea>();
            }
            logArea.parentArea = this;
            childArea.Add(logArea);
        }

        /**
         * ログデータを追加
         */
        public void AddLogData(MemLogData logData)
        {
            if (logs == null)
            {
                logs = new List<MemLogData>();
            }
            logs.Add(logData);
        }

        /**
         * コンソールログに出力する
         * 子エリアも同時に出力するため、再帰呼び出しを行う。
         */
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("area name:{0} ", name);
            sb.AppendFormat(" color:{0:X8}", color);
            sb.AppendFormat(" timeStart:{0}", timeStart);
            if (timeEnd != 0)
            {
                sb.Append(String.Format(" timeEnd:{0}", timeEnd));
            }
            if (imageName != null)
            {
                sb.Append(String.Format(" imageName:{0}", imageName));
            }

            if (parentArea != null)
            {
                //sb.Append(String.Format(" paretArea:{0}", parentArea));
            }

            // ログデータ
            if (logs != null)
            {
                foreach (var log in logs)
                {
                    sb.AppendLine(log.ToString());
                }
            }

            // 子エリア
            if (childArea != null)
            {
                foreach( MemLogArea area in childArea)
                {
                    sb.Append(area.ToString());
                }
            }

            return sb.ToString();
        }

        public void Print()
        {
            Console.WriteLine(this);
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
        private MemLogArea rootArea;
        private MemLogArea lastAddArea;

        public MemLogArea Rootarea
        {
            get { return rootArea; }
            set { rootArea = value; }
        }

        /**
         * Constructor
         */
        public MemLogAreaManager()
        {
            rootArea = new MemLogArea();
            lastAddArea = rootArea;
        }

        //
        // Methods
        //
        /**
         * 追加先を指定してエリアを追加
         * 
         * @input logArea: 追加するログエリア
         * @input parentName: 追加先の親エリア名
         */
        public void AddArea(MemLogArea logArea, MemLogArea parentArea)
        {
            if (parentArea == null)
            {
                // 最後に追加したエリアと同じ階層（同じ親の下）に追加する
                if (lastAddArea != null)
                {
                    if (lastAddArea.ParentArea != null)
                    {
                        lastAddArea.ParentArea.AddChildArea(logArea);
                    }
                    else
                    {
                        rootArea.AddChildArea(logArea);
                    }
                }
                else
                {
                    rootArea.AddChildArea(logArea);
                }
            }
            else
            {
                // 指定した親の下に追加
                parentArea.AddChildArea(logArea);
            }

            lastAddArea = logArea;
        }

        /**
         * ログを追加
         */
        public void AddLogData(MemLogData logData)
        {
            lastAddArea.AddLogData(logData);
        }

        /**
         * 指定の名前のエリアを探す
         * ※エリアを追加できるポイントは、自分の親（親の親も含む）に限られるのでその範囲で探す
         * @input name: 探すエリア名
         */
        public MemLogArea searchArea(string name)
        {
            // １つもエリアを追加していない場合はルート
            if (lastAddArea == null)
            {
                return rootArea;
            }

            MemLogArea area = lastAddArea;

            while(area != rootArea)
            {
                if (area.ParentArea.Name.Equals(name))
                {
                    return area.ParentArea;
                }
                area = area.ParentArea;
            }

            // 見つからなかった場合はルート
            return rootArea;
        }

        public void Print()
        {
            rootArea.Print();
        }
    }
}
