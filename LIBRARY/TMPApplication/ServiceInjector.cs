using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMPApplication
{
    public class ServiceInjector
    {
        #region Fields

        public static readonly ServiceInjector Instance = new ServiceInjector();

        readonly Dictionary<Type, object> _serviceMap;
        readonly object _serviceMapLock;

        #endregion

        #region Constructor

        public ServiceInjector()
        {
            _serviceMap = new Dictionary<Type, object>();
            _serviceMapLock = new object();
        }

        #endregion

        #region Methods

        public void AddService<TServiceContract>(TServiceContract implementation)
            where TServiceContract : class
        {
            lock (_serviceMapLock)
            {
                _serviceMap[typeof(TServiceContract)] = implementation;
            }
        }

        public TServiceContract GetService<TServiceContract>()
            where TServiceContract : class
        {
            object service;
            lock (_serviceMapLock)
            {
                _serviceMap.TryGetValue(typeof(TServiceContract), out service);
            }

            return service as TServiceContract;
        }

        public static void InjectServices()
        {

        }

        #endregion
    }
}
