namespace TMPApplication
{
    using System;
    using System.Collections.Generic;

    public class ServiceInjector
    {
        #region Fields

        public static readonly ServiceInjector Instance = new ServiceInjector();

        private readonly Dictionary<Type, object> serviceMap;
        private readonly object serviceMapLock;

        #endregion

        #region Constructor

        public ServiceInjector()
        {
            this.serviceMap = new Dictionary<Type, object>();
            this.serviceMapLock = new object();
        }

        #endregion

        #region Methods

        public void AddService<TServiceContract>(TServiceContract implementation)
            where TServiceContract : class
        {
            lock (this.serviceMapLock)
            {
                this.serviceMap[typeof(TServiceContract)] = implementation;
            }
        }

        public TServiceContract GetService<TServiceContract>()
            where TServiceContract : class
        {
            object service;
            lock (this.serviceMapLock)
            {
                this.serviceMap.TryGetValue(typeof(TServiceContract), out service);
            }

            return service as TServiceContract;
        }

        public static void InjectServices()
        {
        }

        #endregion
    }
}
