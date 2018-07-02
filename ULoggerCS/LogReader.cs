﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using ULoggerCS.Utility;

/**
 * ULogView用のログファイルを読み込んでメモリに展開するクラス
 */
namespace ULoggerCS
{
    

    class LogReader
    {
        //
        // Consts
        //
        private const int ID_NAME_MAX = 64;         // ID名のByte数
        private const int LANE_NAME_MAX = 64;       // レーン名のByte数
        private const int IMAGE_NAME_MAX = 64;      // 画像名のByte数

        public Lanes lanes = null;
        public MemLogIDs logIDs = null;
        public MemIconImages images = null;
        public MemLogAreaManager areaManager = new MemLogAreaManager();
        public Encoding encoding;

        public MemLogAreaManager AreaManager
        {
            get { return areaManager; }
            set { areaManager = value; }
        }


        //public string        

        public LogReader()
        {
            encoding = Encoding.UTF8;
        }

        /**
         * ULoggerで作成したログファイルを読み込んでULogViewで使用できる種類のデータとしてメモリ展開する
         * 
         * @input inputFilePath: ログファイルのパス
         * @input fileType: ログファイルの種類(テキスト、バイナリ)
         * @output : true:成功 / false:失敗
         */
        public bool ReadLogFile(string inputFilePath, LogFileType fileType)
        {
            if (fileType == LogFileType.Text)
            {
                ReadLogFileText(inputFilePath);
            }
            else
            {
                ReadLogFileBin(inputFilePath);
            }
            areaManager.Print();
            
            return true;
        }

        #region Text

        /**
         * テキスト形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileText(string inputFilePath)
        {
            bool isHeader = false;
            
            // まずはヘッダ部分からエンコードタイプを取得する
            encoding = GetEncodingText(inputFilePath);

            using (StreamReader sr = new StreamReader(inputFilePath, encoding))
            {
                // ヘッダ部分を読み込む <head>～</head>
                while (!sr.EndOfStream)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = sr.ReadLine().Trim();
                    
                    // <head>の行が見つかるまではスキップ
                    if (isHeader == false)
                    {
                        if (line.Equals("<head>"))
                        {
                            isHeader = true;
                        }
                    }
                    else
                    {
                        if (line.Equals("</head>"))
                        {
                            break;
                        }
                        Dictionary<string, string> fields = SplitLineStr(line, ',', ':');

                        // ヘッダーの読み込みメイン
                        switch (line)
                        {
                            case "<lane>":
                                lanes = GetLanesText(sr);
                                break;
                            case "<logid>":
                                logIDs = GetLogIDsText(sr);
                                break;
                            case "<image>":
                                images = GetIconImagesText(sr);
                                break;
                        }
                    }
                }

                // 本体部分を読み込む
                bool isBody = false;
                while (!sr.EndOfStream)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = sr.ReadLine().Trim();

                    // <body>を見つけたら本体の取得モードに入る
                    if (!isBody)
                    {
                        if (line.Equals("<body>"))
                        {
                            isBody = true;
                        }
                    }
                    else
                    {
                        areaManager = GetLogDataText(sr);
                    }
                }
            }
            return true;
        }

        /**
         * LogID情報を取得する
         */
        private MemLogIDs GetLogIDsText(StreamReader sr)
        {
            MemLogIDs _logIDs = new MemLogIDs();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                Dictionary<string, string> fields = SplitLineStr(line, ',', ':');
                
                if (fields.Count == 0)
                {
                    continue;
                }

                // 終了判定
                if (line.Equals("</logid>"))
                {
                    break;
                }

                // レーン情報を取得
                MemLogID logID = new MemLogID();

                foreach (KeyValuePair<string, string> kvp in fields)
                {
                    // keyとvalueに分割
                    if (kvp.Value != null)
                    {
                        switch (kvp.Key)
                        {
                            case "id":
                                UInt32 id;
                                if (UInt32.TryParse(kvp.Value, out id))
                                {
                                    logID.ID = id;
                                }
                                break;
                            case "name":
                                logID.Name = kvp.Value;
                                break;
                            case "color":
                                // FF001122 のような16進数文字列を整数値に変換
                                logID.Color = Convert.ToUInt32(kvp.Value, 16);
                                break;
                            case "image":
                                logID.Image = images.GetImage(kvp.Value);
                                break;
                        }
                    }
                }
                _logIDs.Add(logID);
            }
            return _logIDs;
        }

