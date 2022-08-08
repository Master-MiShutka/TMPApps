using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.AmperM.TestApp
{
/*  using System.Windows;
  using MsgBox;
  using ServiceLocator;
  internal class MessageBox
  {
    static IMessageBoxService service;
    static MessageBox()
    {
      service = ServiceContainer.Instance.GetService<IMessageBoxService>();
      if (service == null)
        throw new NullReferenceException("Not found IMessageBoxService");
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, defaultCloseResult, dialogCanCloseViaChrome, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, MsgBoxButtons buttonOption, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, buttonOption, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, defaultCloseResult, dialogCanCloseViaChrome, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(Exception exp, string caption, MsgBoxButtons buttonOption, MsgBoxImage image, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(exp, caption, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, MsgBoxButtons buttonOption, MsgBoxImage image, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, MsgBoxButtons buttonOption, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, buttonOption, defaultCloseResult, dialogCanCloseViaChrome, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(Window owner, string messageBoxText, string caption = "", MsgBoxButtons buttonOption = MsgBoxButtons.OK, MsgBoxImage image = MsgBoxImage.Error, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLinkLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(owner, messageBoxText, caption, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(Exception exp, string textMessage = "", string caption = "", MsgBoxButtons buttonOption = MsgBoxButtons.OK, MsgBoxImage image = MsgBoxImage.Error, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool enableCopyFunction = false)
    {
      return service.Show(exp, textMessage, caption, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, enableCopyFunction);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, string details, MsgBoxButtons buttonOption, MsgBoxImage image, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, details, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, MsgBoxButtons buttonOption, MsgBoxImage image, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, buttonOption, image, defaultCloseResult, dialogCanCloseViaChrome, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(string messageBoxText, string caption, string details, MsgBoxButtons buttonOption, MsgBoxImage image, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(messageBoxText, caption, details, buttonOption, image, defaultCloseResult, dialogCanCloseViaChrome, btnDefault, helpLink, helpLinkTitle, helpLabel, navigateHelplinkMethod, showCopyMessage);
    }

    public static System.Windows.MessageBoxResult Show(Window owner, string messageBoxText, string caption, System.Windows.MessageBoxResult defaultCloseResult, bool dialogCanCloseViaChrome, MsgBoxButtons buttonOption = MsgBoxButtons.OK, MsgBoxImage image = MsgBoxImage.Error, System.Windows.MessageBoxResult btnDefault = System.Windows.MessageBoxResult.None, object helpLink = null, string helpLinkTitle = "", string helpLinkLabel = "", Func<object, bool> navigateHelplinkMethod = null, bool showCopyMessage = false)
    {
      return service.Show(owner, messageBoxText, caption, defaultCloseResult, dialogCanCloseViaChrome, buttonOption, image, btnDefault, helpLink, helpLinkTitle, helpLinkLabel, navigateHelplinkMethod, showCopyMessage);
    }
  }
  */
}