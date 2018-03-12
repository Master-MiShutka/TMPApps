using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace TMP.WORK.AramisChetchiki.Model
{
    [DataContract]
    [Serializable]
    public class ChangesOfMeters : IModel
    {
        [Display(Order = 0, GroupName = ""), DisplayName("Дата замены"), DataMember]
        public DateTime? Дата_замены
        {
            get;
            set;
        }

        [Display(Order = 1, GroupName = ""), DisplayName("Номер акта"), DataMember]
        public string Номер_акта
        {
            get;
            set;
        }

        [Display(Order = 2, GroupName = ""), DisplayName("Ф.И.О. монтёра"), DataMember]
        public string Фамилия
        {
            get;
            set;
        }

        [Display(Order = 3, GroupName = "Абонент"), DisplayName("Лицевой счёт абонента"), DataMember]
        public ulong Лицевой
        {
            get;
            set;
        }

        [Display(Order = 4, GroupName = "Абонент"), DisplayName("Ф.И.О."), DataMember]
        public string ФИО
        {
            get;
            set;
        }

        [Display(Order = 5, GroupName = "Абонент"), DisplayName("Населённый пункт"), DataMember]
        public string Населённый_пункт
        {
            get;
            set;
        }

        [Display(Order = 6, GroupName = "Абонент"), DisplayName("Адрес"), DataMember]
        public string Адрес
        {
            get;
            set;
        }

        [Display(Order = 7, GroupName = "Снятый счётчик"), DisplayName("Тип снятого счётчика"), DataMember]
        public string Тип_снятого_счетчика
        {
            get;
            set;
        }

        [Display(Order = 8, GroupName = "Снятый счётчик"), DisplayName("Номер снятого счётчика"), DataMember]
        public string Номер_снятого_счетчика
        {
            get;
            set;
        }

        [Display(Order = 9, GroupName = "Снятый счётчик"), DisplayName("Показание снятого"), DataMember]
        public double Показание_снятого
        {
            get;
            set;
        }

        [Display(Order = 10, GroupName = "Снятый счётчик"), DisplayName("Квартал поверки снятого"), DataMember]
        public byte Квартал_поверки_снятого
        {
            get;
            set;
        }

        [Display(Order = 11, GroupName = "Снятый счётчик"), DisplayName("Год поверки снятого"), DataMember]
        public byte Год_поверки_снятого
        {
            get;
            set;
        }

        [Display(Order = 12, GroupName = "Снятый счётчик"), DisplayName("Дата установки снятого"), DataMember]
        public DateTime? Дата_установки_снятого
        {
            get;
            set;
        }

        [Display(Order = 13, GroupName = "Снятый счётчик"), DisplayName("Год выпуска снятого"), DataMember]
        public uint Год_выпуска_снятого
        {
            get;
            set;
        }
        [Display(Order = 14, GroupName = "Установленный счётчик"), DisplayName("Тип установленного счётчика"), DataMember]
        public string Тип_установленного_счетчика
        {
            get;
            set;
        }

        [Display(Order = 15, GroupName = "Установленный счётчик"), DisplayName("Номер установленного счётчика"), DataMember]
        public string Номер_установленного_счетчика
        {
            get;
            set;
        }

        [Display(Order = 16, GroupName = "Установленный счётчик"), DisplayName("Показание установленного"), DataMember]
        public double Показание_установленного
        {
            get;
            set;
        }

        [Display(Order = 17, GroupName = "Установленный счётчик"), DisplayName("Год выпуска установленного"), DataMember]
        public uint Год_выпуска_установленного
        {
            get;
            set;
        }

        [Display(Order = 18), DataMember]
        public string Причина
        {
            get;
            set;
        }

        [Display(Order = 19), DataMember]
        public bool ЭтоЭлектронный
        {
            get;
            set;
        }
    }
}