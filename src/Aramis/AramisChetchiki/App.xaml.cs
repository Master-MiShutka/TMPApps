﻿global using TMP.WORK.AramisChetchiki.Model;

namespace TMP.WORK.AramisChetchiki
{
    using MessagePack;
    using MessagePack.Resolvers;
    using TomsToolbox.Wpf.Styles;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WindowWithDialogs.BaseApplication
    {
        public override string Title => "Просмотр данных из ПО Арамис";

        protected override void Initialize()
        {
            // The CodePagesEncodingProvider class extends EncodingProvider to make these code pages available to .NET Core. To use these additional code pages, you do the following:
            // Add a reference to the System.Text.Encoding.CodePages.dll assembly to your project.
            // Retrieve a CodePagesEncodingProvider object from the static CodePagesEncodingProvider.Instance property.
            // Pass the CodePagesEncodingProvider object to the Encoding.RegisterProvider method.
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Do this once and store it for reuse.
            IFormatterResolver resolver = MessagePack.Resolvers.CompositeResolver.Create(

                // resolver custom types first
                AramisDataInfoResolver.Instance,
                MessagePack.Resolvers.AttributeFormatterResolver.Instance,
                MessagePack.Resolvers.TypelessObjectResolver.Instance,
                MessagePack.Resolvers.NativeDateTimeResolver.Instance,

                // finally use standard resolver
                StandardResolver.Instance);
            MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            MessagePackSerializer.DefaultOptions = options;

            ModelHelper.Init();

            this.AppSettings = TMP.WORK.AramisChetchiki.Properties.Settings.Default;

            System.Threading.Thread.CurrentThread.Name = "MainAppThread";

            this.Resources.MergedDictionaries.Insert(0, TomsToolbox.Wpf.Styles.WpfStyles.GetDefaultStyles().RegisterDefaultWindowStyle());

            var window = new MainWindow();
            CorrectMainWindowSizeAndPos(window);
            this.MainWindowWithDialogs = window;
            this.MainViewModel = new ViewModel.MainViewModel();

            SplashScreen.SplashScreenAdapter.CloseSplashScreen();
        }
    }
}
