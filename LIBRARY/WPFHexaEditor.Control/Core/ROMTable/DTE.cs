namespace WPFHexaEditor.Core.ROMTable
{
    using System;

    /// <summary>
    /// Objet représentant un DTE.
    ///
    /// Derek Tremblay 2003-2017
    /// </summary>
    public class DTE
    {
        /// <summary>Valeur du DTE</summary>
        private string Value;

        /// <summary>Nom du DTE</summary>
        private string _Entry;

        /// <summary>Type de DTE</summary>
        private DTEType Type;

        #region Constructeurs

        /// <summary>
        /// Constructeur principal
        /// </summary>
        public DTE()
        {
            this._Entry = string.Empty;
            this.Type = DTEType.Invalid;
            this.Value = string.Empty;
        }

        /// <summary>
        /// Contructeur permetant d'ajouter une entrée et une valeur
        /// </summary>
        /// <param name="entry">Nom du DTE</param>
        /// <param name="Value">Valeur du DTE</param>
        public DTE(string entry, string Value)
        {
            this._Entry = entry.ToUpper();
            this.Value = Value;
            this.Type = DTEType.DualTitleEncoding;
        }

        /// <summary>
        /// Contructeur permetant d'ajouter une entrée, une valeur et une description
        /// </summary>
        /// <param name="entry">Nom du DTE</param>
        /// <param name="Value">Valeur du DTE</param>
        /// <param name="Description">Description du DTE</param>
        /// <param name="Type">Type de DTE</param>
        public DTE(string entry, string Value, DTEType Type)
        {
            this._Entry = entry.ToUpper();
            this.Value = Value;
            this.Type = Type;
        }
        #endregion

        #region Propriétés

        /// <summary>
        /// Nom du DTE
        /// </summary>
        public string Entry
        {
            set => this._Entry = value.ToUpper();

            get => this._Entry;
        }

        /// <summary>
        /// Valeur du DTE
        /// </summary>
        public string Value
        {
            set => this.Value = value;

            get => this.Value;
        }

        /// <summary>
        /// Type de DTE
        /// </summary>
        public DTEType Type
        {
            set => this.Type = value;

            get => this.Type;
        }
        #endregion

        #region Méthodes

        /// <summary>
        /// Cette fonction permet de retourner le DTE sous forme : [Entry]=[Valeur]
        /// </summary>
        /// <returns>Retourne le DTE sous forme : [Entry]=[Valeur]</returns>
        public override string ToString()
        {
            if (this.Type != DTEType.EndBlock &&
                this.Type != DTEType.EndLine)
            {
                return this._Entry + "=" + this.Value;
            }
            else
            {
                return this._Entry;
            }
        }
        #endregion

        #region Methodes Static
        public static DTEType TypeDTE(DTE dTEValue)
        {
            try
            {
                switch (dTEValue._Entry.Length)
                {
                    case 2:
                        if (dTEValue.Value.Length == 2)
                        {
                            return DTEType.ASCII;
                        }
                        else
                        {
                            return DTEType.DualTitleEncoding;
                        }

                    case 4: // >2
                        return DTEType.MultipleTitleEncoding;
                }
            }
            catch (IndexOutOfRangeException)
            {
                switch (dTEValue._Entry)
                {
                    case @"/":
                        return DTEType.EndBlock;
                    case @"*":
                        return DTEType.EndLine;

                        // case @"\":
                }
            }
            catch (ArgumentOutOfRangeException)
            { // Du a une entre qui a 2 = de suite... EX:  XX==
                return DTEType.DualTitleEncoding;
            }

            return DTEType.Invalid;
        }

        public static DTEType TypeDTE(string dTEValue)
        {
            try
            {
                if (dTEValue.Length == 1)
                {
                    return DTEType.ASCII;
                }
                else if (dTEValue.Length == 2)
                {
                    return DTEType.DualTitleEncoding;
                }
                else if (dTEValue.Length > 2)
                {
                    return DTEType.MultipleTitleEncoding;
                }
            }
            catch (IndexOutOfRangeException)
            {
                switch (dTEValue)
                {
                    case @"/":
                        return DTEType.EndBlock;
                    case @"*":
                        return DTEType.EndLine;

                        // case @"\":
                }
            }
            catch (ArgumentOutOfRangeException)
            { // Du a une entre qui a 2 = de suite... EX:  XX==
                return DTEType.DualTitleEncoding;
            }

            return DTEType.Invalid;
        }
        #endregion
    }
}
