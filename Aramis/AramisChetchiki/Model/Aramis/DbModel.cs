namespace TMP.WORK.AramisChetchiki
{
    using System;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

    /// <summary>
    /// справочник установленных счётчиков
    /// </summary>
    public record KARTSCH(string LIC_SCH,
                          string N_SCH,
                          string COD_TSCH,
                          string G_PROV,
                          DateTime? DUSTAN,
                          int? DATA_OLD,
                          int? DATA_NEW,
                          string N_PLOMB,
                          int? GODVYPUSKA,
                          decimal? POWERS,
                          string COD_SS,
                          DateTime? DATE_FAZ,
                          string PLOMB_GS,
                          string MATER,
                          int? PUSTAN,
                          int? DATA_UST,
                          DateTime? DATE_UST
                          );

    /// <summary>
    /// справочник типов счетчиков
    /// </summary>
    public record KARTTSCH(string COD_TSCH, string NAME, string TOK, string PERIOD, string TIP, int ФАЗ, decimal ЗНАК);

    /// <summary>
    /// справочник мест установки
    /// </summary>
    public record ASVIDYST(string COD_SS, string MESTO);

    /// <summary>
    /// справочник населенных пунктов
    /// </summary>
    public record KartTn(string COD_TN, string TOWN, string COD_SS);

    /// <summary>
    /// справочник сельских советов
    /// </summary>
    public record KartSs(string COD_SS, string СЕЛЬСОВЕТ);

    /// <summary>
    /// справочник улиц
    /// </summary>
    public record KartSt(string COD_ST, string STREET);

    /// <summary>
    /// справочник токоприемников
    /// </summary>
    public record KartTpr(string COD_TPR, string TPRIEM);

    /// <summary>
    /// справочник использования
    /// </summary>
    public record KartIsp(string COD_ISP, string ISPIEM);

    /// <summary>
    /// справочник категорий
    /// </summary>
    public record KartKat(string COD_KAT, string KATEGAB);

    /// <summary>
    /// подстанции
    /// </summary>
    public record Kartps(int ПОДСТАНЦИЯ, int? РЭС, string НАИМЕНОВ);

    /// <summary>
    /// фидера 10 кВ
    /// </summary>
    public record Kartfid(int ПОДСТАНЦИЯ, string ФИДЕР, string НАИМЕНОВ, string НАИМ_ПОД);

    /// <summary>
    /// пс 10 кВ
    /// </summary>
    public record Kartktp(int КОД_ТП, string ФИДЕР, int? НОМЕР_ТП, string НАИМ_ТП, int? ПОДСТАНЦИЯ, int? РЭС, string НАИМЕНОВ, string PR_GS);

    /// <summary>
    /// контролёры
    /// </summary>
    public record ASKONTR(string КОД_КОН, string ФАМИЛИЯ);

    /// <summary>
    /// справочник абонентов
    /// </summary>
    public class KARTAB
    {
        public bool IsDeleted { get; set; }

        public string LIC_SCH { get; set; }

        public string FAM { get; set; }

        public string NAME { get; set; }

        public string OTCH { get; set; }

        public string SMS { get; set; }

        public bool RABOT { get; set; }

        public string COD_TN { get; set; }

        public string COD_ST { get; set; }

        public string HOME { get; set; }

        public string KV { get; set; }

        public int? ЧЛЕНОВ { get; set; }

        public string TELEF { get; set; }

        public string COD_KAT { get; set; }

        public string COD_TPR { get; set; }

        public string ШИФР { get; set; }

        public string COD_PD { get; set; }

        public decimal? DKONT { get; set; }

        public string KOMENT { get; set; }

        public bool PLIT { get; set; }

        public DateTime? YEARMON { get; set; }

        public decimal? ERRSUM { get; set; }

        public decimal? ERRSUMN { get; set; }

        public decimal? SUMMA_KN { get; set; }

        public decimal? SUMMA_KC { get; set; }

        public string КОД_КОН { get; set; }

        public int? СРЕДНЕЕ { get; set; }

        public int? МЕСЯЦ { get; set; }

        public DateTime? ДАТА_ОТКПЛ { get; set; }

        public DateTime? ДАТА_ОТКФК { get; set; }

        public string НОМЕР_ТП { get; set; }

        public int? ФИДЕР { get; set; }

        public string НОМЕР_ОПОР { get; set; }

        public string СМЕНА { get; set; }

        public int? DATA_KON { get; set; }

        public DateTime? DATE_KON { get; set; }

        public string ФАМИЛИЯ { get; set; }

        public string PR_OPL { get; set; }

        public string COD_PRED { get; set; }

        public DateTime? DATE_ZAP { get; set; }

        public string COD_VID { get; set; }

        public int? ПОКАЗАНИЯ { get; set; }

        public DateTime? DATE_LGT { get; set; }

        public int? ЛЬГОТА { get; set; }

        public int? ПРОЦЕНТ { get; set; }

        public bool PR_VN { get; set; }

        public bool PR_VO { get; set; }

        public bool PR_MO { get; set; }

        public bool PR_ZD { get; set; }

        public DateTime? DATE_R { get; set; }

        public int? RACHPOK { get; set; }

        public decimal? PENYA_T { get; set; }

        public string COD_ISP { get; set; }

        public string DOG { get; set; }

        public DateTime? DATE_DOG { get; set; }

        public string FIDER10 { get; set; }

        public bool ASKUE { get; set; }

        public decimal? ERRSUMV { get; set; }

        public int? KVT_LGT { get; set; }

        public string PRIZNAK { get; set; }

        public string GKH_L { get; set; }

        public string GKH_L1 { get; set; }
    }

    /// <summary>
    /// замены счётчиков
    /// </summary>
    public record Assmena(string ЛИЦ_СЧЕТ, string ТИП_СЧЕТЧ, string НОМЕР_СНЯТ, int ПОКАЗ_СНЯТ, string НОМЕР_УСТ, int ПОКАЗ_УСТ, DateTime? ДАТА_ЗАМЕН, int? НОМЕР_АКТА, string ФАМИЛИЯ, string ПРИЧИНА);

    /// <summary>
    /// наименование РЭС
    /// </summary>
    public record KartPd(string COD_PD, string SNPD, string PNPD);

    /// <summary>
    /// Оплаты
    /// </summary>
    public record KartKvGd(string LIC_SCH, string PACHKA, DateTime? YEARMON, int? DATA_OLD, int? DATA_NEW, int? РАЗН_КН, decimal? TAR_KN, decimal? SUMMA_KN);

    /// <summary>
    /// Удаленные абоненты
    /// </summary>
    public record RemovAb(ulong LIC_SCH, string FAM, string NAME, string OTCH, DateOnly DATE_ZAP);

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
}
