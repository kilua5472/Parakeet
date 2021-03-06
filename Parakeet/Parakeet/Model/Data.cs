﻿using System;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Xml;
using Manager.Manager;
using Parakeet.Properties;

namespace Parakeet.Model
{
    public sealed class Data
    {
        public static string DirectoryToSave = "\\Parakeet\\";

        public static string FullPathSaveDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + DirectoryToSave;

        public SerializableList<ChangeRule> RenameRules;
        public SerializableList<RemoveRule> RemoveRules;
        public SerializableList<DirectoryModel> DirectoryModels;
        public SerializableList<SortByRule> SortRules;

        private string _fileTitle;

        public FFManager.FFManager FFManager = new FFManager.FFManager();
        public SManager.SManager SManager = new SManager.SManager();

        private static Data _instance;
        static readonly object Instancelock = new object();

        private Data()
        {
            if (!Directory.Exists(FullPathSaveDirectory))
            {
                Directory.CreateDirectory(FullPathSaveDirectory);
                DirectorySecurity sec = Directory.GetAccessControl(FullPathSaveDirectory);
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                sec.AddAccessRule(new FileSystemAccessRule(everyone,
                    FileSystemRights.Modify | FileSystemRights.Synchronize,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(FullPathSaveDirectory, sec);
            }

            RenameRules = new SerializableList<ChangeRule>();
            DirectoryModels = new SerializableList<DirectoryModel>();
            RemoveRules = new SerializableList<RemoveRule>();
            SortRules = new SerializableList<SortByRule>();
            FFManager.BwTask.RunWorkerCompleted += TaskCompleted;
            SManager.BwTask.RunWorkerCompleted += TaskCompleted;
        }

        private void TaskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show(App.Current.MainWindow, Resources.Data_TaskCompleted_ActionUserCancel, Resources.Data_TaskCompleted_Result);
                return;
            }
            var res = e.Result as int?;
            MessageBox.Show(App.Current.MainWindow, Resources.Data_TaskCompleted_ActionExecuteOn + res + " éléments !", Resources.Data_TaskCompleted_Result);
        }

        public static Data GetInstance()
        {
            if (_instance == null)
            {
                lock (Instancelock)
                    if (_instance == null)
                        _instance = new Data();
            }
            return _instance;
        }

        public string FileTitle
        {
            get { return _fileTitle; }
            set { _fileTitle = value; }
        }

        public void ReadData()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlReader xr = XmlReader.Create(FileTitle, settings))
            {
                DirectoryModels.ReadXml(xr);
                RenameRules.ReadXml(xr);
                RemoveRules.ReadXml(xr);
                SortRules.ReadXml(xr);
                xr.Close();
            }
        }

        public void WriteData()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            try
            {
                using (XmlWriter wr = XmlWriter.Create(FileTitle, settings))
                {
                    DirectoryModels.WriteXml(wr);
                    RenameRules.WriteXml(wr);
                    RemoveRules.WriteXml(wr);
                    SortRules.WriteXml(wr);
                    wr.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Resources.Data_WriteData_Error + e.Message);
            }
        }
    }
}