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
        [DataMember]
        [DisplayName("Дата замены")]
        [Display(Order = 0, GroupName = "")]
        public DateTime? Дата_замены { get; set; }
        [DataMember]
        [DisplayName("Номер акта")]
        [Display(Order = 1, GroupName = "")]
        public string Номер_акта { get; set; }
        [DataMember]
        [DisplayName("Ф.И.О. монтёра")]
        [Display(Order = 2, GroupName = "")]
        public string Фамилия { get; set; }

        [DataMember]
        [DisplayName("Лицевой счёт абонента")]
        [Display(Order = 3, GroupName = "Абонент")]
        public ulong Лицевой { get; set; }
        [DataMember]
        [DisplayName("Ф.И.О.")]
        [Display(Order = 4, GroupName = "Абонент")]
        public string ФИО { get; set; }
        [DataMember]
        [DisplayName("Населённый пункт")]
        [Display(Order = 5, GroupName = "Абонент")]
        public string Населённый_пункт { get; set; }
        [DataMember]
        [DisplayName("Тип снятого счётчика")]
        [Display(Order = 6, GroupName = "Снятый счётчик")]
        public string Тип_снятого_счетчика { get; set; }
        [DataMember]
        [DisplayName("Номер снятого счётчика")]
        [Display(Order = 7, GroupName = "Снятый счётчик")]
        public string Номер_снятого_счетчика { get; set; }
        [DataMember]
        [DisplayName("Показание снятого")]
        [Display(Order = 8, GroupName = "Снятый счётчик")]
        public double Показание_снятого { get; set; }
        [DataMember]
        [DisplayName("Квартал поверки снятого")]
        [Display(Order = 9, GroupName = "Снятый счётчик")]
        public byte Квартал_поверки_снятого { get; set; }
        [DataMember]
        [DisplayName("Год поверки снятого")]
        [Display(Order = 10, GroupName = "Снятый счётчик")]
        public byte Год_поверки_снятого { get; set; }
        [DataMember]
        [DisplayName("Дата установки снятого")]
        [Display(Order = 11, GroupName = "Снятый счётчик")]
        public DateTime? Дата_установки_снятого { get; set; }
        [DataMember]
        [DisplayName("Год выпуска снятого")]
        [Display(Order = 12, GroupName = "Снятый счётчик")]
        public uint Год_выпуска_снятого { get; set; }

        [DataMember]
        [DisplayName("Номер установленного счётчика")]
        [Display(Order = 13, GroupName = "Установленный счётчик")]
        public string Номер_установленного_счетчика { get; set; }
        [DataMember]
        [DisplayName("Показание установленного")]
        [Display(Order = 14, GroupName = "Установленный счётчик")]
        public double Показание_установленного { get; set; }

        [DataMember]
        [Display(Order = 15)]
        public string Причина { get; set; }
    }
}