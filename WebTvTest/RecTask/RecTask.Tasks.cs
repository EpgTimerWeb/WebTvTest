using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebTvTest.RecTask
{
    public class RecTaskInfo
    {
        private HeaderClass _Header = null;
        public HeaderClass Header { get { return _Header; } }
        private InfoClass _Info = null;
        public InfoClass Info { get { return _Info; } }
        private StreamStatisticsClass _Statistics = null;
        public StreamStatisticsClass Statistics { get { return _Statistics; } }
        private ulong _StatisticsUpdateCount = 0;
        public ulong StatisticsUpdateCount { get { return _StatisticsUpdateCount; } }
        private ulong _TotTime = 0;
        public ulong TotTime { get { return _TotTime; } }
        public RecTaskInfo(RecTaskSharedMemoryStruct taskInfo)
        {
            _Header = new HeaderClass(taskInfo.Header);
            _Info = new InfoClass(taskInfo.Info);
            _Statistics = new StreamStatisticsClass(taskInfo.Statistics);
            _StatisticsUpdateCount = taskInfo.StatisticsUpdateCount;
            _TotTime = taskInfo.TotTime;
        }
    }
    #region Class
    public class HeaderClass
    {
        private uint _Size = 0;
        public uint Size { get { return _Size; } }
        private uint _Version = 0;
        public uint Version { get { return _Version; } }
        public HeaderClass(HeaderStruct header)
        {
            _Size = header.Size;
            _Version = header.Version;
        }
    }
    public class InfoClass
    {
        private uint _TaskID = 0;
        public uint TaskID { get { return _TaskID; } }
        private TaskType _Type = TaskType.TASK_TYPE_SERVER;
        public TaskType Type { get { return _Type; } }
        private uint _ProcessID = 0;
        public uint ProcessID { get { return _ProcessID; } }
        private uint _Version = 0;
        public uint Version { get { return _Version; } }
        private TaskState _State = TaskState.TASK_STATE_STARTING;
        public TaskState State { get { return _State; } }
        public InfoClass(InfoStruct info)
        {
            _TaskID = info.TaskID;
            _Type = info.Type;
            _ProcessID = info.ProcessID;
            _Version = info.Version;
            _State = info.State;
        }
    }
    public class StreamStatisticsClass
    {
        private float _SignalLevel = 0.0F;
        public float SignalLevel { get { return _SignalLevel; } }
        private uint _Bitrate = 0;
        public uint Bitrate { get { return _Bitrate; } }
        private ulong _InputPacketCount = 0;
        public ulong InputPacketCount { get { return _InputPacketCount; } }
        private ulong _ErrorPacketCount = 0;
        public ulong ErrorPacketCount { get { return _ErrorPacketCount; } }
        private ulong _DiscotinuityCount = 0;
        public ulong DiscotinuityCount { get { return _DiscotinuityCount; } }
        private ulong _ScramblePacketCount = 0;
        public ulong ScramblePacketCount { get { return _ScramblePacketCount; } }
        public StreamStatisticsClass(StreamStatisticsStruct streamStatistics)
        {
            _SignalLevel = streamStatistics.SignalLevel;
            _Bitrate = streamStatistics.Bitrate;
            _InputPacketCount = streamStatistics.InputPacketCount;
            _ErrorPacketCount = streamStatistics.ErrorPackerCount;
            _DiscotinuityCount = streamStatistics.DiscotinuityCount;
            _ScramblePacketCount = streamStatistics.ScramblePacketCount;
        }
    }
    #endregion
    #region Struct
    public struct RecTaskSharedMemoryStruct
    {
        public HeaderStruct Header;
        public InfoStruct Info;
        public ulong StatisticsUpdateCount;
        public StreamStatisticsStruct Statistics;
        public ulong TotTime;
    }
    public struct HeaderStruct
    {
        public uint Size;
        public uint Version;
    }
    public struct InfoStruct
    {
        public uint TaskID;
        public TaskType Type;
        public uint ProcessID;
        public uint Version;
        public TaskState State;
    }
    public struct StreamStatisticsStruct
    {
        public float SignalLevel;
        public uint Bitrate;
        public ulong InputPacketCount;
        public ulong ErrorPackerCount;
        public ulong DiscotinuityCount;
        public ulong ScramblePacketCount;
    }
    #endregion
    public enum TaskType
    {
        TASK_TYPE_SERVER = 0,
        TASK_TYPE_CLIENT
    }
    public enum TaskState
    {
        TASK_STATE_STARTING = 0, TASK_STATE_RUNNING, TASK_STATE_ENDING
    }
    

}
