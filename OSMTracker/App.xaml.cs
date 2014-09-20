using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using System.IO.IsolatedStorage;

using OSMTracker.Resources;
using OSMTracker.ViewModels;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Markup;

using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using Coding4Fun.Toolkit.Controls;

namespace OSMTracker
{
    public partial class App : Application
    {
        // Global variable to set app locale at launch for International testing
        // An empty value causes the app to following users phone language culture
        public static String appForceCulture = "";
        public static MainPage mPage = null;
        GeoCoordinateWatcher gcw;
        MapPolyline trackseg = new MapPolyline();
        Boolean isRecording = false;
        List<GeoPosition<GeoCoordinate>> trk = new List<GeoPosition<GeoCoordinate>>();
        static List<Trace> allTraces = new List<Trace>();
        static Traces traces;


        /// <summary>
        /// Provides easy access to the root frame of the phone app.
        /// </summary>
        /// <returns>The root frame of the phone app.</returns>
        
        private static MainViewModel viewModel = null;

        /// <summary>
        /// 视图用于进行绑定的静态 ViewModel。
        /// </summary>
        /// <returns>MainViewModel 对象。</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // 延迟创建视图模型，直至需要时
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// 提供对电话应用程序的根框架的轻松访问。
        /// </summary>
        /// <returns>电话应用程序的根框架。</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Application 对象的构造函数。
        /// </summary>
        public App()
        {
            // 未捕获的异常的全局处理程序。 
            UnhandledException += Application_UnhandledException;

            // 标准 Silverlight 初始化
            InitializeComponent();

            // 特定于电话的初始化
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // 调试时显示图形分析信息。
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 显示当前帧速率计数器。
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // 显示在每个帧中重绘的应用程序区域。
                //Application.Current.Host.Settings.EnableRedrawRegions = true；

                // 启用非生产分析可视化模式， 
                // 该模式显示递交给 GPU 的包含彩色重叠区的页面区域。
                //Application.Current.Host.Settings.EnableCacheVisualization = true；

                // 通过将应用程序的 PhoneApplicationService 对象的 UserIdleDetectionMode 属性
                // 设置为 Disabled 来禁用应用程序空闲检测。
                //  注意: 仅在调试模式下使用此设置。禁用用户空闲检测的应用程序在用户不使用电话时将继续运行
                // 并且消耗电池电量。
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            var appBar = App.Current.Resources["TrackingBar"] as ApplicationBar;
            ((ApplicationBarIconButton)appBar.Buttons[0]).Text = AppResources.BtnRecord;
            ((ApplicationBarIconButton)appBar.Buttons[1]).Text = AppResources.BtnStop;
            ((ApplicationBarIconButton)appBar.Buttons[2]).Text = AppResources.BtnAdd;
            ((ApplicationBarMenuItem)appBar.MenuItems[0]).Text = AppResources.MIRecordSetting;
            ((ApplicationBarMenuItem)appBar.MenuItems[1]).Text = AppResources.MIAbout;

            var appBar2 = App.Current.Resources["ManageBar"] as ApplicationBar;
            ((ApplicationBarIconButton)appBar2.Buttons[0]).Text = AppResources.BtnCheck;
            ((ApplicationBarIconButton)appBar2.Buttons[1]).Text = AppResources.BtnSort;
            ((ApplicationBarIconButton)appBar2.Buttons[2]).Text = AppResources.BtnShare;
            ((ApplicationBarMenuItem)appBar2.MenuItems[0]).Text = AppResources.MIAppSetting;
            ((ApplicationBarMenuItem)appBar2.MenuItems[1]).Text = AppResources.MIAbout;
             
            
        }

        // 应用程序启动(例如，从“开始”菜单启动)时执行的代码
        // 此代码在重新激活应用程序时不执行
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // 激活应用程序(置于前台)时执行的代码
        // 此代码在首次启动应用程序时不执行
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // 确保正确恢复应用程序状态
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // 停用应用程序(发送到后台)时执行的代码
        // 此代码在应用程序关闭时不执行
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // 确保所需的应用程序状态在此处保持不变。
        }

