using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Linq;
using System.Linq.Expressions;

using Model = TMP.DWRES.Objects;

namespace TMP.DWRES.DB
{
    public class dWRESdbHelper : IDisposable
    {
        //переменные, нужные в контексте всей программы
        private FbConnection fbConnection; //fb ссылается на соединение с нашей базой данных, по-этому она должна быть доступна всем методам нашего класса

        private FbDatabaseInfo fbDBInfo; //информация о БД

        public dWRESdbHelper()
        {
            //string cs = ConfigurationManager.ConnectionStrings["dWRESModel"].ConnectionString;
            HasError = false;
            LastException = null;
        }

        public void Dispose()
        {
            if (fbConnection == null) return;
            if ((fbConnection.State == System.Data.ConnectionState.Connecting) ||
                (fbConnection.State == System.Data.ConnectionState.Executing) ||
                (fbConnection.State == System.Data.ConnectionState.Fetching) ||
                (fbConnection.State == System.Data.ConnectionState.Open))
            {
                fbConnection.Close();
            }
        }
        private string getConnectionString(string dbFileName, bool connectToServer = false, DBConnectionParams dbConnectionParams = null)
        {
            //формируем connection string для последующего соединения с нашей базой данных
            FbConnectionStringBuilder fbConnStringBuilder = new FbConnectionStringBuilder();
            fbConnStringBuilder.Charset = FbCharset.None.ToString(); //используемая кодировка
            fbConnStringBuilder.UserID = "SYSDBA"; //логин
            fbConnStringBuilder.Password = "masterkey"; //пароль
            fbConnStringBuilder.Pooling = true;

            if (connectToServer)
            {
                fbConnStringBuilder.DataSource = dbConnectionParams.DataSource; // сервер
                fbConnStringBuilder.Database = dbConnectionParams.Database; //путь к файлу базы данных
                fbConnStringBuilder.ServerType = FbServerType.Default; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))
            }
            else
            {
                fbConnStringBuilder.Database = dbFileName; //путь к файлу базы данных
                fbConnStringBuilder.ServerType = FbServerType.Embedded; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))
            }

            return fbConnStringBuilder.ToString();
        }

        private void Init()
        {
            HasError = false;
            try
            {
                try
                {
                    fbConnection.Open(); //открываем БД
                    fbDBInfo = new FbDatabaseInfo(fbConnection);

                    // Get the list of User Tables
                    // Restrictions:
                    // TABLE_CATALOG
                    // TABLE_SCHEMA
                    // TABLE_NAME
                    // TABLE_TYPE
                    System.Data.DataTable userTables = fbConnection.GetSchema("Tables", new string[] { null, null, null, "TABLE" });

                    // Get the list of System Tables
                    // Restrictions:
                    // TABLE_CATALOG
                    // TABLE_SCHEMA
                    // TABLE_NAME
                    // TABLE_TYPE
                    System.Data.DataTable systemTables = fbConnection.GetSchema("Tables", new string[] { null, null, null, "SYSTEM TABLE" });
                }
                finally
                {
                    fbConnection.Close();
                }                
            }
            catch (DllNotFoundException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Не удалось создать подключение к базе данных.\n");
                sb.Append("В папке с программой не найдены необходимые библиотеки\n");
                sb.Append("fbembed.dll, ib_util.dll, icudt30.dll, icuin30.dll, icuuc30.dll\n");
                sb.Append("Переустановите программу или обратитесь к разработчику.");

                HasError = true;
                LastException = ex;

                throw new InvalidOperationException(sb.ToString());
            }
            catch (FbException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Не удалось создать подключение к базе данных.\n");

                switch (ex.ErrorCode)
                {
                    case 335544344:
                        sb.Append("Файл базы данных используется другой программой.\n");
                        break;
                    case 335544346:
                        sb.Append("База данных повреждена.\n");
                        break;
                    case 335544415:
                        sb.Append("База данных повреждена.\n");
                        break;
                    case 335544472:
                        sb.Append("У Вас нет доступа к базе данных, проверьте параметры подключения.\n");
                        break;
                    case 335544352:
                        sb.Append("У Вас отсутствуют разрешения, проверьте параметры подключения.\n");
                        break;
                    default:
                        sb.Append("\tкод ошибки: ");
                        sb.Append(ex.ErrorCode.ToString());
                        sb.Append("\n\tописание: ");
                        sb.Append(ex.Message);
                        break;
                }

                HasError = true;
                LastException = ex;

                throw new InvalidOperationException(sb.ToString());
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Не удалось создать подключение к базе данных.\n");
                sb.Append("Произошла неизвестная ошибка.\n");
                sb.Append("\n\tописание: ");
                sb.Append(ex.Message);


                HasError = true;
                LastException = ex;

                throw new InvalidOperationException(sb.ToString());
            }
        }
        /// <summary>
        /// Инициализация: создание подключения к указанной базе данных
        /// </summary>
        public void Init(DBConnectionParams dbConnectionParams)
        {
            //создаем подключение
            fbConnection = new FbConnection(getConnectionString(string.Empty, true, dbConnectionParams));
            //
            Init();
        }
        /// <summary>
        /// Инициализация: создание подключения к указанной базе данных
        /// </summary>
        public void Init(string fileName)
        {
            //создаем подключение
            fbConnection = new FbConnection(getConnectionString(fileName));
            //
            Init();
        }

        #region Get DWRES Tables

        public ICollection<Model.EnergoSystem> GetEnergoSystems()
        {
            using (var command = new FbCommand("SELECT * FROM ENERGOSYS", fbConnection))
            {
                ICollection<Model.EnergoSystem> energoSystems = new List<Model.EnergoSystem>();

                try
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }

                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                Model.EnergoSystem energosystem = new Model.EnergoSystem(reader);
                                energosystem.Filials = this.GetFilials(energosystem.ID);
                                energoSystems.Add(energosystem);
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    return energoSystems;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает список филиалов энергосистемы
        /// </summary>
        /// <param name="energoSystemId">ИД энергосистемы</param>
        /// <returns>Список филиалов</returns>
        public ICollection<Model.Filial> GetFilials(int energoSystemId)
        {
            using (var command = new FbCommand(String.Format("SELECT * FROM FILIAL WHERE ID_ENERGOSYS={0} ORDER BY NAME", energoSystemId), fbConnection))
            {
                ICollection<Model.Filial> filials = new List<Model.Filial>();

                try
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                Model.Filial filial = new Model.Filial(reader);
                                filial.Reses = this.GetReses(filial.ID);
                                filials.Add(filial);
                            }
                        }
                        finally { reader.Close(); }
                    return filials;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Список РЭС филиала
        /// </summary>
        /// <param name="filialId">ИД филиала</param>
        /// <returns>Список РЭС</returns>
        public ICollection<Model.Res> GetReses(int filialId)
        {
            using (var command = new FbCommand(String.Format("SELECT * FROM RES WHERE ID_FILIAL={0} ORDER BY NAME", filialId), fbConnection))
            {
                ICollection<Model.Res> reses = new List<Model.Res>();

                try
                { 
                if (fbConnection.State == System.Data.ConnectionState.Closed)
                {
                    fbConnection.Open();
                }
                FbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                    try
                    {
                        while (reader.Read())
                        {
                            Model.Res res = new Model.Res(reader);
                            res.Substations = this.GetSubstations(res.ID);
                            reses.Add(res);
                        }
                    }
                    finally { reader.Close(); }
                return reses;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает список подстанций РЭС
        /// </summary>
        /// <param name="resId">ИД РЭС</param>
        /// <returns>Список подстанций РЭС</returns>
        public ICollection<Model.Substation> GetSubstations(int resId)
        {
            using (var command = new FbCommand(String.Format("SELECT * FROM PST WHERE ID_RES={0} ORDER BY NAME", resId), fbConnection))
            {
                ICollection<Model.Substation> psts = new List<Model.Substation>();
                try
                { 
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                Model.Substation pst = new Model.Substation(reader);
                                pst.Fiders = this.GetFiders(pst.ID);
                                psts.Add(pst);
                            }
                        }
                        finally { reader.Close(); }
                    return psts;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает список фидеров, относящихся к подстанции
        /// </summary>
        /// <param name="substationId">ИД подстанции</param>
        /// <returns>Список фидеров подстанции</returns>
        public ICollection<Model.Fider> GetFiders(int substationId)
        {
            using (var command = new FbCommand(String.Format("SELECT * FROM FIDER WHERE ID_PST={0}", substationId), fbConnection))
            {
                List<Model.Fider> fiders = new List<Model.Fider>();
                try
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                fiders.Add(new Model.Fider(reader));
                            }
                        }
                        finally { reader.Close(); }
                    fiders.Sort(new FiderNameComparer());
                    return fiders;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает по ИД фидера список его участков
        /// </summary>
        /// <param name="fiderId">ИД фидера</param>
        /// <returns>Список участков фидера</returns>
        public ICollection<Model.Line> GetLines(int fiderId)
        {
            using (var command = new FbCommand(String.Format("SELECT ID, ID_NODE1, ID_NODE2, KA1_STATE_CUR, KA2_STATE_CUR, SCHEM, PRZ_AB FROM LINE WHERE ID_FIDER={0} ORDER BY ID", fiderId), fbConnection))
            {
                ICollection<Model.Line> lines = new List<Model.Line>();
                try
                { 
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                                lines.Add(new Model.Line(reader));
                        }
                        finally { reader.Close(); }
                    return lines;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает по ИД фидера список вершин его участков
        /// </summary>
        /// <param name="fiderId">ИД фидера</param>
        /// <returns>Список вершин участов фидера</returns>
        public ICollection<Model.LineVertex> GetLinesVertities(int fiderId)
        {
            using (var command = new FbCommand(
                String.Format("SELECT ID_NODE1 ID FROM LINE WHERE ID_FIDER={0} UNION SELECT ID_NODE2 ID FROM LINE WHERE ID_FIDER={0}", fiderId), fbConnection))
            {
                ICollection<Model.LineVertex> linesVertities = new List<Model.LineVertex>();
                try
                { 
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                linesVertities.Add(new Model.LineVertex(reader));
                            }
                        }
                        finally { reader.Close(); }
                    return linesVertities;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return null;
                }
            }
        }


        internal void FillVertexProperties(int nodeId,
                                           ref Dictionary<int, string> dictionaryNodeTPNames,
                                           ref Dictionary<int, string> dictionaryNodeNames,
                                           ref Dictionary<int, string> dictionaryNodePSTName,
                                           ref Dictionary<int, Graph.FiderGraphVertex> vertities)
        {
            // если узел не указан
            if (nodeId == default(int))
            {
                vertities[nodeId].Name = "<нет>";
                vertities[nodeId].Type = Graph.GraphVertexType.unknown;
            }
            else
            // если это текущий узел это секция подстанции (ЦП)
            if (dictionaryNodePSTName.ContainsKey(nodeId))
            {
                vertities[nodeId].Name = dictionaryNodePSTName[nodeId].Trim();
                vertities[nodeId].Type = Graph.GraphVertexType.Supply;
            }
            else
                // если это текущий узел это просто узел сети
                if (dictionaryNodeNames.ContainsKey(nodeId))
                {
                    vertities[nodeId].Name = dictionaryNodeNames[nodeId].Trim();
                    vertities[nodeId].Type = Graph.GraphVertexType.Node;
                }
                else
                    // если это текущий узел это трансформатор
                    if (dictionaryNodeTPNames.ContainsKey(nodeId))
                    {
                        vertities[nodeId].Name = dictionaryNodeTPNames[nodeId].Trim();
                        vertities[nodeId].Type = Graph.GraphVertexType.Transformer;
                    }
                    else
                    // почему-то не нашли
                    {
                        vertities[nodeId].Name = "<нет>";
                        vertities[nodeId].Type = Graph.GraphVertexType.unknown;
                    }
        }

        public struct FiderScheme
        {
            public ICollection<Model.Line> Lines;
            public ICollection<Graph.FiderGraphVertex> GraphVertities;
            public ICollection<Graph.FiderGraphEdge> GraphEdges;
        }

        /// <summary>
        /// Возвращает список участков схемы фидера
        /// </summary>
        /// <param name="fider">Фидер</param>
        /// <returns>Список участков схемы, список вершин графа схемы</returns>
        public FiderScheme GetFiderLines(Model.Fider fider)
        {
            FiderScheme result = new FiderScheme();

            string query;

            int id_fider = fider.ID;
            int id_sect_nn = fider.ID_Sect_NN;

            // словарь вершин ИД вершин схемы фидера
            ICollection<Model.LineVertex> linesVertities = GetLinesVertities(id_fider);
            // список вершин графа схемы фидера
            List<Graph.FiderGraphVertex> vertitiesList = new List<Graph.FiderGraphVertex>(linesVertities.Count);
            // словарь вершин графа схемы фидера
            Dictionary<int, Graph.FiderGraphVertex> vertities = new Dictionary<int, Graph.FiderGraphVertex>(linesVertities.Count);

            // список ребер графа схемы фидера
            List<Graph.FiderGraphEdge> edges = new List<Graph.FiderGraphEdge>();

            // заполняем словарь вершин
            foreach (Model.LineVertex item in linesVertities)
            {
                Graph.FiderGraphVertex vertex = new Graph.FiderGraphVertex(item.ID);
                vertitiesList.Add(vertex);
                vertities.Add(item.ID, vertex);
            }

            // список ребер графа схемы фидера

            // список участков схемы фидера (узел задан ИД и названием)
            List<Model.Line> linesWithNodeNameList = new List<Model.Line>();

            #region Таблица для поиска названия узла ТП по его ИД

            Dictionary<int, string> dictionaryNodeTPNames = new Dictionary<int, string>();
            query = String.Format(
                    @"SELECT NODELINKS.ID, CAST((TP.NAME||'/'||SECTTP.NAME) AS CHAR(100)) NODE_NAME
                      FROM TP
                      INNER JOIN SECTTP
                      ON (SECTTP.ID_PST = TP.ID)
                      INNER JOIN NODELINKS
                      ON (SECTTP.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=1", id_fider);
            try
            { 
                using (var command = new FbCommand(query, fbConnection))
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                dictionaryNodeTPNames.Add(key, value);
                            }
                        }
                        finally { reader.Close(); }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                LastException = ex;
                return result;
            }

            #endregion Таблица для поиска названия узла ТП по его ИД

            #region Таблица для поиска названия узла по его ИД

            Dictionary<int, string> dictionaryNodeNames = new Dictionary<int, string>();
            query = String.Format(
                    @"SELECT NODELINKS.ID, NODE.NAME NODE_NAME
                      FROM NODE
                      INNER JOIN NODELINKS
                      ON (NODE.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=2 AND NODELINKS.ID IN
                      (
                        SELECT LINE.ID_NODE1 NODEID FROM LINE
                        WHERE LINE.ID_FIDER = {0}
                        UNION
                        SELECT LINE.ID_NODE2 NODEID FROM LINE
                        WHERE LINE.ID_FIDER = {0}
                      )", id_fider);
            try
            { 
                using (var command = new FbCommand(query, fbConnection))
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                if (dictionaryNodeNames.ContainsKey(key) == false)
                                    dictionaryNodeNames.Add(key, value);
                            }
                        }
                        finally { reader.Close(); }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                LastException = ex;
                return result;
            }
            #endregion Таблица для поиска названия узла по его ИД

            #region Таблица для поиска названия узла подстанции по его ИД

            Dictionary<int, string> dictionaryNodePSTName = new Dictionary<int, string>();
            query = String.Format(
                    @"SELECT NODELINKS.ID, CAST(('[ЦП] - '||PST.NAME||'/'||SECTPST.NAME) AS CHAR(100)) NODE_NAME
                      FROM PST
                      INNER JOIN SECTPST
                      ON  (SECTPST.ID_PST = PST.ID)
                      INNER JOIN NODELINKS
                      ON  (SECTPST.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=3 AND SECTPST.ID = {0}", id_sect_nn);
            try
            { 
                using (var command = new FbCommand(query, fbConnection))
                {
                    if (fbConnection.State == System.Data.ConnectionState.Closed)
                    {
                        fbConnection.Open();
                    }
                    FbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                        try
                        {
                            while (reader.Read())
                            {
                                int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                dictionaryNodePSTName.Add(key, value);
                            }
                        }
                        finally { reader.Close(); }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                LastException = ex;
                return result;
            }
            #endregion Таблица для поиска названия узла подстанции по его ИД

            // список участков схемы фидера (узел задан ИД)
            ICollection<Model.Line> lineNodesList = GetLines(id_fider);
            // цикл по всем участкам схемы фидера
            int edgeIndex = 1;
            foreach (var item in lineNodesList)
            {
                FillVertexProperties(item.NodeStartId, ref dictionaryNodeTPNames, ref dictionaryNodeNames, ref dictionaryNodePSTName, ref vertities);
                FillVertexProperties(item.NodeEndId, ref dictionaryNodeTPNames, ref dictionaryNodeNames, ref dictionaryNodePSTName, ref vertities);

                item.NodeStart = vertities[item.NodeStartId].Name;
                item.NodeEnd = vertities[item.NodeEndId].Name;

                edges.Add(new Graph.FiderGraphEdge(edgeIndex, vertities[item.NodeStartId], vertities[item.NodeEndId]));
                edgeIndex++;
            }

            result.Lines = lineNodesList;
            result.GraphVertities = vertitiesList;
            result.GraphEdges = edges;

            return result;
        }

        /// <summary>
        /// Возвращает список участков схемы фидеров всей подстанции
        /// </summary>
        /// <param name="substation">Подстанция</param>
        /// <returns>Список участков схемы, список вершин графа схемы</returns>
        public FiderScheme GetSubstationLines(Model.Substation substation)
        {
            FiderScheme result = new FiderScheme();

            string query;

            int id_substation = substation.ID;

            // словарь вершин ИД вершин
            List<Model.LineVertex> substationFidersLinesVertities = new List<Model.LineVertex>();
            // список вершин графа
            List<Graph.FiderGraphVertex> substationFidersVertitiesList = new List<Graph.FiderGraphVertex>();
            // словарь вершин графа
            Dictionary<int, Graph.FiderGraphVertex> substationFidersVertities = new Dictionary<int, Graph.FiderGraphVertex>();
            // список ребер графа
            List<Graph.FiderGraphEdge> substationFidersEdges = new List<Graph.FiderGraphEdge>();

            // общий список участков
            List<Model.Line> substationFidersLineNodesList = new List<Model.Line>();




            // счётчик ИД ребер графа
            int edgeIndex = 1;

            // для каждого фидера подстанции
            foreach (TMP.DWRES.Objects.Fider fider in substation.Fiders)
            {
                // получение списка вершин
                ICollection<Model.LineVertex> currentFiderLinesVertities = GetLinesVertities(fider.ID);

                List<Graph.FiderGraphVertex> currentFiderVertitiesList = new List<Graph.FiderGraphVertex>(currentFiderLinesVertities.Count);

                // заполняем словарь вершин
                foreach (Model.LineVertex item in currentFiderLinesVertities)
                {
                    // создание вершины графа
                    Graph.FiderGraphVertex vertex = new Graph.FiderGraphVertex(item.ID);
                    currentFiderVertitiesList.Add(vertex);
                    // словарь вершин графа с ключём - кодом вершины
                    if (substationFidersVertities.ContainsKey(item.ID) == false)
                        substationFidersVertities.Add(item.ID, vertex);
                }

                // вершины графа трёх типов: центр питания (ЦП) или узел подстанции, просто узел и узел, представляющий подстанцию
                // полученные выше списки не содержат наименования узла, для его получения создаём следующие таблицы

                #region Таблица для поиска названия узла ТП по его ИД

                Dictionary<int, string> dictionaryNodeTPNames = new Dictionary<int, string>();
                query = String.Format(
                        @"SELECT NODELINKS.ID, CAST((TP.NAME||'/'||SECTTP.NAME) AS CHAR(100)) NODE_NAME
                      FROM TP
                      INNER JOIN SECTTP
                      ON (SECTTP.ID_PST = TP.ID)
                      INNER JOIN NODELINKS
                      ON (SECTTP.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=1", fider.ID);
                try
                {
                    using (var command = new FbCommand(query, fbConnection))
                    {
                        if (fbConnection.State == System.Data.ConnectionState.Closed)
                        {
                            fbConnection.Open();
                        }
                        FbDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                            try
                            {
                                while (reader.Read())
                                {
                                    int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                    string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                    dictionaryNodeTPNames.Add(key, value);
                                }
                            }
                            finally { reader.Close(); }
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return result;
                }

                #endregion Таблица для поиска названия узла ТП по его ИД

                #region Таблица для поиска названия узла по его ИД

                Dictionary<int, string> dictionaryNodeNames = new Dictionary<int, string>();
                query = String.Format(
                        @"SELECT NODELINKS.ID, NODE.NAME NODE_NAME
                      FROM NODE
                      INNER JOIN NODELINKS
                      ON (NODE.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=2 AND NODELINKS.ID IN
                      (
                        SELECT LINE.ID_NODE1 NODEID FROM LINE
                        WHERE LINE.ID_FIDER = {0}
                        UNION
                        SELECT LINE.ID_NODE2 NODEID FROM LINE
                        WHERE LINE.ID_FIDER = {0}
                      )", fider.ID);
                try
                {
                    using (var command = new FbCommand(query, fbConnection))
                    {
                        if (fbConnection.State == System.Data.ConnectionState.Closed)
                        {
                            fbConnection.Open();
                        }
                        FbDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                            try
                            {
                                while (reader.Read())
                                {
                                    int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                    string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                    if (dictionaryNodeNames.ContainsKey(key) == false)
                                        dictionaryNodeNames.Add(key, value);
                                }
                            }
                            finally { reader.Close(); }
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return result;
                }
                #endregion Таблица для поиска названия узла по его ИД

                #region Таблица для поиска названия узла подстанции по его ИД

                Dictionary<int, string> dictionaryNodePSTName = new Dictionary<int, string>();
                query = String.Format(
                        @"SELECT NODELINKS.ID, CAST(('[ЦП] - '||PST.NAME||'/'||SECTPST.NAME) AS CHAR(100)) NODE_NAME
                      FROM PST
                      INNER JOIN SECTPST
                      ON  (SECTPST.ID_PST = PST.ID)
                      INNER JOIN NODELINKS
                      ON  (SECTPST.ID_NODELINKS = NODELINKS.ID)
                      WHERE NODELINKS.NODE_TYPE=3 AND SECTPST.ID = {0}", fider.ID_Sect_NN);
                try
                {
                    using (var command = new FbCommand(query, fbConnection))
                    {
                        if (fbConnection.State == System.Data.ConnectionState.Closed)
                        {
                            fbConnection.Open();
                        }
                        FbDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                            try
                            {
                                while (reader.Read())
                                {
                                    int key = reader[0] != DBNull.Value ? (int)reader[0] : default(int);
                                    string value = reader[1] != DBNull.Value ? (string)reader[1] : default(string);

                                    dictionaryNodePSTName.Add(key, value);
                                }
                            }
                            finally { reader.Close(); }
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    LastException = ex;
                    return result;
                }
                #endregion Таблица для поиска названия узла подстанции по его ИД

                // список участков схемы фидера (узел задан ИД)
                ICollection<Model.Line> lineNodesList = GetLines(fider.ID);
                // цикл по всем участкам схемы фидера                
                foreach (var item in lineNodesList)
                {
                    FillVertexProperties(item.NodeStartId, ref dictionaryNodeTPNames, ref dictionaryNodeNames, ref dictionaryNodePSTName, ref substationFidersVertities);
                    FillVertexProperties(item.NodeEndId, ref dictionaryNodeTPNames, ref dictionaryNodeNames, ref dictionaryNodePSTName, ref substationFidersVertities);

                    item.NodeStart = substationFidersVertities[item.NodeStartId].Name;
                    item.NodeEnd = substationFidersVertities[item.NodeEndId].Name;

                    // добавление ребра графа
                    substationFidersEdges.Add(new Graph.FiderGraphEdge(edgeIndex, substationFidersVertities[item.NodeStartId], substationFidersVertities[item.NodeEndId]));
                    edgeIndex++;
                }

                // добавление в общий список
                substationFidersLineNodesList.AddRange(lineNodesList);
                substationFidersVertitiesList.AddRange(currentFiderVertitiesList);

            }

            result.Lines = substationFidersLineNodesList;
            result.GraphVertities = substationFidersVertitiesList;
            result.GraphEdges = substationFidersEdges;

            return result;
        }

        #endregion Get DWRES Tables

        #region Public Methods

        public void Close()
        {
            if (fbConnection.State != System.Data.ConnectionState.Closed)
            {
                fbConnection.Close();
                fbConnection.Dispose();
            }
        }

        #endregion

        public bool HasError { get; private set; }
        public Exception LastException { get; set; }

        private void btnSelect_Click(object sender, EventArgs e) //обрабатываем событие Click кнопки Select
        {
            //так проверять состояние соединения (активно или не активно)
            if (fbConnection.State == System.Data.ConnectionState.Closed)
                fbConnection.Open();

            FbTransaction fbt = fbConnection.BeginTransaction(); //стартуем транзакцию; стартовать транзакцию можно только для открытой базы (т.е. мутод Open() уже был вызван ранее, иначе ошибка)

            FbCommand SelectSQL = new FbCommand("SELECT * FROM BOOKS", fbConnection); //задаем запрос на выборку

            SelectSQL.Transaction = fbt; //необходимо проинициализить транзакцию для объекта SelectSQL
            FbDataReader reader = SelectSQL.ExecuteReader(); //для запросов, которые возвращают результат в виде набора данных надо использоваться метод ExecuteReader()

            string select_result = ""; //в эту переменную будем складывать результат запроса Select

            try
            {
                while (reader.Read()) //пока не прочли все данные выполняем...
                {
                    select_result = select_result + reader.GetInt32(0).ToString() + ", " + reader.GetString(1) + "\n";
                }
            }
            finally
            {
                //всегда необходимо вызывать метод Close(), когда чтение данных завершено
                reader.Close();
                fbConnection.Close(); //закрываем соединение, т.к. оно нам больше не нужно
            }
            MessageBox.Show(select_result); //выводим результат запроса
            SelectSQL.Dispose(); //в документации написано, что ОЧЕНЬ рекомендуется убивать объекты этого типа, если они больше не нужны
        }
    }

    public class DBConnectionParams
    {
        public string DataSource { get; set; }
        public string Database { get; set; }
    }

    public class FiderNameComparer : IComparer<Model.Fider>
    {
        public int Compare(Model.Fider x, Model.Fider y)
        {
            string x_fider_number = String.Empty, y_fider_number = String.Empty;

            if (x.Name.IndexOf("-") > 0)
                x_fider_number = x.Name.Substring(x.Name.IndexOf("-")+1);
            if (y.Name.IndexOf("-") > 0)
                y_fider_number = y.Name.Substring(y.Name.IndexOf("-") + 1);
            int x_number = 0, y_number = 0;
            if (Int32.TryParse(x_fider_number, out x_number) && Int32.TryParse(y_fider_number, out y_number))
                return x_number.CompareTo(y_number);
            else
                return x_fider_number.CompareTo(y_fider_number);
        }
    }

}