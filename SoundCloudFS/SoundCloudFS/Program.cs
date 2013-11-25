using System;
using System.Net;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dokan;

namespace SoundCloudFS
{
    class SoundCloudFS : DokanOperations
    {
        private string token_;
        private string cl_ID_;
        private WebClient client;
        private string StreamList = "";
        public SoundCloudFS(string token,string clientid)
        {
            token_ = token;
            cl_ID_ = clientid;
            client = new WebClient();
        }

        #region API Zone
        JArray GetStreamLine()
        {
            try
            {
                // this is used to get the "stream" or timeline on the soundcloud app/frontpage
                string boop;
                if (StreamList == "")
                {
                    boop = client.DownloadString("https://api.soundcloud.com/e1/me/stream.json?oauth_token=" + token_ + "&limit=50");
                    StreamList = boop;
                }
                else
                {
                    boop = StreamList;
                }
                dynamic InitObj = JObject.Parse(boop);
                return InitObj.collection;
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to get stream/timeline.");
                Console.Read();
                Environment.Exit(1);
            }
            return null;
        }

        string GetSCURL(string EntryName)
        {
            JArray Stream = GetStreamLine();
            foreach (JObject a in GetStreamLine())
            {
                dynamic obj = JObject.Parse(a["track"].ToString()); // lolwtf
                if (obj.name == EntryName)
                {
                    return obj.stream_url + "&secret_token&client_id=" + cl_ID_;
                }
            }
            return "";
        }

        string GetSCFile(dynamic trackobj)
        {
            try
            {
                Console.WriteLine("Fetching Track " + trackobj.id);
                string SaveDir = Path.GetTempPath();
                if (File.Exists(SaveDir + "/SCFSTemp" + trackobj.id))
                {
                    return SaveDir + "/SCFSTemp" + trackobj.id;
                }
                client.DownloadFile(trackobj.stream_url + "&secret_token&client_id=" + cl_ID_, SaveDir + "/SCFSTemp" + trackobj.id);
                return SaveDir + "/SCFSTemp" + trackobj.id;
            }
            catch
            {
                Console.WriteLine("Failed to get track!!!");
                Console.Read();
                Environment.Exit(0);
                return ""; // Keep VS happy I guess
            }

        }

        #endregion

        #region Dokan Zone
        public int CreateFile(String filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, DokanFileInfo info)
        {
            /*
            string path = GetPath(filename);
            info.Context = count_++;
            if (File.Exists(path))
            {
                return 0;
            }
            else if (Directory.Exists(path))
            {
                info.IsDirectory = true;
                return 0;
            }
            else
            {*/
                return -DokanNet.ERROR_FILE_NOT_FOUND;
            //}
        }

        public int OpenDirectory(String filename, DokanFileInfo info) // This is used
        {
            Console.WriteLine("Attempting to open path {0}", filename);
            if (filename == "\\")
            {
                return 0;
            }
            else if (filename == "\\stream")
            {
                return 0;
            }
            /*
            info.Context = count_++;
            if (Directory.Exists(GetPath(filename)))
                return 0;
            else
             */
                return -DokanNet.ERROR_PATH_NOT_FOUND;
        }

        public int CreateDirectory(String filename, DokanFileInfo info)
        {
            return -1;
        }

        public int Cleanup(String filename, DokanFileInfo info)
        {
            return 0;
        }

        public int CloseFile(String filename, DokanFileInfo info)
        {
            return 0;
        }

        public int ReadFile(String filename, Byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info) // This is used
        {
            Console.WriteLine("Attempting to open file {0}", filename);
            if (filename.StartsWith("\\stream\\"))
            {
                Console.WriteLine("Reasonable request.");
                string endname = filename.Split('\\')[filename.Split('\\').Length - 1];
                string TargetURL = GetSCURL(endname.Replace(".mp3", ""));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TargetURL);
                request.AddRange((int)offset, buffer.Length);
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                Stream streamm = response.GetResponseStream();
                int w = streamm.Read(buffer, (int)offset, (int)readBytes);
            }
            /*
            try
            {
                FileStream fs = File.OpenRead(GetPath(filename));
                fs.Seek(offset, SeekOrigin.Begin);
                readBytes = (uint)fs.Read(buffer, 0, buffer.Length);
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }*/
            return -1;
        }

        public int WriteFile(String filename, Byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            return -1;
        }

        public int FlushFileBuffers(String filename, DokanFileInfo info)
        {
            return -1;
        }

