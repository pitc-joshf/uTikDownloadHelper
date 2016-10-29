﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace uTikDownloadHelper
{
    public static class HelperFunctions
    {
        public static void patchTicket(ref Byte[] bytes)
        {
            if (bytes[0x01] == 0x3)
            {
                bytes[0x1] = 1;
                bytes[0xF] = (byte)(bytes[0xF] ^ 2);
            }
        }
        public static readonly string[] SizeSuffixes =
                   { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value)
        {
            if (value == 0) { return "0" + SizeSuffixes[0]; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n2} {1}", adjustedSize, SizeSuffixes[mag]);
        }
        public static long DirSize(string Directory)
        {
            DirectoryInfo d = new DirectoryInfo(Directory);
            d.Refresh();
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                fi.Refresh();
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di.FullName);
            }
            return size;
        }
        public static string ReplaceInvalidFilenameCharacters(string input, string replacementChar = "_")
        {
            string output = input;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                output = output.Replace(c.ToString(), replacementChar);
            }

            foreach (char c in Path.GetInvalidPathChars())
            {
                output = output.Replace(c.ToString(), replacementChar);
            }
            return output;
        }
        public async static Task<byte[]> DownloadTitleKeyWebsiteTicket(string TitleID)
        {
            string url = "https://" + Common.Settings.ticketWebsite + "/ticket/" + TitleID + ".tik";
            byte[] data = await (new WebClient()).DownloadDataTaskAsync(new Uri(url));
            return data;
        }
        public static string GetAutoIncrementedDirectory(string baseDirectory, string name)
        {
            int counter = 0;
            name = ReplaceInvalidFilenameCharacters(name);
            string path = Path.Combine(baseDirectory, name);
            while(File.Exists(path) || Directory.Exists(path))
            {
                if (Directory.Exists(path) && Directory.GetFiles(path).Count() == 0 && Directory.GetDirectories(path).Count() == 0)
                    break;

                counter++;
                path = Path.Combine(baseDirectory, name + (counter > 0 ? " (" + counter + ")" : ""));
            }
            return path;
        }
        public static string escapeCommandArgument(string argument)
        {
            string s = argument;

            s = Regex.Replace(s, @"(\\*)" + "\"", @"$1$1\" + "\"");
            s = "\"" + Regex.Replace(s, @"(\\+)$", @"$1$1") + "\"";

            return s;
        }
    }
}