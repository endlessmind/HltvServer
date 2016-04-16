using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace HltvRss
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();


        ILog log;
        public App()
        {

            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
            this.MainWindow = MainWindow;

            AllocConsole();

            XmlConfigurator.Configure();

            //AppDomain currentDomain = AppDomain.CurrentDomain;

            //currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

        }

#if DEBUG
        public const bool isDebug = true;
#else
        public const bool isDebug = false;
#endif


        //List of accepted commands
        public static String CMD_REQUEST_MATCHES = "server_get_matches";
        public static String CMD_REQUEST_SERVER_STATUS = "server_ge_mig_status";
        public static String CMD_REQUEST_SERVER_CHARTS = "server_diagram_data_tack";
        public static String CMD_REQUEST_REGISTER_GCM = "server_regi_gcm_device";
        public static String CMD_REQUEST_UNREGISTER_GCM = "server_unregi_gcm_device";
        public static String CMD_REQUEST_UPDATE_GCM_TOKEN = "server_update_gcm_device";
        public static String CMD_REQUEST_REGISTER_WATCH = "server_regi_watch_match";
        public static String CMD_REQUEST_UNREGISTER_WATCH = "server_unregi_watch_match";
        public static String CMD_REQUEST_WATCH_STATUS = "server_get_watch_status";
        public static String CMD_REQUEST_TEAM_NAMES = "server_fetch_team_names_from_ids"; //Will be a lot slower than all the other commands

        public static String CMD_REQUEST_DATA_STATUS = "server_is_data_upto_date"; //Should simply return true or false;
        public static String CMD_REQUEST_ACCEPT_DATA = "server_here_is_your_data"; //Should received parsed data back from the server.
        public static String CMD_REQUEST_ACCEPT_SB_STATUS = "server_scorebot_status";



        public const int GCM_REGISTER_SUCCESS = 1;
        public const int GCM_REGISTER_KEY_ALREADY_EXISTS = 2;
        public const int GCM_REGISTER_ERROR = -1;
        public const int GCM_UPDATE_SUCCESS = 3;
        public const int GCM_UPDATE_ERROR = -2;
        public const int GCM_UNREG_SUCCESS = 1;
        public const int GCM_UNREG_ERROR = 0;
        public const int GCM_UNREG_NOT_FOUND = 2;


        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            //MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Exception ex = default(Exception);
            ex = (Exception)e.Exception;
            
            log.Error(ex.Message + "\n" + ex.StackTrace);

            e.Handled = true;
        }

        private void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = (Exception)e.ExceptionObject;
            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

        private void GlobalThreadExceptionHandler(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = default(Exception);
            ex = e.Exception;
            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

    }
}
