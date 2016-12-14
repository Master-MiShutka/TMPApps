using System;
using System.Collections.Generic;
using System.Data.Common;

namespace TMP.DWRES.Objects
{
    #region argsWrappers

    /// <summary>
    /// Это базовый класс для все оболочек вокруг args
    /// </summary>
    public class BaseargsWrapper
    {
        private int id;

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public BaseargsWrapper()
        {
        }

        public BaseargsWrapper(DbDataReader args)
        {
            if (args == null) throw new ArgumentNullException();

            id = args[0] != DBNull.Value ? (int)args[0] : default(int);
        }
    }

    public class EnergoSystem : BaseargsWrapper
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public ICollection<Filial> Filials { get; set; }

        public EnergoSystem()
            : base()
        {
        }

        public EnergoSystem(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 2) throw new ArgumentException();

            name = args[1] != DBNull.Value ? (String)args[1] : default(String);
            Filials = new List<Filial>();
        }
    }

    public class Filial : BaseargsWrapper
    {
        private int id_energosys;

        public int ID_Energosys
        {
            get { return id_energosys; }
            set { id_energosys = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public ICollection<Res> Reses { get; set; }

        public Filial()
            : base()
        {
        }

        public Filial(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 3) throw new ArgumentException();

            id_energosys = args[1] != DBNull.Value ? (int)args[1] : default(int);
            name = args[2] != DBNull.Value ? (String)args[2] : default(String);
            code = args[3] != DBNull.Value ? (String)args[3] : default(String);

            Reses = new List<Res>();
        }
    }

    public class Res : BaseargsWrapper
    {
        private int id_filial;

        public int ID_Filial
        {
            get { return id_filial; }
            set { id_filial = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public ICollection<Substation> Substations { get; set; }

        public Res()
            : base()
        {
        }

        public Res(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 3) throw new ArgumentException();

            id_filial = args[1] != DBNull.Value ? (int)args[1] : default(int);
            name = args[2] != DBNull.Value ? (String)args[2] : default(String);
            code = args[3] != DBNull.Value ? (String)args[3] : default(String);

            Substations = new List<Substation>();
        }
    }

    public class Substation : BaseargsWrapper
    {
        private int id_res;

        public int ID_Res
        {
            get { return id_res; }
            set { id_res = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string shortname;

        public string ShortName
        {
            get { return shortname; }
            set { shortname = value; }
        }

        public ICollection<Fider> Fiders { get; set; }

        public Substation()
            : base()
        {
        }

        public Substation(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 3) throw new ArgumentException();

            id_res = args[1] != DBNull.Value ? (int)args[1] : default(int);
            name = args[2] != DBNull.Value ? (String)args[2] : default(String);
            shortname = args[3] != DBNull.Value ? (String)args[3] : default(String);

            Fiders = new List<Fider>();
        }
    }

    public class Fider : BaseargsWrapper
    {
        private int id_pst;

        public int ID_Pst
        {
            get { return id_pst; }
            set { id_pst = value; }
        }

        private int id_sect_nn;

        public int ID_Sect_NN
        {
            get { return id_sect_nn; }
            set { id_sect_nn = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Fider()
            : base()
        {
        }

        public Fider(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 3) throw new ArgumentException();

            id_pst = args[1] != DBNull.Value ? (int)args[1] : default(int);
            id_sect_nn = args[2] != DBNull.Value ? (int)args[2] : default(int);
            name = args[3] != DBNull.Value ? (String)args[3] : default(String);
        }
    }

    public class Tp : BaseargsWrapper
    {
        private int id_res;

        public int ID_Res
        {
            get { return id_res; }
            set { id_res = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int obj_type;

        public int Obj_type
        {
            get { return obj_type; }
            set { obj_type = value; }
        }

        private string subj_type;

        public string Subj_type
        {
            get { return subj_type; }
            set { subj_type = value; }
        }

        private int id_fider;

        public int ID_Fider
        {
            get { return id_fider; }
            set { id_fider = value; }
        }

        public Tp()
            : base()
        {
        }

        public Tp(DbDataReader args)
            : base(args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 5) throw new ArgumentException();

            id_res = args[1] != DBNull.Value ? (int)args[1] : default(int);
            name = args[2] != DBNull.Value ? (String)args[2] : default(String);
            obj_type = args[3] != DBNull.Value ? (int)args[3] : default(int);
            subj_type = args[4] != DBNull.Value ? (String)args[4] : default(String);
            id_fider = args[5] != DBNull.Value ? (int)args[5] : default(int);
        }
    }

    public class Line
    {
        public int ID { get; set; }

        public int NodeStartId { get; set; }
        public string NodeStart { get; set; }

        public int NodeEndId { get; set; }
        public string NodeEnd { get; set; }

        public short KAStartState { get; set; }

        public short KAEndState { get; set; }

        public string Schema { get; set; }

        public short AB { get; set; }

        public Line()
        {
        }

        public Line(DbDataReader args)
        {
            if (args == null) throw new ArgumentNullException();
            if (args.FieldCount < 6) throw new ArgumentException();

            ID = args[0] != DBNull.Value ? (int)args[0] : default(int);
            NodeStartId = args[1] != DBNull.Value ? (int)args[1] : default(int);
            NodeEndId = args[2] != DBNull.Value ? (int)args[2] : default(int);
            KAStartState = args[3] != DBNull.Value ? (short)args[3] : default(short);
            KAEndState = args[4] != DBNull.Value ? (short)args[4] : default(short);
            Schema = args[5] != DBNull.Value ? (string)args[5] : default(string);
            AB = args[6] != DBNull.Value ? (short)args[6] : default(short);
        }
    }

    public class LineVertex : BaseargsWrapper
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public LineVertex()
            : base()
        {
        }

        public LineVertex(DbDataReader args)
            : base(args)
        {
        }
    }

    #endregion argsWrappers
}