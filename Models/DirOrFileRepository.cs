using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DiskSpaceWebUI.Models
{
    public class DirOrFileRepository
    {
        public bool type { get; set; }
        public DirOrFileRepository(bool type)
        {
            this.type = type;
        }
        public List<DirOrFile> ScanFolders(string folderPath)
        {
            List<DirOrFile> DataList = new List<DirOrFile>();
            IEnumerable<string> currentDirFolders = Directory.EnumerateDirectories(folderPath);
            foreach(string folder in currentDirFolders)
            {
                if (type)
                {
                   DataList.Add(new DirOrFile() { Type = "Папка", Name = folder.Remove(0, folderPath.Length), 
                       Path = folder, Weight = GetDirectorySize(folder), DiskSpaceTaken = GetDirectorySizeOnDisk(folder) });
                }
                else
                {
                    DataList.Add(new DirOrFile() { Type = "Папка", Name = folder.Remove(0, folderPath.Length), 
                        Path = folder });//,Weight = GetDirectorySize(folder), DiskSpaceTaken = GetDirectorySizeOnDisk(folder)
                }
               
            }
            return DataList;
        }
        public List<DirOrFile> ScanFiles(string folderPath)
        {
            List<DirOrFile> DataList = new List<DirOrFile>();
            DirectoryInfo di = new DirectoryInfo(folderPath);
            IEnumerable<FileInfo> currentDirFilesFI = di.EnumerateFiles();
            foreach (FileInfo fileInfo in currentDirFilesFI)
            {
                DataList.Add(new DirOrFile() { Type = "Файл", Name = fileInfo.Name, Weight = fileInfo.Length, 
                    DiskSpaceTaken = GetFileSizeOnDisk(fileInfo.FullName, fileInfo) });
            }
            return DataList;
        }
        private long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            long a=0;
            try
            {
                a = di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch { }
            return a;
            //return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private long GetDirectorySizeOnDisk(string folderPath)
        {
            long size = 0;
            DirectoryInfo di = new DirectoryInfo(folderPath);
            try
            {
                IEnumerable<FileInfo> currentDirFiles = di.EnumerateFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo fileInfo in currentDirFiles)
                {
                    size += GetFileSizeOnDisk(fileInfo.FullName, fileInfo);
                }
            }
            catch { }
            return size;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
           out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);

        public static long GetFileSizeOnDisk(string file, FileInfo info)
        {
            uint dummy, sectorsPerCluster, bytesPerSector;

            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = GetCompressedFileSizeW(file, out hosize);
            long size;
            size = (long)hosize << 32 | losize;
            long sizeondisk = clusterSize * ((size + clusterSize - 1) / clusterSize);
            return sizeondisk;
        }
        public List<DirOrFile> Sort(List<DirOrFile> DataList, bool order)
        {
            DataList = QuickSort(DataList);
            if (!order)
            {
                DataList.Reverse();
            }
            return DataList;
        }

        static int Partition(List<DirOrFile> list, int minIndex, int maxIndex)
        {
            var pivot = minIndex - 1;
            for (var i = minIndex; i < maxIndex; i++)
            {
                if (list[i].DiskSpaceTaken < list[maxIndex].DiskSpaceTaken)
                {
                    pivot++;
                    DirOrFile t = list[pivot];
                    list[pivot] = list[i];
                    list[i] = t;
                }
            }

            pivot++;
            DirOrFile x = list[pivot];
            list[pivot] = list[maxIndex];
            list[maxIndex] = x;
            return pivot;
        }

        static List<DirOrFile> QuickSort(List<DirOrFile> list, int minIndex, int maxIndex)
        {
            if (minIndex >= maxIndex)
            {
                return list;
            }

            var pivotIndex = Partition(list, minIndex, maxIndex);
            QuickSort(list, minIndex, pivotIndex - 1);
            QuickSort(list, pivotIndex + 1, maxIndex);

            return list;
        }

        static List<DirOrFile> QuickSort(List<DirOrFile> list)
        {
            return QuickSort(list, 0, list.Count - 1);
        }
    }
}
