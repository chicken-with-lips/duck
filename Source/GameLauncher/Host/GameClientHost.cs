using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Duck;
using Duck.GameHost;
using Duck.Logging;

namespace Game.Host
{
    public class GameClientHost
    {
        #region Properties

        public bool IsLoaded { get; private set; }
        public bool IsBusy { get; private set; }

        #endregion

        #region Members

        private readonly IApplication _application;
        private readonly ILogger _logger;
        private IGameClient? _hostedClient;

        #endregion

        #region Methods

        public GameClientHost(IApplication application, ILogger logger)
        {
            _application = application;
            _logger = logger;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool Load()
        {
            if (IsLoaded) {
                _logger.LogError("Client has already been loaded.");
                return false;
            }

            if (IsBusy) {
                _logger.LogError("Client is busy and cannot be loaded");
                return false;
            }

            IsBusy = true;

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // var assemblyFileName = Path.Combine(directory, "Game.dll");

            var assemblyFileName = "/media/jolly_samurai/Data/Projects/chicken-with-lips/duck/Build/Debug/net6.0/Game/net6.0/Game.dll";
            var assembly = Assembly.LoadFrom(assemblyFileName);

            var clientTypes = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(IGameClient).IsAssignableFrom(type))
                .ToArray();

            if (clientTypes.Length != 1) {
                _logger.LogError("There must be exactly one IClient defined.");
                return false;
            }

            _hostedClient = Activator.CreateInstance(clientTypes[0]) as IGameClient;

            IsBusy = false;
            IsLoaded = true;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool LoadAndInitialize()
        {
            return Load()
                   && Initialize(false);
        }

        private void AssertContextIsReady()
        {
            if (!IsLoaded || IsBusy) {
                throw new Exception("Client is not loaded or is busy");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool Initialize(bool isHotReload)
        {
            AssertContextIsReady();

            if (null == _hostedClient) {
                return false;
            }

            var type = Type.GetType("Duck.GameFramework.GameClient.GameClientInitializationContext, Duck.GameFramework");
            var context = (IGameClientInitializationContext)Activator.CreateInstance(type, new object[] {
                _application,
                isHotReload
            });

            _hostedClient?.Initialize(context);

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Tick()
        {
            AssertContextIsReady();

            _hostedClient?.Tick();
        }

        #endregion
    }
}
