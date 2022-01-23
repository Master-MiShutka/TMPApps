namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.Properties;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        /// <summary>
        /// Создание сводной информации
        /// </summary>
        /// <param name="meters">Список счетчиков</param>
        /// <returns></returns>
        private ObservableCollection<SummaryInfoItem> BuildSummaryInfo(IEnumerable<Meter> meters)
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.AramisDBParser>BuildSummaryInfo");

            Model.WorkTask workTask = new("формирование сводной информации");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            if (meters == null)
            {
                return null;
            }

            int totalRows = ModelHelper.MeterSummaryInfoProperties.Count;
            int processedRows = 0;

            List<SummaryInfoItem> infos = new();

            // по всем свойствам
            foreach (var field in AppSettings.Default.SummaryInfoFields)
            {
                infos.Add(SummaryInfoHelper.BuildSummaryInfoItem(meters, field.Name));
                workTask.UpdateUI(++processedRows, totalRows, stepNameString: "формирование свода");
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "формирование свода");

            workTask.IsCompleted = true;
            return new ObservableCollection<SummaryInfoItem>(infos);
        }
    }
}
