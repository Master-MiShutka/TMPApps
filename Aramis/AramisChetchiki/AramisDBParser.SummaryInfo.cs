namespace TMP.WORK.AramisChetchiki
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using TMP.WORK.AramisChetchiki.Model;

    internal partial class AramisDBParser
    {
        /// <summary>
        /// Создание сводной информации
        /// </summary>
        /// <param name="meters">Список счетчиков</param>
        /// <returns></returns>
        private ReadOnlyCollection<SummaryInfoItem> BuildSummaryInfo(IEnumerable<Meter> meters)
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
            foreach (Shared.PlusPropertyDescriptor field in AppSettings.Default.SummaryInfoFields)
            {
                infos.Add(SummaryInfoHelper.BuildSummaryInfoItem(meters, field.Name));
                workTask.UpdateUI(++processedRows, totalRows, stepNameString: "формирование свода");
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "формирование свода");

            workTask.IsCompleted = true;
            return new ReadOnlyCollection<SummaryInfoItem>(infos);
        }
    }
}
