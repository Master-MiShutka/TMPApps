namespace WPFHexaEditor.Core.ROMTable
{
    using System;

    /// <summary>
    /// Represente une adresse favorite dans une ROM
    ///
    /// Derek Tremblay 2003-2017
    /// </summary>
    public sealed class Bookmark
    {
        public string Position;
        public string Name;
        public string File;
        public string Key;

        public Bookmark()
        {
        }

        public Bookmark(string name, string position, string file, string key)
        {
            this.Position = position;
            this.Name = name;
            this.File = file;
            this.Key = key;
        }
    }
}
