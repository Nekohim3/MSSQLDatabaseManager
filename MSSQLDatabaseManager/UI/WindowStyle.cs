using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.UI
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            Window window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    public class LoadingControlViewModel : NotificationObject
    {
        public Visibility LoadingVisibility => IsVisible ? Visibility.Visible : Visibility.Collapsed;

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
                RaisePropertyChanged(() => LoadingVisibility);
            }
        }

        private string _loadingText;

        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                RaisePropertyChanged(() => LoadingText);
            }
        }

        public LoadingControlViewModel()
        {
            //IsVisible   = true;
            //LoadingText = "Restore database, please wait... [59%]";
        }
    }
    
    public class NMsgViewModel : NotificationObject
    {
        private NMsgReply _reply;

        public NMsgReply Reply
        {
            get => _reply;
            set
            {
                _reply = value;
                RaisePropertyChanged(() => Reply);
            }
        }

        private string _msgText;

        public string MsgText
        {
            get => _msgText;
            set
            {
                _msgText = value;
                RaisePropertyChanged(() => MsgText);
            }
        }

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public Visibility MsgVisibility => IsMsgVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility YesVisibility => IsYesVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoVisibility => IsNoVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OkVisibility => IsOkVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CancelVisibility => IsCancelVisible ? Visibility.Visible : Visibility.Collapsed;

        private bool _isMsgVisible;

        public bool IsMsgVisible
        {
            get => _isMsgVisible;
            set
            {
                _isMsgVisible = value;
                RaisePropertyChanged(() => IsMsgVisible);
                RaisePropertyChanged(() => MsgVisibility);
                
            }
        }

        private bool _isYesVisible;

        public bool IsYesVisible
        {
            get => _isYesVisible;
            set
            {
                _isYesVisible = value;
                RaisePropertyChanged(() => IsYesVisible);
                RaisePropertyChanged(() => YesVisibility);
            }
        }

        private bool _isNoVisible;

        public bool IsNoVisible
        {
            get => _isNoVisible;
            set
            {
                _isNoVisible = value;
                RaisePropertyChanged(() => IsNoVisible);
                RaisePropertyChanged(() => NoVisibility);
            }
        }

        private bool _isOkVisible;

        public bool IsOkVisible
        {
            get => _isOkVisible;
            set
            {
                _isOkVisible = value;
                RaisePropertyChanged(() => IsOkVisible);
                RaisePropertyChanged(() => OkVisibility);
            }
        }

        private bool _isCancelVisible;

        public bool IsCancelVisible
        {
            get => _isCancelVisible;
            set
            {
                _isCancelVisible = value;
                RaisePropertyChanged(() => IsCancelVisible);
                RaisePropertyChanged(() => CancelVisibility);
            }
        }

        public DelegateCommand YesCmd { get; }
        public DelegateCommand NoCmd { get; }
        public DelegateCommand OkCmd { get; }
        public DelegateCommand CancelCmd { get; }

        public NMsgViewModel()
        {
            YesCmd = new DelegateCommand(OnYes);
            NoCmd = new DelegateCommand(OnNo);
            OkCmd = new DelegateCommand(OnOk);
            CancelCmd = new DelegateCommand(OnCancel);
        }

        public NMsgReply Show(string message, string title)
        {
            return Show(message, title, NMsgButtons.Ok);
        }

        public NMsgReply Show(string message, string title, NMsgButtons buttons)
        {
            Clear();
            MsgText = message;
            Title   = title;
            if (buttons == NMsgButtons.Ok)
                IsOkVisible = true;
            if (buttons == NMsgButtons.YesNo)
                IsYesVisible = IsNoVisible = true;
            if (buttons == NMsgButtons.OkCancel)
                IsOkVisible = IsCancelVisible = true;
            if (buttons == NMsgButtons.YesNoCancel)
                IsYesVisible = IsNoVisible = IsCancelVisible = true;
            IsMsgVisible = true;
            while (IsMsgVisible)
            {
                Application.Current.Dispatcher.Invoke(() => { Thread.Sleep(50); }, DispatcherPriority.Background);
            }

            return Reply;
        }

        private void Clear()
        {
            Title           = string.Empty;
            MsgText         = string.Empty;
            IsYesVisible    = false;
            IsNoVisible     = false;
            IsOkVisible     = false;
            IsCancelVisible = false;
        }

        private void OnYes()
        {
            Reply         = NMsgReply.Yes;
            IsMsgVisible = false;
        }

        private void OnNo()
        {
            Reply        = NMsgReply.No;
            IsMsgVisible = false;
        }

        private void OnOk()
        {
            Reply        = NMsgReply.Ok;
            IsMsgVisible = false;
        }

        private void OnCancel()
        {
            Reply        = NMsgReply.Cancel;
            IsMsgVisible = false;
        }
    }

    public partial class VS2012WindowStyle
    {
        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ((Window)sender).StateChanged += WindowStateChanged;
            var w              = ((Window)sender);
            var loadingControl = (Grid)w.Template.FindName("LoadingControl", w);
            g.LoadingControlVM         = new LoadingControlViewModel();
            loadingControl.DataContext = g.LoadingControlVM;
            var nMsgControl = (Grid)w.Template.FindName("NMsgControl", w);
            g.NMsgVM                = new NMsgViewModel();
            nMsgControl.DataContext = g.NMsgVM;
        }

        void WindowStateChanged(object sender, EventArgs e)
        {
            var w = ((Window)sender);
            var handle = w.GetWindowHandle();
            var containerBorder = (Border)w.Template.FindName("PART_Container", w);

            if (w.WindowState == WindowState.Maximized)
            {
                // Make sure window doesn't overlap with the taskbar.
                var screen = System.Windows.Forms.Screen.FromHandle(handle);
                if (screen.Primary)
                {
                    containerBorder.Padding = new Thickness(
                        SystemParameters.WorkArea.Left + 7,
                        SystemParameters.WorkArea.Top + 7,
                        (SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right) + 7,
                        (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom) + 5);
                }
            }
            else
            {
                containerBorder.Padding = new Thickness(7, 7, 7, 5);
            }
        }

        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.CloseWindow);
        }

        void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.MinimizeWindow);
        }

        void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(w);
                else SystemCommands.MaximizeWindow(w);
            });
        }
    }
    public class SkinResourceDictionary : ResourceDictionary
    {
        private Uri _redSource;
        private Uri _blueSource;

        public Uri RedSource
        {
            get { return _redSource; }
            set
            {
                _redSource = value;
                UpdateSource();
            }
        }
        public Uri BlueSource
        {
            get { return _blueSource; }
            set
            {
                _blueSource = value;
                UpdateSource();
            }
        }

        public void UpdateSource()
        {
            var val = App.Skin == Skin.White ? RedSource : BlueSource;
            if (val != null && base.Source != val)
                base.Source = val;
        }
    }
}
