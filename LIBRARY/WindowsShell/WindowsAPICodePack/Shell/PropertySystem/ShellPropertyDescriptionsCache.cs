// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace Microsoft.WindowsAPICodePack.Shell.PropertySystem
{
    using System.Collections.Generic;
    using Microsoft.WindowsAPICodePack.Shell;

    internal class ShellPropertyDescriptionsCache
    {
        private ShellPropertyDescriptionsCache()
        {
            this.propsDictionary = new Dictionary<PropertyKey, ShellPropertyDescription>();
        }

        private IDictionary<PropertyKey, ShellPropertyDescription> propsDictionary;
        private static ShellPropertyDescriptionsCache cacheInstance;

        public static ShellPropertyDescriptionsCache Cache
        {
            get
            {
                if (cacheInstance == null)
                {
                    cacheInstance = new ShellPropertyDescriptionsCache();
                }

                return cacheInstance;
            }
        }

        public ShellPropertyDescription GetPropertyDescription(PropertyKey key)
        {
            if (!this.propsDictionary.ContainsKey(key))
            {
                this.propsDictionary.Add(key, new ShellPropertyDescription(key));
            }

            return this.propsDictionary[key];
        }
    }
}
