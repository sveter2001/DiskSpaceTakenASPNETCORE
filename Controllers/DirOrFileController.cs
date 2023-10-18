using DiskSpaceWebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiskSpaceWebUI.Controllers
{
    public class DirOrFileController : Controller
    {
        DirOrFileRepository dirOrFileRepository = new DirOrFileRepository(true);
        public ActionResult Index(string sortOrder, string path)
        {
            if (path == null)
                path = Environment.CurrentDirectory;
            ViewData["DiskSpaceTakenSortParm"] = sortOrder == "ris" ? "desc" : "ris";
            var files = dirOrFileRepository.ScanFiles(path);
            var folders = dirOrFileRepository.ScanFolders(path);
            switch (sortOrder)
            {
                case "ris":
                    files = dirOrFileRepository.Sort(files, true);
                    folders = dirOrFileRepository.Sort(folders, true);
                    files = folders.Concat(files).ToList();
                    break;
                case "desc":
                    files = dirOrFileRepository.Sort(files, false);
                    folders = dirOrFileRepository.Sort(folders, false);
                    files = folders.Concat(files).ToList();
                    break;
                default:
                    files = folders.Concat(files).ToList();
                    break;
            }
            return View("Index", files);
        }
        public ActionResult ChangePath(string sortOrder, string path)
        {
            try
            {
                Environment.CurrentDirectory = path;
            }
            catch
            {

            }
            return Index(sortOrder, Environment.CurrentDirectory);
        }
        public ActionResult GoUp()
        {
            string str = Environment.CurrentDirectory;
            while (str[str.Length-1]!='\\')
            {
                str = str.Remove(str.Length-1);
            }
            if (str.Length > 3)
                str = str.Remove(str.Length - 1);
            if (str.Length == 3)
                return View("Index", dirOrFileRepository.ScanDrives());
            Environment.CurrentDirectory = str;
            return Index("", str);
        }
    }
}