        public int GetFileInformation(String filename, FileInformation fileinfo, DokanFileInfo info) // I guess this is used?
        {
            if (filename.StartsWith("\\stream"))
            {
                foreach (JObject a in GetStreamLine())
                {
                    try
                    {
                        dynamic obj = JObject.Parse(a["track"].ToString()); // lolwtf
                        if (obj.title == filename.Replace(@"\stream\", ""))
                        {
                            fileinfo.Attributes = FileAttributes.Normal;
                            fileinfo.CreationTime = DateTime.Today;
                            fileinfo.LastAccessTime = DateTime.Now;
                            fileinfo.LastWriteTime = DateTime.Now;
                            fileinfo.FileName = obj.title + ".mp3";
                            fileinfo.Length = (obj.duration / 1000) * 16 * 1024;
                            return 0;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Nothing bad ever happened.");
                        return -1;
                    }
                }
                return -1;
            }
            return -1;
            /*
            string path = GetPath(filename);
            if (File.Exists(path))
            {
                FileInfo f = new FileInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = f.Length;
                return 0;
            }
            else if (Directory.Exists(path))
            {
                DirectoryInfo f = new DirectoryInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = 0;// f.Length;
                return 0;
            }
            else
            {
                return -1;
            }*/
            return -1;
        }

        public int FindFiles(String filename, ArrayList files, DokanFileInfo info) // This is used
        {
            Console.WriteLine("listing files in {0}",filename);
            if (filename == "\\")
            {
                // We need to chuck in a stream folder for now.
                FileInformation fi = new FileInformation();
                fi.Attributes = FileAttributes.Directory;
                fi.FileName = "stream";
                fi.CreationTime = DateTime.Today;
                fi.LastAccessTime = DateTime.Now;
                fi.LastWriteTime = DateTime.Now;
                fi.Length = 0;
                files.Add(fi);
                // TODO: add user folders.
                return 0;
            }
            if (filename == "\\stream")
            {
                foreach (JObject a in GetStreamLine())
                {
                    try
                    {
                        FileInformation fi = new FileInformation();
                        dynamic obj = JObject.Parse(a["track"].ToString()); // lolwtf
                        fi.Attributes = FileAttributes.Normal;
                        fi.CreationTime = DateTime.Today;
                        fi.LastAccessTime = DateTime.Now;
                        fi.LastWriteTime = DateTime.Now;
                        fi.FileName = obj.title + ".mp3";
                        fi.Length = (obj.duration / 1000) * 16 * 1024;
                        files.Add(fi);
                    }
                    catch
                    {
                        Console.WriteLine("Only a minor explosion happened. its fine.");
                    }
                }
                return 0;
            }
            /*
            string path = GetPath(filename);
            if (Directory.Exists(path))
            {
                DirectoryInfo d = new DirectoryInfo(path);
                FileSystemInfo[] entries = d.GetFileSystemInfos();
                foreach (FileSystemInfo f in entries)
                {
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = (f is DirectoryInfo) ? 0 : ((FileInfo)f).Length;
                    fi.FileName = f.Name;
                    files.Add(fi);
                }
                return 0;
            }
            else
            {
                return -1;
            }*/
            return -1;
        }

        public int SetFileAttributes(String filename, FileAttributes attr, DokanFileInfo info)
        {
            return -1;
        }

        public int SetFileTime(String filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            return -1;
        }

        public int DeleteFile(String filename, DokanFileInfo info)
        {
            return -1;
        }

        public int DeleteDirectory(String filename, DokanFileInfo info)
        {
            return -1;
        }

        public int MoveFile(String filename, String newname, bool replace, DokanFileInfo info)
        {
            return -1;
        }

        public int SetEndOfFile(String filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int SetAllocationSize(String filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int LockFile(String filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int UnlockFile(String filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {
            return 0;
        }
        #endregion
        static void Main(string[] args)
        {
            WebClient client = new WebClient(); // I hope this works.
            string[] Deets = File.ReadAllLines("./api_info.txt");
            /* THIS FILE NEEDS TO CONTAIN THE FOLLOWING (each on a new line)
                ClientId
                ClientSecret
                username
                password
             */

            
            if (Deets.Length != 4)
            {
                Console.WriteLine("Please fill out the \"api_info.txt\" with the following:");
                Console.WriteLine(@"
ClientId
ClientSecret
username
password");
                Environment.Exit(1);
            }
            string postData = "client_id=" + Deets[0]
                            + "&client_secret=" + Deets[1]
                            + "&grant_type=password&username=" + Deets[2]
                            + "&password=" + Deets[3];
            string soundCloudTokenRes = "https://api.soundcloud.com/oauth2/token";
            // Now we send off the API Request:
            string tokenInfo = "";
            try
            {
                tokenInfo = client.UploadString(soundCloudTokenRes, postData);
            }
            catch
            {
                Console.WriteLine("Failed to auth with soundcloud.");
                Console.Read();
                Environment.Exit(1);
            }
            dynamic AuthInfo;
            string AToken = "";
            try
            {
                AuthInfo = JObject.Parse(tokenInfo);
                AToken = AuthInfo.access_token;
            }
            catch
            {
                Console.WriteLine("Malformed responce from soundcloud D:");
                Environment.Exit(1);
            }
            try
            {
                dynamic meobj = JObject.Parse(client.DownloadString("https://api.soundcloud.com/me.json?oauth_token=" + AToken));
                Console.WriteLine("Thank you for using SoundcloudFS" + meobj.username);
            }
            catch
            {

            }
            Console.WriteLine("Starting Mounting process...");

            
            DokanOptions opt = new DokanOptions();
            opt.DebugMode = true;
            opt.MountPoint = "v:\\";
            opt.ThreadCount = 5;
            int status = DokanNet.DokanMain(opt, new SoundCloudFS(AToken,Deets[1]));
            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    Console.WriteLine("Driver letter error");
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    Console.WriteLine("Driver install error");
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    Console.WriteLine("Mount error");
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    Console.WriteLine("Start error");
                    break;
                case DokanNet.DOKAN_ERROR:
                    Console.WriteLine("Unknown error");
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    Console.WriteLine("Success");
                    break;
                default:
                    Console.WriteLine("Unknown status: %d", status);
                    break;

            }
        }
    }
}
