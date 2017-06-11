using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.ipc
{
    public class IpcClient : IDisposable
    {
        MemoryMappedFile file;
        MemoryMappedViewAccessor statusView;

        public IpcClient()
        {
            file = MemoryMappedFile.OpenExisting(Constants.MemoryMappedFiles.Status, MemoryMappedFileRights.Read, HandleInheritability.None);

            statusView = file.CreateViewAccessor(1024, 512, MemoryMappedFileAccess.Read);
        }

        public ServerStatus ReadStatus()
        {
            var lengthOfVersionString = statusView.ReadByte(0);
            byte[] versionStringBytes = new byte[lengthOfVersionString];
            for (var i = 0; i < lengthOfVersionString; i++)
            {
                versionStringBytes[i] = statusView.ReadByte(i + sizeof(byte));
            }

            var maximumNumberOfPlayers  = statusView.ReadInt32(versionStringBytes.Length + sizeof(byte));
            var currentNumberOfPlayers  = statusView.ReadInt32(versionStringBytes.Length + sizeof(byte) + sizeof(Int32));
            var version = Encoding.UTF8.GetString(versionStringBytes, 0, versionStringBytes.Length);

            return new ServerStatus
            {
                Version = version,
                MaximumNumbersOfPlayers = maximumNumberOfPlayers,
                CurrentNumberOfPlayers = currentNumberOfPlayers
            };
        }

        public void Dispose()
        {
            statusView?.Dispose();
            statusView = null;
            file?.Dispose();
            file = null;
        }
    }
}
