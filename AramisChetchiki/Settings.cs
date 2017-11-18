using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMP.WORK.AramisChetchiki.Model;

namespace TMP.WORK.AramisChetchiki.Properties {


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {

        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            this.SettingsSaving += this.SettingsSavingEventHandler;

            this.SettingsLoaded += Settings_SettingsLoaded;
        }

        private void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            if (SummaryInfoFields == null)
            {
                SummaryInfoFields = new ObservableCollection<TableField>(GetFields(ModelHelper.MeterSummaryInfoPropertiesCollection.Values).OrderBy(i => i.DisplayOrder));
            }
            if (ChangesOfMetersFields == null)
            {
                ChangesOfMetersFields = new ObservableCollection<TableField>(GetFields(ModelHelper.ChangesOfMetersPropertiesCollection.Values).OrderBy(i => i.DisplayOrder));
            }
            if (Departaments == null)
            {
                Departaments = new ObservableCollection<Model.Departament>();
            }

            this.Save();
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }

        private IEnumerable<TableField> GetFields(ICollection<PlusPropertyDescriptor> props)
        {
            return from i in props
                   select new Properties.TableField()
                   {
                       Type = i.PropertyType,
                       DisplayOrder = i.Order,
                       Name = i.Name,
                       DisplayName = i.DisplayName,
                       GroupName = i.GroupName,
                       IsVisible = i.IsVisible
                   };
        }

        private ICollection<TableField> GetOnlyVisibleAndOrderedFields(ICollection<PlusPropertyDescriptor> props)
        {
            return new List<TableField>(GetFields(props).Where(i => i.IsVisible).OrderBy(i => i.DisplayOrder));
        }

        public IEnumerable<Properties.TableField> GetChangesOfMetersColumnsNames()
        {
            IEnumerable<Properties.TableField> fields = ChangesOfMetersFields;
            int index = 0;
            foreach (var item in fields)
                item.DisplayOrder = index++;
            if (fields == null)
                fields = GetFields(ModelHelper.ChangesOfMetersPropertiesCollection.Values);
            return fields.OrderBy(i => i.DisplayOrder);
        }

        public void SelectDepartamentAndStore(Departament departament)
        {
            System.Diagnostics.Debug.Assert(Departaments != null);
            foreach (Departament d in Departaments)
                d.IsSelected = false;
            Departaments.Where(d => d == departament).First().IsSelected = true;
            Save();
        }

        public bool HasDepartaments => Departaments != null && Departaments.Count > 0;

        public Departament SelectedDepartament
        {
            get
            {
                Departament result = null;
                if (HasDepartaments)
                {
                    var list = Departaments.Where(d => d.IsSelected).ToArray();
                    if (list != null && list.Length > 0)
                        result = list[0];
                }
                return result;
            }
        }

    }
}
