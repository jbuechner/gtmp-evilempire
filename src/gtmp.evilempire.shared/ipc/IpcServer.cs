using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.ipc
{
    public class IpcServer : IDisposable
    {
        MemoryMappedFile file;
        MemoryMappedViewAccessor statusView;

        public IpcServer()
        {
            file = MemoryMappedFile.CreateNew(Constants.MemoryMappedFiles.Status, 4096, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, HandleInheritability.None);
            statusView = file.CreateViewAccessor(1024, 512, MemoryMappedFileAccess.Write);
        }

        public void UpdateStatus(ServerStatus status)
        {
            byte sizeOfVersionInByte = (byte)status.Version.Length;
            statusView.Write(0, sizeOfVersionInByte);

            var versionBytes = Encoding.UTF8.GetBytes(status.Version);
            for (var i = 0; i < versionBytes.Length; i++)
            {
                statusView.Write(i + sizeof(byte), versionBytes[i]);
            }

            statusView.Write(versionBytes.Length + sizeof(byte), status.MaximumNumbersOfPlayers);
            statusView.Write(versionBytes.Length + sizeof(byte) + sizeof(Int32), status.CurrentNumberOfPlayers);
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
