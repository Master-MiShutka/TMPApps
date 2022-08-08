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
        private string value;

        /// <summary>Nom du DTE</summary>
        private string entry;

        /// <summary>Type de DTE</summary>
        private DTEType type;

        #region Constructeurs

        /// <summary>
        /// Constructeur principal
        /// </summary>
        public DTE()
        {
            this.entry = string.Empty;
            this.type = DTEType.Invalid;
            this.value = string.Empty;
        }

        /// <summary>
        /// Contructeur permetant d'ajouter une entrée et une valeur
        /// </summary>
        /// <param name="entry">Nom du DTE</param>
        /// <param name="Value">Valeur du DTE</param>
        public DTE(string entry, string Value)
        {
            this.entry = entry.ToUpper();
            this.value = Value;
            this.type = DTEType.DualTitleEncoding;
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
            this.entry = entry.ToUpper();
            this.value = Value;
            this.type = Type;
        }
        #endregion

        #region Propriétés

        /// <summary>
        /// Nom du DTE
        /// </summary>
        public string Entry
        {
            set => this.entry = value.ToUpper();

            get => this.entry;
        }

        /// <summary>
        /// Valeur du DTE
        /// </summary>
        public string Value
        {
            set => this.value = value;

            get => this.value;
        }

        /// <summary>
        /// Type de DTE
        /// </summary>
        public DTEType Type
        {
            set => this.type = value;

            get => this.type;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Cette fonction permet de retourner le DTE sous forme : [Entry]=[Valeur]
        /// </summary>
        /// <returns>Retourne le DTE sous forme : [Entry]=[Valeur]</returns>
        public override string ToString()
        {
            if (this.Type != DTEType.EndBlock &&
                this.Type != DTEType.EndLine)
            {
                return this.entry + "=" + this.Value;
            }
            else
            {
                return this.entry;
            }
        }
        #endregion

        #region Methodes Static
        public static DTEType TypeDTE(DTE dTEValue)
        {
            try
            {
                switch (dTEValue.entry.Length)
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
                switch (dTEValue.entry)
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