        /**
         * Lane情報を取得する
         * <lane>～</lane> の中を行をすべて取得する
         */
        private Lanes GetLanesText(StreamReader sr)
        {
            Lanes _lanes = new Lanes();

            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                Dictionary<string, string> fields = SplitLineStr(line);
                
                if (fields.Count == 0)
                {
                    continue;
                }
                // 終了判定
                if (line.Equals("</lane>"))
                {
                    break;
                }

                // レーン情報を取得
                Lane lane = new Lane();

                foreach (KeyValuePair<string, string> kvp in fields)
                {
                    // keyとvalueに分割
                    if (kvp.Value != null)
                    {
                        switch (kvp.Key)
                        {
                            case "id":
                                UInt32 id;
                                if (UInt32.TryParse(kvp.Value, out id))
                                {
                                    lane.ID = id;
                                }
                                break;
                            case "name":
                                lane.Name = kvp.Value;
                                break;
                            case "color":
                                try {
                                    // FF001122 のような16進数文字列を整数値に変換
                                    lane.Color = Convert.ToUInt32(kvp.Value, 16);
                                }
                                catch (Exception e)
                                {
                                    lane.Color = 0;
                                }
                                break;
                        }
                    }
                }
                _lanes.Add(lane);
            }
            return _lanes;
        }

        /**
         * １行分のデータを フィールド名:値 の辞書型に変換する
         * @input line: 1行分のテキスト
         * @input fieldSplit : フィール間の区切り文字
         * @input tokenSplit : キー＆値の区切り文字
         */
        private static Dictionary<string, string> SplitLineStr(string line, char fieldSplit = ',', char tokenSplit = ':')
        {
            var fields = new Dictionary<string, string>();

            // key:value, key:"value" を辞書型に変換する
            // "" に囲まれた文字列中にカンマが入っていても大丈夫なようにする
            char[] chars = line.ToCharArray();

            bool isKey = true;
            bool isDQ = false;      // ダブルクオート(")で囲まれた文字を処理中にtrue
            bool isEscape = false;  // ダブルクオート(")内で \" を "と認識するためのフラグ
            List<char> cbuf = new List<char>(100);
            string key = null, value = null;

            foreach (char c in chars)
            {
                if (isDQ)
                {
                    // "～" で囲まれた部分は fieldSplit や tokenSplit の文字列を無視する
                    if (c == '\\')
                    {
                        isEscape = true;
                    }
                    else if (c == '"' && isEscape != true)
                    {
                        isDQ = false;
                    }
                    else
                    {
                        cbuf.Add(c);
                    }
                    if (c != '\\')
                    {
                        isEscape = false;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        isDQ = true;
                    }
                    else if (c == fieldSplit)
                    {
                        // フィールドの区切り文字を見つけたら辞書型 キー = 値 のセットをに追加する
                        if (isKey)
                        {
                            key = new String(cbuf.ToArray());
                        }
                        else
                        {
                            value = new String(cbuf.ToArray());
                        }
                        if (key != null && key.Length > 0)
                        {
                            fields[key] = value;
                        }
                        cbuf.Clear();
                        key = null;
                        value = null;
                        isKey = true;
                    }
                    else if (c == tokenSplit)
                    {
                        // トークンの区切り文字を見つけたら キー読み取りモードから値読み取りのモードに切り替える
                        key = new String(cbuf.ToArray());
                        cbuf.Clear();
                        isKey = false;
                    }
                    else
                    {
                        cbuf.Add(c);
                    }
                }
            }
            // フィールドの区切り文字を見つけたら辞書型 キー = 値 のセットをに追加する
            if (isKey)
            {
                key = new String(cbuf.ToArray());
            }
            else
            {
                value = new String(cbuf.ToArray());
            }
            if (key != null && key.Length > 0)
            {
                fields[key] = value;
            }

            return fields;
        }

        /**
         * １行分のIconImage情報を取得する
         */
        private MemIconImages GetIconImagesText(StreamReader sr)
        {
            MemIconImages _images = new MemIconImages();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                Dictionary<string, string> fields = SplitLineStr(line);

                if (fields.Count == 0)
                {
                    continue;
                }
                // 終了判定
                if (line.Equals("</image>"))
                {
                    break;
                }

                // 画像情報を取得
                MemIconImage image = new MemIconImage();

                foreach (KeyValuePair<string, string> kvp in fields)
                {
                    // keyとvalueに分割
                    if (kvp.Value != null)
                    {
                        switch (kvp.Key.ToLower())
                        {
                            case "name":
                                image.Name = kvp.Value;
                                break;
                            case "image":
                                try
                                {
                                    // Base64文字列を出コード
                                    byte[] byteImage = Convert.FromBase64String(kvp.Value);
                                    image.SetByteImage(byteImage);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("base64のデコードに失敗しました。");
                                }
                                break;
                        }
                    }
                }
                _images.Add(image);
            }
            return _images;
        }

        /**
         * ログの本体部分を取得
         * <body>～</body> の中の行をすべて取得する
         * 
         */
        private MemLogAreaManager GetLogDataText(StreamReader sr)
        {
            MemLogAreaManager manager = new MemLogAreaManager();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                Dictionary<string, string> fields = SplitLineStr(line);

                if (fields.Count == 0)
                {
                    continue;
                }
                
                // 終了判定
                if (line.Equals("</body>"))
                {
                    return manager;
                }
                else if (fields.ContainsKey("area"))
                {
                    MemLogArea area = GetMemAreaText(fields, manager);
                    manager.AddArea(area);
                }
                else if (fields.ContainsKey("log"))
                {
                    MemLogData log = GetMemLogText(fields);
                    manager.AddLogData(log);
                }
            }

            return null;
        }

        /**
         * 1行分のフィールドからエリアデータを取得する
         * @input fields:
         * @output  取得したエリアデータ
         */
        private MemLogArea GetMemAreaText(Dictionary<string,string> fields, MemLogAreaManager manager)
        {
            MemLogArea area = new MemLogArea();

            // area,name:"area1",parent:"root",color=FF000000, image="icon1"

            foreach (KeyValuePair<string, string> kvp in fields)
            {
                if (kvp.Value != null)
                {
                    switch (kvp.Key.ToLower())
                    {
                        case "name":
                            area.Name = kvp.Value;
                            break;
                        case "parent":
                            area.ParentArea = manager.searchArea(kvp.Value);
                            break;
                        case "color":
                            area.Color = Convert.ToUInt32(kvp.Value, 16);
                            break;
                        case "image":
                            area.Image = images.GetImage(kvp.Value);
                            break;
                    }
                }
            }
            return area;
        }

        /**
         * 1行分のフィールドからログデータを取得する
         * @input fields:
         * @output  取得したログデータ
         *  Sample
         *  log,type: Single,id: 1,lane: 1,time: 0.0026799596,text: "test1"
         */
        private static MemLogData GetMemLogText(Dictionary<string,string> fields)
        {
            MemLogData log = new MemLogData();

            foreach (KeyValuePair<string,string> kvp in fields)
            {
                if (kvp.Value != null)
                {
                    switch (kvp.Key.ToLower())
                    {
                        case "type":
                            switch (kvp.Value.ToLower()) {
                                case "single":               // 単体ログ
                                    log.Type = MemLogType.Point;
                                    break;
                                case "areastart":            // 範囲開始
                                    log.Type = MemLogType.Range;
                                    break;
                                case "areaend":              // 範囲終了
                                    log.Type = MemLogType.Range;
                                    break;
                                case "value":                // 値
                                    log.Type = MemLogType.Value;
                                    break;
                            }
                            break;
                        case "id":
                            log.ID = UInt32.Parse(kvp.Value);
                            break;
                        case "lane":
                            log.LaneId = Byte.Parse(kvp.Value);
                            break;
                        case "time":
                            log.Time1 = Double.Parse(kvp.Value);
                            break;
                        case "text":
                            log.Text = kvp.Value;
                            break;
                        case "color":
                            log.Color = Convert.ToUInt32(kvp.Value, 16);
                            break;
                        case "detail_type":
                            switch(kvp.Value.ToLower())
                            {
                                case "text":
                                    log.DetailType = DetailDataType.Text;
                                    break;
                                case "array":
                                    log.DetailType = DetailDataType.Array;
                                    break;
                                case "dictionary":
                                    log.DetailType = DetailDataType.Dictionary;
                                    break;
                                case "json":
                                    log.DetailType = DetailDataType.JSON;
                                    break;
                            }
                            break;
                        case "detail":
                            if (log.DetailType == DetailDataType.Array)
                            {
                                Console.WriteLine("");
                            }
                            log.Detail = kvp.Value;
                            break;
                    }
                }
            }
            return log;
        }

        /**
         * ヘッダ部分からファイルのエンコードタイプを取得する
         * 
         * @input inputFilePath:  LogViewのファイルパス
         * @output エンコードの種類
         */
        public static Encoding GetEncodingText(string inputFilePath)
        {
            bool isHeader = false;
            int count = 0;
            Encoding encoding = Encoding.UTF8;

            using (StreamReader sr = new StreamReader(inputFilePath))
            {
                // ヘッダ部分を読み込む <head>～</head>
                while (sr.Peek() >= 0)
                {
                    // ファイルを 1 行ずつ読み込む
                    string line = sr.ReadLine().Trim();

                    // <head>の行が見つかるまではスキップ
                    if (isHeader == false)
                    {
                        if (line.Equals("<head>"))
                        {
                            isHeader = true;
                        }
                    }
                    else
                    {
                        if (line.Equals("</head>"))
                        {
                            break;
                        }
                        // エンコード種別
                        if (line.Contains("encoding:"))
                        {
                            string[] splitted = line.Split(':');
                            if (splitted.Length >= 2)
                            {
                                encoding = UUtility.GetEncodingFromStr(splitted[1].ToLower());
                            }
                        }
                    }
                    count++;
                    if (count > 100)
                    {
                        // ファイルの先頭部分に見つからなかったので打ち切り
                        break;
                    }
                }
                return encoding;
            }
        }  // GetEncoding()

        #endregion

        #region Binary
        /**
         * バイナリ形式のログファイルを読み込んでメモリに展開する
         * @input inputFilePath: ログファイルのパス
         * @output : true:成功 / false:失敗
         */
        private bool ReadLogFileBin(string inputFilePath)
        {
            using (var fs = new UFileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    // Header
                    ReadLogHeadBin(fs);

                    // Body
                    ReadLogBodyBin(fs);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error ReadLogFileBin " + e.Message);
                    throw;
                }
            }
            return true;
        }

        /**
         * バイナリログのヘッダ部分を取得
         * @input fs : ファイルオブジェクト
         * 
         */ 
        private void ReadLogHeadBin(UFileStream fs)
        {
            // 文字コードを取得
            string encStr = fs.GetSizeString();
            this.encoding = UUtility.GetEncodingFromStr(encStr);
            fs.EncodingType = encoding;
            
            // ログID情報
            logIDs = ReadLogIDsBin(fs);

            // レーン情報
            lanes = ReadLogLanesBin(fs);

            // アイコン画像
            images = ReadLogImagesBin(fs);

            // ログIDの画像を設定する
            foreach (MemLogID logId in logIDs)
            {
                logId.Image = images.GetImage(logId.ImageName);
            }
        }

        /**
         * バイナリログのログID情報を読み込む
         * 
         */
        private MemLogIDs ReadLogIDsBin(UFileStream fs)
        {
            MemLogIDs _logIds = new MemLogIDs();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // 1件分のログを取得
                MemLogID logId = new MemLogID();

                // ID
                logId.ID = fs.GetUInt32();

                // 名前
                logId.Name = fs.GetSizeString();

                // 色
                logId.Color = fs.GetUInt32();

                // アイコン画像名
                // 画像はアイコン画像を読み込んでから設定する
                logId.ImageName = fs.GetSizeString();

                _logIds.Add(logId);
            }

            return _logIds;
        }

        /**
         * バイナリログのレーン情報を読み込む
         * @input fs: 読み込み元のファイル
         */
        private Lanes ReadLogLanesBin(UFileStream fs)
        {
            Lanes _lanes = new Lanes();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // 1件分のログを取得
                Lane lane = new Lane();

                // ID
                lane.ID = fs.GetUInt32();

                // 名前
                lane.Name = fs.GetSizeString();

                // 色
                lane.Color = fs.GetUInt32();

                _lanes.Add(lane);
            }

            return _lanes;
        }

        /**
         * バイナリログの画像ID情報を読み込む
         */
        private MemIconImages ReadLogImagesBin(UFileStream fs)
        {
            MemIconImages _images = new MemIconImages();

            // 件数取得
            int size = fs.GetInt32();

            for (int i = 0; i < size; i++)
            {
                // １件分のログを取得
                MemIconImage image = new MemIconImage();

                // 名前
                image.Name = fs.GetSizeString();

                // 画像
                int imageSize = fs.GetInt32();
                if (imageSize > 0)
                {
                    byte[] byteImage = fs.GetBytes(imageSize);
                    image.SetByteImage(byteImage);
                }

                _images.Add(image);
            }

            return _images;
        }

        /**
         * バイナリログの本体部分を取得
         * @input fs : ファイルオブジェクト
         */
        private void ReadLogBodyBin(UFileStream fs)
        {
            while (!fs.EndOfStream())
            {
                // 件数取得
                int logNum = fs.GetInt32();

                for (int i = 0; i < logNum; i++)
                {
                    // 種類を取得
                    LogType type = (LogType)fs.GetByte();

                    if (type == LogType.Data)
                    {
                        ReadLogDataBin(fs);
                    }
                    else
                    {
                        ReadLogAreaBin(fs);
                    }
                }
            }
        }

        /**
         * バイナリログデータを読み込む
         */
        private void ReadLogDataBin(UFileStream fs)
        {
            MemLogData log = new MemLogData();

            // ログID
            log.ID = fs.GetUInt32();

            //ログタイプ
            bool isRangeEnd = false;
            LogDataType dataType = (LogDataType)fs.GetByte();
            
            switch (dataType) {
                case LogDataType.Single:
                    log.Type = MemLogType.Point;
                    break;
                case LogDataType.RangeStart:
                    log.Type = MemLogType.Range;
                    break;
                case LogDataType.RangeEnd:
                    // 同じレーンの Range タイプのログに結合する
                    // todo
                    isRangeEnd = true;
                    break;
                case LogDataType.Value:
                    log.Type = MemLogType.Value;
                    break;
            }

            //表示レーンID
            log.LaneId = fs.GetUInt32();

            //タイトルの長さ
            //タイトル
            log.Text = fs.GetSizeString();

            // 範囲ログの終了タイプの場合、結合する

            //時間
            Double time = fs.GetDouble();
            if (log.Type == MemLogType.Range && isRangeEnd == true)
            {
                // 1つ前の Rangeタイプの Time2 に時間を設定
                // todo
                return;
            }
            else
            {
                log.Time1 = time;
            }

            //ログデータ（詳細）の種類
            log.DetailType = (DetailDataType)fs.GetByte();

            if (log.DetailType == DetailDataType.Array)
            {
                Console.WriteLine("hoge");
            }
            //ログデータ(詳細)のサイズ
            //ログデータ(詳細)
            if (log.DetailType != DetailDataType.None)
            {
                log.Detail = fs.GetSizeString();
            }

            // ログを追加する
            areaManager.AddLogData(log);
        }

        /**
         * バイナリエリアデータを読み込む
         */
        private void ReadLogAreaBin(UFileStream fs)
        {
            MemLogArea area = new MemLogArea();

            // エリア名の長さ
            // エリア名
            area.Name = fs.GetSizeString();

            // 親のエリア名の長さ
            // 親のエリア名
            area.ParentArea = areaManager.searchArea(fs.GetSizeString());

            // 色
            area.Color = fs.GetUInt32();

            // エリアを追加
            areaManager.AddArea(area);
        }

        #endregion

        #region Common
        
        #endregion

        #region Debug

        /**
         * LogReaderに読み込んだ情報をファイルに保存する
         * 
         * @input filePath : 書き込み先のファイルパス
         */
        public void WriteToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                if (logIDs != null)
                {
                    sw.Write(logIDs.ToString());
                }
                if (lanes != null)
                {
                    sw.Write(lanes.ToString());
                }
                if (images != null)
                {
                    sw.Write(images.ToString());
                }
                
                if (areaManager != null)
                {
                    areaManager.WriteToFile(sw);
                }
            }
        }
        #endregion
    }
}
