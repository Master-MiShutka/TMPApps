﻿namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    public class StartViewModel : BaseViewModel
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private AramisDataInfo selectedDataInfo;
        private string aramisDbPath;
        private bool aramisDbSelected;

        public StartViewModel()
        {
            this.CommandSelectAndLoad = new DelegateCommand<AramisDataInfo>(
                (adi) =>
            {
                if (this.SelectedDataInfo == null && adi == null)
                {
                    Repository.Instance.AddNewAramisDbPath(this.AramisDbPath);

                    // теперь если список файлов был пуст, нужно выбрать добавленный
                    var selected = Repository.Instance.AvailableDataFiles.Where(file => file.IsSelected == true).ToList();

                    if (selected.Any() == false)
                    {
                        // тогда создаем новый
                        this.SelectedDataInfo = new AramisDataInfo()
                        {
                            DepartamentName = Repository.GetDepartamentName(this.AramisDbPath),
                            AramisDbPath = this.AramisDbPath,
                            IsSelected = false,
                        };
                    }
                    else
                    {
                        this.SelectedDataInfo = selected.FirstOrDefault() as Model.AramisDataInfo;
                    }
                }

                if (this.SelectedDataInfo == null && adi != null)
                {
                    this.SelectedDataInfo = adi;
                }

                // итак подразделение выбрано - загрузка данных
                MainViewModel.SelectedDataFileInfo = this.SelectedDataInfo;
            },
                (o) => this.SelectedDataInfo != null || (this.AramisDbSelected && string.IsNullOrEmpty(this.AramisDbPath) == false),
                "Выбрать\nи загрузить");

            this.CommandGoToSettings = new DelegateCommand(
                () => MainViewModel.ChangeMode(Mode.Preferences),
                "Перейти к параметрам\nи указать путь к базе данных");

            this.CommandDeleteDataFile = new DelegateCommand<TMP.Common.RepositoryCommon.IDataFileInfo>(
                (dataFileInfo) =>
            {
                try
                {
                    System.IO.File.Delete(dataFileInfo.FileName);
                }
                catch (Exception e)
                {
                    App.ShowWarning($"Файл не удалось удалить.\nОписание проблемы:\n{App.GetExceptionDetails(e)}");
                }
            }, (o) => o != null);

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                this.AramisDbPath = @"d:\3tkd\efd33r \3 3 er3";

                this.DataFileInfos = new List<AramisDataInfo>
                {
                    new AramisDataInfo { AramisDbPath = @"d:\1\123\456456", DepartamentName = "Derpr", FileName = @"rgrgrg.DATA", FileSize = 12115453446464, LastModifiedDate = DateTime.Now.AddDays(-20) },
                    new AramisDataInfo { AramisDbPath = @"d:\efgegf\123\456456", DepartamentName = "Test", FileName = @"nbgfntgn.DATA", FileSize = 5754756, LastModifiedDate = DateTime.Now.AddDays(-2) },
                    new AramisDataInfo { AramisDbPath = @"r:\2e4\456456", DepartamentName = "Ojhfy", FileName = @"ergf.DATA", FileSize = 75476533, LastModifiedDate = DateTime.Now.AddHours(-1) },
                };

                return;
            }

            this.DataFileInfos = Repository.Instance.AvailableDataFiles.Cast<AramisDataInfo>().ToList();
        }

        public List<AramisDataInfo> DataFileInfos { get; }

        public bool HasDataFiles => this.DataFileInfos.Count > 0;

        public AramisDataInfo SelectedDataInfo { get => this.selectedDataInfo; set => this.SetProperty(ref this.selectedDataInfo, value); }

        public ICommand CommandSelectAndLoad { get; }

        public ICommand CommandGoToSettings { get; }

        public ICommand CommandDeleteDataFile { get; }

        public string AramisDbPath
        {
            get => this.aramisDbPath;
            set
            {
                if (this.SetProperty(ref this.aramisDbPath, value))
                {
                    this.SelectedDataInfo = null;
                }
            }
        }

        public bool AramisDbSelected
        {
            get => this.aramisDbSelected;
            set => this.SetProperty(ref this.aramisDbSelected, value);
        }
    }
}