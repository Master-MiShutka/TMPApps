﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    /// <summary>
    /// Interaction logic for GetDataControl.xaml
    /// </summary>
    public partial class GetDataControl : UserControl
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Dispatcher _callingDispatcher;
        private Action<string> _onCompleted;
        private Action _onCanceled;

        private IList<ListPointWithResult> _list;

        private class ListPointWithResult : ListPoint
        {
            /// <summary>
            /// Processed | Wait
            /// </summary>
            public string Status { get; set; }
            public string Result { get; set; }
            public ListPointWithResult(ListPoint source)
            {
                this.ParentId = source.ParentId;
                this.ParentTypeCode = source.ParentTypeCode;
                this.ParentTypeName = source.ParentTypeName;
                this.Id = source.Id;
                this.Name = source.Name;
                this.IsGroup = source.IsGroup;
                this.TypeCode = source.TypeCode;
                this.EсpName = source.EсpName;
                this.Type = source.Type;
                this.Checked = source.Checked;
            }
        }

        public GetDataControl(IList<ListPoint> list, Action<string> completed, Action canceled = null)
        {
            if (_list == null)
                throw new ArgumentNullException();
            _list = list.Select(i => new ListPointWithResult(i)).ToList();

            _onCompleted = completed;
            _onCanceled = canceled;

            InitializeComponent();

            _callingDispatcher = Dispatcher.CurrentDispatcher;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() => Go())
                .ContinueWith((t) => _onCanceled(), cts.Token, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
        }

        private void Go()
        {
            int itemsCount = _list.Count;
            Action<int> report = (pos) =>
                {
                    UIAction(() => progress.Value = 100d * pos / itemsCount);
                };
            string csv = string.Empty;


            for (int i = 1; i <= itemsCount; i++)
            {
                if (cts.IsCancellationRequested)
                {                   
                    return;
                }
                

                report(i);
            }
            _onCompleted(csv);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (App.ShowQuestion("Прервать операцию?") == MsgBox.MsgBoxResult.Yes)
            {
                cts.Cancel();
            }
        }

        private void UIAction(Action action)
        {
            if (_callingDispatcher.CheckAccess())
                action();
            else
                _callingDispatcher.BeginInvoke(((Action)(() => { action(); })));
        }

        public event LoadCompletedEventHandler LoadCompleted;
        public event CancelEventHandler Canceled;
    }
}

