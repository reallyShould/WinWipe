﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace WinWipe
{
    public partial class MainWindow : Window
    {
        SystemAdd SysAdd = new SystemAdd();
        Cleaner cleaner = new Cleaner();

        static public string version = "1.2";
        static public string authors = "reallyShould";
        public string customFolder = null;
        public int counter = 0;
        public string start_message = $"=================\nWinWipe by {authors}\nVersion {version}\n=================\n";

        List<string> apps = new List<string>() { ".exe", ".msi" };
        List<string> audio = new List<string>() { ".mp3", ".wav", ".ogg", ".aac", ".flac", ".alac", ".dsd" };
        List<string> video = new List<string>() { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".avchd" };
        List<string> images = new List<string>() { ".svg", ".png", ".jpeg", ".jpg", ".gif", ".raw", ".tiff" };
        List<string> archives = new List<string>() { ".zip", ".rar", ".tar", ".7z", ".cab", ".arj", ".lzh" };
        List<string> torrents = new List<string>() { ".torrent" };
        List<string> word = new List<string>() { ".doc", ".docx" };

        Dictionary<string, CheckBox> checkers = new Dictionary<string, CheckBox>();
        Dictionary<string, Action> checkersDes = new Dictionary<string, Action>();
        Dictionary<string, CheckBox> checkersDownloads = new Dictionary<string, CheckBox>();
        Dictionary<string, List<string>>  checkersDownloadsDes = new Dictionary<string, List<string>>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                SysAdd.init();
                cleaner.init();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            SelectScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogScrollXAML.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LogsTextBoxXAML.Text = start_message;

            Title = $"WinWipe {version} [{SysAdd.user_name}] Admin: {SysAdd.admin} Disk: {SysAdd.activeDisk}";

            if (customFolder == null)
            {
                CustomFolderCheckerXAML.IsEnabled = false;
            }

            //DICTS FOR MAIN
            checkers = new Dictionary<string, CheckBox>()
            {
                { "TmpCheckerXAML", TmpCheckerXAML },
                { "RecycleCheckerXAML", RecycleCheckerXAML },
                { "UpdatesCheckerXAML", UpdatesCheckerXAML },
                { "CustomFolderCheckerXAML", CustomFolderCheckerXAML },
                { "WebCacheCheckerXAML", WebCacheCheckerXAML },
                { "CrushDumpsCheckerXAML", CrushDumpsCheckerXAML },
                { "SteamCacheCheckerXAML", SteamCacheCheckerXAML }
            };

            checkersDes = new Dictionary<string, Action>()
            {
                { "TmpCheckerXAML", () => cleaner.CleanTemporary(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "RecycleCheckerXAML", () => cleaner.CleanRecycleBin(LogsTextBoxXAML, LogScrollXAML, CleanButtonXAML) },
                { "UpdatesCheckerXAML", () => cleaner.CleanOldUpdates(LogsTextBoxXAML, LogScrollXAML) },
                { "CustomFolderCheckerXAML", () => cleaner.CleanCustomFolder(customFolder, LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "WebCacheCheckerXAML", () => cleaner.CleanWebCache(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "CrushDumpsCheckerXAML", () => cleaner.CleanCrushDumps(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) },
                { "SteamCacheCheckerXAML", () => cleaner.CleanSteamCache(LogsTextBoxXAML, LogScrollXAML, FinalLabelXAML, Application.Current.Dispatcher) }
            };

            
            //DICTS FOR DOWNLOADS
            checkersDownloads = new Dictionary<string, CheckBox>()
            {
                { "ExeCheckerXAML", ExeCheckerXAML },
                { "AudioCheckerXAML", AudioCheckerXAML },
                { "VideoCheckerXAML", VideoCheckerXAML },
                { "ImagesCheckerXAML", ImagesCheckerXAML },
                { "TorrentsCheckerXAML", TorrentsCheckerXAML },
                { "ArchiveCheckerXAML", ArchiveCheckerXAML },
                { "DocumentsCheckerXAML", DocumentsCheckerXAML }
            };

            checkersDownloadsDes = new Dictionary<string, List<string>>()
            {
                { "ExeCheckerXAML", apps },
                { "AudioCheckerXAML", audio },
                { "VideoCheckerXAML", video },
                { "ImagesCheckerXAML", images },
                { "TorrentsCheckerXAML", torrents },
                { "ArchiveCheckerXAML", archives },
                { "DocumentsCheckerXAML", word }
            };
        }

        //BUTTONS EVENTS
        private void LogsButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    cleaner.writeLogs();
                    Process.Start("explorer.exe", SysAdd.defaultLogDir);
                }
                catch (Exception ex)
                {
                    LogsTextBoxXAML.Text = ex.Message;
                    Debug.WriteLine("Write error: " + ex.Message);
                }
            }));
        }

        private void CleanLogsButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => LogsTextBoxXAML.Clear()));
        }

        private void btn_ChangeCustomFolderButtonXAML(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.ShowDialog();
            if (dialog.SelectedPath != "")
            {
                customFolder = dialog.SelectedPath;
                CustomFolderCheckerXAML.IsEnabled = true;
                CustomFolderCheckerXAML.IsChecked = true;
                Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Custom folder is " + customFolder, LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            }
        }

        private void btn_clean(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => SysAdd.ResetFinal(FinalLabelXAML)));
            CleanButtonXAML.IsEnabled = false;

            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("Clean start", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));
            Dispatcher.Invoke(new Action(() => SysAdd.AddLog("sep", LogsTextBoxXAML, LogScrollXAML, Application.Current.Dispatcher)));

            foreach (FrameworkElement checkBox in StackPanelXAML.Children)
            {
                if (checkBox is CheckBox || checkers.ContainsKey(checkBox.Name))
                {
                    try
                    {
                        if (checkers[checkBox.Name].IsChecked == true)
                        {
                            Dispatcher.Invoke(checkersDes[checkBox.Name]);
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}");
                    }
                }
            }

            //CLEAN DOWNLOADS
            foreach (FrameworkElement checkBox in StackPanelXAML.Children)
            {
                if (checkBox is CheckBox || checkersDownloads.ContainsKey(checkBox.Name))
                {
                    try
                    {
                        if (checkersDownloads[checkBox.Name].IsChecked == true)
                        {
                            Dispatcher.Invoke(new Action(() => cleaner.CleanDownloads(checkersDownloadsDes[checkBox.Name], 
                                                                                      LogsTextBoxXAML, 
                                                                                      LogScrollXAML, 
                                                                                      FinalLabelXAML, 
                                                                                      Application.Current.Dispatcher)));
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine($"Warning: {err}\ncheckBox: {checkBox.Name}");
                    }
                }
            }
            CleanButtonXAML.IsEnabled = true;
        }

        private void GrammerButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Grammer grammer = new Grammer();
                grammer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                grammer.Owner = this;
                grammer.Show();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void HelpButtonXAML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists($"{SysAdd.defaultLogDir}\\About.html"))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/reallyShould/WinWipe/releases/download/1.1b2/About.html", $"{SysAdd.defaultLogDir}\\About.html");
                    }
                }
                Process.Start($"{SysAdd.defaultLogDir}\\About.html");
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