        // 应用程序关闭(例如，用户点击“后退”)时执行的代码
        // 此代码在停用应用程序时不执行
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // 导航失败时执行的代码
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 导航已失败；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        // 出现未处理的异常时执行的代码
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.Message);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 出现未处理的异常；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that your apps font is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each .resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Change locale to appForceCulture if it is not empty
                if (String.IsNullOrWhiteSpace(appForceCulture) == false)
                {
                    // Force app globalization to follow appForceCulture
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(appForceCulture);

                    // Force app UI culture to follow appForceCulture
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(appForceCulture);
                }
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the display
                // language of the phone is not supported.
                //
                // If a compiler error occurs, ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error occurs, ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection, false);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                throw;
            }
        }

        

        #region 电话应用程序初始化

        // 避免双重初始化
        private bool phoneApplicationInitialized = false;

        // 请勿向此方法中添加任何其他代码
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // 创建框架但先不将它设置为 RootVisual；这允许初始
            // 屏幕保持活动状态，直到准备呈现应用程序时。
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // 处理导航故障
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // 确保我们未再次初始化
            phoneApplicationInitialized = true;
        }

        // 请勿向此方法中添加任何其他代码
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // 设置根视觉效果以允许应用程序呈现
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // 删除此处理程序，因为不再需要它
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        private void ApplicationBarIconButtonRecord_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                InitializeGeoWatcher();
                InitializeTrkseg();//MessageBox.Show((Application.Current.RootVisual as PhoneApplicationFrame).Content as MainPage);
                gcw.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(gcw_StatusChanged);
                gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
                gcw.Start();

                /*
                MainPage mPage = (Application.Current.RootVisual as PhoneApplicationFrame).Content as MainPage;


                if (!gcw.Position.Location.IsUnknown)
                {
                    Map map = mPage.map;
                    map.Center = new GeoCoordinate(gcw.Position.Location.Latitude, gcw.Position.Location.Longitude);
                    //if (gcw != null) gcw.Stop();
                    map.ZoomLevel = 10;
                    //http://maps.googleapis.com/maps/api/geocode/json?latlng=30.533649427844708,114.34974454353969&sensor=true_or_false
                    //google reverse geocoding API
                    mPage.curAdmin.Text = "Administration info";
                    mPage.curCoord.Text = gcw.Position.Location.Latitude + "," + gcw.Position.Location.Longitude;
                    this.trackseg.Locations.Add(gcw.Position.Location);
                    trk.Add(gcw.Position);

                    //Add a pin to map
                    if (map.Children.Count != 0)
                    {
                        var pushpin = map.Children.FirstOrDefault(p => (p.GetType() == typeof(Pushpin) && ((Pushpin)p).Tag == "locationPushpin"));

                        if (pushpin != null)
                        {
                            map.Children.Remove(pushpin);
                        }
                    }

                    Pushpin locationPushpin = new Pushpin();
                    locationPushpin.Tag = "locationPushpin";
                    locationPushpin.Content = "Last";
                    locationPushpin.Location = gcw.Position.Location;
                    map.Children.Add(locationPushpin);
                    map.Children.Add(trackseg);
                }
                else
                {
                    mPage.curAdmin.Text = AppResources.UnknownLocation;
                    mPage.curCoord.Text = "";
                    MessageBox.Show(AppResources.UnknownLocation);
                }*/
            }
            isRecording = !isRecording;
        
        }


        // Initialize listener
        private void InitializeGeoWatcher()
        {
            gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            gcw.MovementThreshold = 20;
        }
        // Initialize track layer
        private void InitializeTrkseg()
        {
            trackseg.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan);
            trackseg.StrokeThickness = 2;
            trackseg.Locations = new LocationCollection();
        }

        void gcw_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            //Deployment.Current.Dispatcher.BeginInvoke(() => MyStatusChanged(e));

        }

        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => MyPositionChanged(e));
        }


        void MyPositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            MainPage mPage = (Application.Current.RootVisual as PhoneApplicationFrame).Content as MainPage;
            Map map = mPage.map;
            // Update the map to show the current location
            GeoCoordinate cur = e.Position.Location;
            trk.Add(e.Position);
            map.SetView(cur, 15);
            map.Visibility = System.Windows.Visibility.Visible;

            //update pushpin location and show
            //lstCoord.Text = curCoord.Text;
            mPage.curCoord.Text = "@" + e.Position.Timestamp + cur.Latitude + "," + cur.Longitude;
            //lstAdmin.Text = curAdmin.Text;
            mPage.curAdmin.Text = "Administrative address not supported now.";

            //Add a pin to map
            if (map.Children.Count != 0)
            {
                var pushpin = map.Children.FirstOrDefault(p => (p.GetType() == typeof(Pushpin) && ((Pushpin)p).Tag == "locationPushpin"));

                if (pushpin != null)
                {
                    map.Children.Remove(pushpin);
                }
            }

            Pushpin locationPushpin = new Pushpin();
            locationPushpin.Tag = "locationPushpin";
            locationPushpin.Content = "Now";
            locationPushpin.Location = cur;
            map.Children.Add(locationPushpin);

            // Draw lines between trackpoints
            this.trackseg.Locations.Add(cur);
            this.trackseg.Visibility = System.Windows.Visibility.Visible;

        }

        private void ApplicationBarIconButtonStop_Click(object sender, EventArgs e)
        {
            if (isRecording)
            {
                gcw.Stop();
                isRecording = false;
                string filename = DateTime.Now.ToString("yyyyMMdd_HHmm");
                
                InputPrompt input = new InputPrompt();
                input.Title = "Trace name:"; input.Value = filename; input.IsCancelVisible = true;
                input.Completed += new EventHandler<PopUpEventArgs<string, PopUpResult>>(input_Completed);
                input.Show();
                 
                //ViewModel.Items.Add(new Trace(filename, DateTime.Now, trk.Count));
            }
            else
                MessageBox.Show("Not recording.");
        }

        private void input_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.Result != null)
            {
                string filename = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                if (e.Result != "")
                    filename = e.Result;

                //allTraces.Add(new Trace(filename, DateTime.Now, trk.Count));
                ViewModel.Items.Add(new Trace(filename, DateTime.Now, trk.Count));
                
                // Write to file
                
                GpxWriter writer = new GpxWriter(filename);
                foreach (GeoPosition<GeoCoordinate> gpsinfo in trk)
                {
                    writer.AddGpsInfo(gpsinfo);
                }
                writer.WriteToGpx();
                
                MessageBox.Show("Saved to " + filename + ".\nTotal"+ trk.Count + " points.\nStart: " + trk.ElementAt(0).Timestamp.ToString() + "\nEnd: " + trk.ElementAt(trk.Count-1).Timestamp.ToString());
                trk.Clear();
                //TODO: clear map layer for track segment
            }
            else
            {
                MessageBox.Show("Not saved.");
            }
        }

        private void ApplicationBarIconButtonCheck_Click(object sender, EventArgs e)
        {
            MainPage mPage = (Application.Current.RootVisual as PhoneApplicationFrame).Content as MainPage;
            mPage.listBoxCheckable.IsInChooseState = true;
            var onCheckBar = App.Current.Resources["onCheckBar"] as ApplicationBar;
            ((ApplicationBarIconButton)onCheckBar.Buttons[0]).Text = AppResources.BtnDraw;
            ((ApplicationBarIconButton)onCheckBar.Buttons[1]).Text = AppResources.BtnUpload;
            ((ApplicationBarIconButton)onCheckBar.Buttons[2]).Text = AppResources.BtnDelete;
            ((ApplicationBarIconButton)onCheckBar.Buttons[3]).Text = AppResources.BtnCancel;
            mPage.ApplicationBar = onCheckBar;
        }

        private void ApplicationBarIconButtonCheckCancel_Click(object sender, EventArgs e)
        {
            mPage.listBoxCheckable.IsInChooseState = false;
            mPage.ApplicationBar = App.Current.Resources["ManageBar"] as ApplicationBar;
        }

        private void ApplicationBarIconButtonCheckUpload_Click(object sender, EventArgs e)
        {
            Trace listTrace = mPage.listBoxCheckable.SelectedItem as Trace;
            //MessageBox.Show(listTrace.LineTwo + " traces selected.");
            uploadViaMail(listTrace);
            ApplicationBarIconButtonCheckCancel_Click(null,null);
        }

        private void uploadViaMail(Trace selected)
        {
            EmailComposeTask ect = new EmailComposeTask();
            ect.Body = getGpxString(selected);
            ect.Subject = selected.Name;
            ect.Show();
        }

        private string getGpxString(Trace trace)
        {
            string filename = trace.Name;
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isoFile.FileExists(filename))
            {
                MessageBox.Show("File not found.");
                return "";
            }
            string gpxStr;
            using (IsolatedStorageFileStream stream = isoFile.OpenFile(filename, System.IO.FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    gpxStr = reader.ReadToEnd();
                }
            }
            return gpxStr;
        }

        private void ApplicationBarIconButtonCheckDelete_Click(object sender, EventArgs e)
        {
            Trace select = mPage.listBoxCheckable.SelectedItem as Trace;
            if ( -1 == deleteGpx(select.Name))
                MessageBox.Show("File does not exist.");
            ViewModel.Items.Remove(select);
            ApplicationBarIconButtonCheckCancel_Click(null, null);
        }

        private int deleteGpx(string filename)
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isoFile.FileExists(filename))
            {
                //MessageBox.Show("File not found.");
                return -1;
            }
            isoFile.DeleteFile(filename);
            return 0;
        }
    }
}