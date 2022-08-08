using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using TMP.Common.RepositoryCommon;

namespace Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        private void TMPApp_Startup(object sender, StartupEventArgs e)
        {
            /*var p1 = BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.XmlDataContractDeSerialize("Settings\\DataModel-dc.xml", null);
            var p2 = BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.JsonDeSerialize("Settings\\DataModel.json", null);
            var p3 = BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.GzJsonDeSerialize("Settings\\DataModel.gz.json", null);

            string hash1 = p1.GetMD5Hash();
            string hash2 = p2.GetMD5Hash();
            string hash3 = p3.GetMD5Hash();

            var q1 = hash1 == hash2;
            var q2 = hash2 == hash3;

            BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.JsonSerialize(p1, "Settings\\DataModel.json");
            BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.GzJsonSerialize(p1, "Settings\\DataModel.gz.json");

            var p = BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.XmlDeSerialize("Settings\\DataModel.xml");
            BaseRepository<TMP.Work.Emcos.Model.EmcosPoint>.XmlDataContractSerialize(p, "Settings\\DataModel-dc.xml", null);*/
            //Common.RepositoryCommon.BaseRepository<Model.EmcosPoint>.JsonSerialize(p, "qqqqqqq-J", null);
        }
    }
}