/*

callerUID=INIT_ID_1	instance=_level1.scene.spContentHolder6.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder	RP_PUBLIC=1	__const=TYPE=RP_TYPE	datablock=RP_TYPE	action=GET

RP_TYPE_ID_0=72	RP_TYPE_NAME_0=DWRES Отчеты	TYPE_0=RP_TYPE	RP_PUBLIC_0=1
RP_TYPE_ID_1=42	RP_TYPE_NAME_1=Балансы подстанций	TYPE_1=RP_TYPE	RP_PUBLIC_1=1
RP_TYPE_ID_2=2	RP_TYPE_NAME_2=Волковысские сети	TYPE_2=RP_TYPE	RP_PUBLIC_2=1
RP_TYPE_ID_3=22	RP_TYPE_NAME_3=Выработка	TYPE_3=RP_TYPE	RP_PUBLIC_3=1
RP_TYPE_ID_4=7	RP_TYPE_NAME_4=Отчеты ТЭЦ-2	TYPE_4=RP_TYPE	RP_PUBLIC_4=1
RP_TYPE_ID_5=32	RP_TYPE_NAME_5=Отчётные формы	TYPE_5=RP_TYPE	RP_PUBLIC_5=1
RP_TYPE_ID_6=17	RP_TYPE_NAME_6=Перетоки	TYPE_6=RP_TYPE	RP_PUBLIC_6=1
RP_TYPE_ID_7=67	RP_TYPE_NAME_7=Промышленные предприятия	TYPE_7=RP_TYPE	RP_PUBLIC_7=1
RP_TYPE_ID_8=27	RP_TYPE_NAME_8=Структура дерева	TYPE_8=RP_TYPE	RP_PUBLIC_8=1
RP_TYPE_ID_9=47	RP_TYPE_NAME_9=Тест	TYPE_9=RP_TYPE	RP_PUBLIC_9=1
RP_TYPE_ID_10=37	RP_TYPE_NAME_10=Утвержденные	TYPE_10=RP_TYPE	RP_PUBLIC_10=1	
result=0	recordCount=11
-------------------------------------------------------
callerUID=INIT_ID_1/RP_TYPE_ID_72	instance=_level1.scene.spContentHolder6.PANEL_RP_TREE.spContentHolder0.RP_TREE.spContentHolder	RP_PUBLIC=1	RP_TYPE_ID=72	__const=TYPE=RP	datablock=RP	action=GET

RP_ID_0=679	RP_TYPE_ID_0=72	RP_NAME_0=Выборка точек по группе	RP_PUBLIC_0=1	RP_DESCRIPTION_0=	RPF_ID_0=1	RP_LOG_ENABLED_0=0	USER_NAME_0=System user EMCOS	TYPE_0=RP
RP_ID_1=675	RP_TYPE_ID_1=72	RP_NAME_1=Для расчета балансов ПС до 35кВ	RP_PUBLIC_1=1	RP_DESCRIPTION_1=	RPF_ID_1=2	RP_LOG_ENABLED_1=0	USER_NAME_1=Щелухин Д.Д.	TYPE_1=RP
RP_ID_2=698	RP_TYPE_ID_2=72	RP_NAME_2=Для расчета балансов ПС 35-770кВ	RP_PUBLIC_2=1	RP_DESCRIPTION_2=	RPF_ID_2=2	RP_LOG_ENABLED_2=1	USER_NAME_2=System user EMCOS	TYPE_2=RP	
result=0	recordCount=3
-------------------------------------------------------

-------------------------------------------------------
-------------------------------------------------------
******* 
__invoker=undefined RP_ID=679   RP_NAME=Выборка точек по группе errorDispatch=false dataBlock=PARAMETERS    action=GET

RPQ_ID_0=1288   RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5404	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2	result=0	recordCount=1
&RPQ_ID_0=1288&RPQ_NAME_0=Запрос 1&TABLE_NAME_0=&RPQF_ID_0=5404&COLUMN_NAME_0=GR_ID&RPQO_ID_0=1&RPQO_CODE_0==&RPQF_VALUE_0=&FIELD_DESC_0=&RPQF_ALLOW_EMPTY_VALUES_0=0&DATA_TYPE_0=VARCHAR2&result=0&recordCount=1
*******
refresh=true    instance=_level1.scene.spContentHolder6.POINTS.POINTMENU.APTTabs.spContentHolder0.APT.spContentHolder   TYPE=GROUP  action=expand   ID=1

GR_ID_0=7257	GRC_NR_0=	GRC_DESC_0=	GR_CODE_0=Расчетные точки	GR_NAME_0=Расчетные точки	USER_NAME_0=System user EMCOS	IS_PUBLIC_0=1	IS_PUBLIC_TXT_0=Да	GR_TYPE_ID_0=52	GR_TYPE_NAME_0=Расчетные точки	GR_TYPE_CODE_0=CalcPoint	PARENT_0=0	HASCHILDS_0=1	GRC_BT_0=	TYPE_0=GROUP
GR_ID_1=52	GRC_NR_1=	GRC_DESC_1=	GR_CODE_1=1	GR_NAME_1=Белэнерго	USER_NAME_1=System user EMCOS	IS_PUBLIC_1=1	IS_PUBLIC_TXT_1=Да	GR_TYPE_ID_1=7	GR_TYPE_NAME_1=Белэнерго	GR_TYPE_CODE_1=CONCERN	PARENT_1=0	HASCHILDS_1=1	GRC_BT_1=	TYPE_1=GROUP	result=0	recordCount=2
*******
__invoker=undefined	RP_ID=679	RP_NAME=Выборка точек по группе     errorDispatch=false     dataBlock=PREPARED_RP	action=GET

&result=0   recordCount=0
-------------------------------------------------------
-------------------------------------------------------
*******
__invoker=undefined	RP_ID=675	RP_NAME=Для расчета балансов ПС до 35кВ	errorDispatch=false	dataBlock=PARAMETERS	action=GET

RPQ_ID_0=1283	RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5409	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2
RPQ_ID_1=1283	RPQ_NAME_1=Запрос 1	TABLE_NAME_1=	RPQF_ID_1=5410	COLUMN_NAME_1=GR_ID	RPQO_ID_1=1	RPQO_CODE_1==	RPQF_VALUE_1=	FIELD_DESC_1=	RPQF_ALLOW_EMPTY_VALUES_1=0	DATA_TYPE_1=VARCHAR2
RPQ_ID_2=1283	RPQ_NAME_2=Запрос 1	TABLE_NAME_2=	RPQF_ID_2=5411	COLUMN_NAME_2=GR_ID	RPQO_ID_2=1	RPQO_CODE_2==	RPQF_VALUE_2=	FIELD_DESC_2=	RPQF_ALLOW_EMPTY_VALUES_2=0	DATA_TYPE_2=VARCHAR2
RPQ_ID_3=1283	RPQ_NAME_3=Запрос 1	TABLE_NAME_3=	RPQF_ID_3=5412	COLUMN_NAME_3=GR_ID	RPQO_ID_3=1	RPQO_CODE_3==	RPQF_VALUE_3=	FIELD_DESC_3=	RPQF_ALLOW_EMPTY_VALUES_3=0	DATA_TYPE_3=VARCHAR2
result=0	recordCount=4
*******
__invoker=undefined	RP_ID=675	RP_NAME=Для расчета балансов ПС до 35кВ	errorDispatch=false	dataBlock=PREPARED_RP	action=GET

&result=0&recordCount=0
-------------------------------------------------------
-------------------------------------------------------
*******
__invoker=undefined	RP_ID=698	RP_NAME=Для расчета балансов ПС 35-770кВ	errorDispatch=false	dataBlock=PARAMETERS	action=GET

RPQ_ID_0=1307	RPQ_NAME_0=Запрос 1	TABLE_NAME_0=	RPQF_ID_0=5534	COLUMN_NAME_0=GR_ID	RPQO_ID_0=1	RPQO_CODE_0==	RPQF_VALUE_0=	FIELD_DESC_0=	RPQF_ALLOW_EMPTY_VALUES_0=0	DATA_TYPE_0=VARCHAR2
RPQ_ID_1=1307	RPQ_NAME_1=Запрос 1	TABLE_NAME_1=	RPQF_ID_1=5535	COLUMN_NAME_1=GR_ID	RPQO_ID_1=1	RPQO_CODE_1==	RPQF_VALUE_1=	FIELD_DESC_1=	RPQF_ALLOW_EMPTY_VALUES_1=0	DATA_TYPE_1=VARCHAR2
RPQ_ID_2=1307	RPQ_NAME_2=Запрос 1	TABLE_NAME_2=	RPQF_ID_2=5536	COLUMN_NAME_2=GR_ID	RPQO_ID_2=1	RPQO_CODE_2==	RPQF_VALUE_2=	FIELD_DESC_2=	RPQF_ALLOW_EMPTY_VALUES_2=0	DATA_TYPE_2=VARCHAR2
RPQ_ID_3=1307	RPQ_NAME_3=Запрос 1	TABLE_NAME_3=	RPQF_ID_3=5537	COLUMN_NAME_3=GR_ID	RPQO_ID_3=1	RPQO_CODE_3==	RPQF_VALUE_3=	FIELD_DESC_3=	RPQF_ALLOW_EMPTY_VALUES_3=0	DATA_TYPE_3=VARCHAR2
result=0	recordCount=4
-------------------------------------------------------
-------------------------------------------------------
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp
1807 - Клиденята (GR_ID_14=1807&GRC_NR_14=&GRC_DESC_14=&GR_CODE_14=143688.Клиденяты&GR_NAME_14=ПС 35кВ Клиденяты)
RPQF_5404=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5404	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1288	RPQF_COUNT_0=1	
RPQ_COUNT=1	export_to_html=0	RP_ID=679	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Выборка точек по группе_2016_12_30_112843_1.xls">
</HEAD>
</HTML>

*******
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp 
RPQF_5412=1807	DATA_TYPE_0_3=VARCHAR2	RPQF_VALUE_0_3=1807	COLUMN_NAME_0_3=GR_ID	RPQO_ID_0_3=1	RPQF_ID_0_3=5412	RPQF_5411=1807	DATA_TYPE_0_2=VARCHAR2	RPQF_VALUE_0_2=1807	COLUMN_NAME_0_2=GR_ID	RPQO_ID_0_2=1	RPQF_ID_0_2=5411	RPQF_5410=1807	DATA_TYPE_0_1=VARCHAR2	RPQF_VALUE_0_1=1807	COLUMN_NAME_0_1=GR_ID	RPQO_ID_0_1=1	RPQF_ID_0_1=5410	RPQF_5409=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5409	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1283	RPQF_COUNT_0=4	RPQ_COUNT=1	export_to_html=0	RP_ID=675	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Для расчета балансов ПС до 35кВ_2016_12_30_113404_1.txt">
</HEAD>
</HTML>

4082;10,4;0;0;0;0;0;0
4087;10,4;16,82619525;4,83579369;146,400047;0;22,560049;0
4092;10,4;8,85270864;4,02588858;27,480012;0;6,360012;0
4093;10,4;7,89662588;,6989603;134,640031;0;21,240035;0

*******
POST http://10.96.18.16/emcos/scripts/ReportGenerator.asp
RPQF_5537=1807	DATA_TYPE_0_3=VARCHAR2	RPQF_VALUE_0_3=1807	COLUMN_NAME_0_3=GR_ID	RPQO_ID_0_3=1	RPQF_ID_0_3=5537	RPQF_5536=1807	DATA_TYPE_0_2=VARCHAR2	RPQF_VALUE_0_2=1807	COLUMN_NAME_0_2=GR_ID	RPQO_ID_0_2=1	RPQF_ID_0_2=5536	RPQF_5535=1807	DATA_TYPE_0_1=VARCHAR2	RPQF_VALUE_0_1=1807	COLUMN_NAME_0_1=GR_ID	RPQO_ID_0_1=1	RPQF_ID_0_1=5535	RPQF_5534=1807	DATA_TYPE_0_0=VARCHAR2	RPQF_VALUE_0_0=1807	COLUMN_NAME_0_0=GR_ID	RPQO_ID_0_0=1	RPQF_ID_0_0=5534	RPQ_NAME_0=Запрос 1	RPQ_ID_0=1307	RPQF_COUNT_0=4	RPQ_COUNT=1	export_to_html=0	RP_ID=698	action=SHOW_REPORT

<HTML>
<HEAD>
<META HTTP-EQUIV=Refresh CONTENT="0;URL=../\\reports\2016\12\30\Для расчета балансов ПС 35-770кВ_2016_12_30_121702_1.txt">
</HEAD>
</HTML>

0;0;0;0;0;0;0;0;0;0;0;0
4087;720;0;6,24;16,82619525;4,94451456;0;,10872086;,14640004;0;,02256004;0
4093;0;720;6,24;0;,26622548;7,89662588;,96518579;,13464003;0;,02124003;0
4092;0;720;6,24;0;,00240005;8,85270864;4,02828864;,02748001;0;,00636001;0

Код АСКУЭ;Тнаг;Тген;Uср.э.;Wpн;Wqн;Wpг;Wqг;Pмакс;Pмин;Qмакс;Qмин


     
*/
