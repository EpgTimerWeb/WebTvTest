using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebTvTest.RecTask
{
    public class RecTask
    {
        private const string RECTASK_SERVER_PIPE_NAME = "RecTask_Server_Pipe_";
        private const string RECTASK_SERVER_SHARED_MEMORY_NAME = "RecTask_Server_SharedMemory_";

        public static string[] GetPipes()
        {
            List<string> Names = new List<string>();
            WIN32_FIND_DATA Data;
            IntPtr Handle = FindFirstFile(@"\\.\\pipe\*", out Data);
            if (Handle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    Names.Add(Data.cFileName);
                } while (FindNextFile(Handle, out Data));
                FindClose(Handle);
            }
            return Names.ToArray();
        }

        private int taskID = 0;

        /// <summary>
        /// RecTaskを開きます
        /// </summary>
        /// <param name="taskID">対象のTaskID</param>
        public RecTask(int taskID)
        {
            using (var stream = new NamedPipeClientStream(".", RECTASK_SERVER_PIPE_NAME + taskID))
            {
                stream.Connect(100);
                if (!stream.IsConnected)
                {
                    throw new IOException("RecTaskに接続できません");
                }
            }
            this.taskID = taskID;
        }

        public Dictionary<string, object> SendCommand(string name, Dictionary<string, object> parameters = null)
        {
            using (var stream = new NamedPipeClientStream(".", RECTASK_SERVER_PIPE_NAME + taskID))
            {
                stream.Connect(100);
                if (!stream.IsConnected)
                {
                    throw new IOException("RecTaskに接続できません");
                }
                var input = Message.Serialize(name, parameters);
                var inputBytes = Encoding.Unicode.GetBytes(input);
                var head = new byte[4];

                BitConverter.GetBytes((uint)inputBytes.Length).CopyTo(head, 0);
                stream.Write(head, 0, 4);
                stream.Write(inputBytes, 0, inputBytes.Length);
                if (stream.Read(head, 0, 4) != 4)
                {
                    return null;
                }
                var data = Util.ReadStream(stream, (int)BitConverter.ToUInt32(head, 0));
                var dataString = Encoding.Unicode.GetString(data);
                return Message.Parse(dataString);
            }
        }

        public RecTaskInfo GetTaskInfo()
        {
            RecTaskSharedMemoryStruct shared = new RecTaskSharedMemoryStruct();
            try
            {
                using (MemoryMappedFile sm = MemoryMappedFile.OpenExisting(RECTASK_SERVER_SHARED_MEMORY_NAME + taskID))
                {
                    var ma = sm.CreateViewAccessor(0, Marshal.SizeOf(typeof(RecTaskSharedMemoryStruct)));
                    ma.Read(0, out shared);
                }
            }
            catch (Exception ex)
            {

            }
            return new RecTaskInfo(shared);
        }

        #region Win32 API
        private static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        private const int MAX_PATH = 260;
        private const int MAX_ALTERNATE = 14;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh; //changed all to uint from int, otherwise you run into unexpected overflow
            public uint nFileSizeLow;  //| http://www.pinvoke.net/default.aspx/Structures/WIN32_FIND_DATA.html
            public uint dwReserved0;   //|
            public uint dwReserved1;   //v
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA
           lpFindFileData);
        [DllImport("kernel32.dll")]
        static extern bool FindClose(IntPtr hFindFile);
        #endregion
    }
}
