// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : Loc.cs
// Created    : 18/12/2019

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
namespace DataGridWpf
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public enum Local
    {
        English = 0,
        French,
        Russian,
        German,
        Italian,
        Chinese,
    }

    public class Loc
    {
        #region Private Fields

        private int language;

        #endregion Private Fields

        #region Private Fields

        // culture name(used for dates)
        private static readonly string[] CultureNames = { "en-US", "fr-FR", "ru-RU", "de-DE", "it-IT", "zh-Hans" };

        // RESPECT THE ORDER OF THE Local ENUMERATION
        // translation
        private static readonly Dictionary<string, string[]> Translation = new Dictionary<string, string[]>
        {
            {
                "All", new[]
                {
                    "(Select all)",
                    "(Sélectionner tout)",
                    "(Выбрать все)",
                    "(Alle auswählen)",
                    "(Seleziona tutto)",
                    "(全选)",
                }
            },
            {
                "Empty", new[]
                {
                    "(Blank)",
                    "(Vides)",
                    "(пусто)",
                    "(Leer)",
                    "(Vuoto)",
                    "(空白)",
                }
            },
            {
                "Clear", new[]
                {
                    "Clear filter \"{0}\"",
                    "Effacer le filtre \"{0}\"",
                    "Очистить фильтр \"{0}\"",
                    "Filter löschen \"{0}\"",
                    "Cancella filtro \"{0}\"",
                    "清除过滤器 \"{0}\"",
                }
            },
            {
                "Search", new[]
                {
                    "Search (contains)",
                    "Rechercher (contient)",
                    "Искать (содержит)",
                    "Suche (enthält)",
                    "Cerca (contiene)",
                    "搜索（包含)",
                }
            },
            {
                "Ok", new[]
                {
                    "Ok",
                    "Ok",
                    "Ok",
                    "Ok",
                    "Ok",
                    "确定",
                }
            },
            {
                "Cancel", new[]
                {
                    "Cancel",
                    "Annuler",
                    "Отмена",
                    "Abbrechen",
                    "Annulla",
                    "取消",
                }
            },
            {
                "Status", new[]
                {
                    "{0:n0} record(s) found on {1:n0}",
                    "{0:n0} enregistrement(s) trouvé(s) sur {1:n0}",
                    "{0:n0} записей найдено на {1:n0}",
                    "{0:n0} zeilen angezeigt von {1:n0}",
                    "{0:n0} record trovati su {1:n0}",
                    "{0:n0} 找到了 {1:n0} 条记录",
                }
            },
            {
                "ElapsedTime", new[]
                {
                    "Elapsed time {0:mm}:{0:ss}.{0:ff}",
                    "Temps écoulé {0:mm}:{0:ss}.{0:ff}",
                    "Затрачено времени {0:mm}:{0:ss}.{0:ff}",
                    "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}",
                    "Tempo trascorso {0:mm}:{0:ss}.{0:ff}",
                    "经过时间{0:mm}:{0:ss}.{0:ff}",
                }
            },
            {
                "Export", new[]
                {
                    "Export data",
                    "Exporter des données",
                    "Экпорт данных",
                    "Daten exportieren",
                    "Esportare i dati",
                    "出口數據",
                }
            },
        };

        #endregion Private Fields

        #region Constructors

        public Loc()
        {
            this.Language = (int)Local.English;
        }

        #endregion Constructors

        #region Public Properties

        public string All => Translation["All"][this.Language];

        public string Cancel => Translation["Cancel"][this.Language];

        public string Clear => Translation["Clear"][this.Language];

        public CultureInfo Culture { get; set; }

        public string CultureName => CultureNames[this.Language];

        public string ElapsedTime => Translation["ElapsedTime"][this.Language];

        public string Empty => Translation["Empty"][this.Language];

        public int Language
        {
            get => this.language;
            set
            {
                this.language = value;
                this.Culture = new CultureInfo(CultureNames[this.Language]);
            }
        }

        public string LanguageName => Enum.GetName(typeof(Local), this.Language);

        public string Ok => Translation["Ok"][this.Language];

        public string Search => Translation["Search"][this.Language];

        public string Status => Translation["Status"][this.Language];

        public string Export => Translation["Export"][this.Language];

        #endregion Public Properties
    }
}
