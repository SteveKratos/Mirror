// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MixedRemoteViewCompositor.Network;
using Windows.Web.Http;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Collections;


namespace Viewer
{
    public sealed partial class Playback : Page
    {
        private MediaExtensionManager mediaExtensionManager = null;

        private Connector connector = null;
        private Connection connection = null;

        static private DispatcherTimer dispatcherTimer;
        //static private DispatcherTimer clickTimer;

        public Playback()
        {
            this.InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += GetHoloPoints;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 3, 0);
            dispatcherTimer.Start();

            //clickTimer = new DispatcherTimer();
            //clickTimer.Tick += ClickTimer_Fun;
            //clickTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            //clickTimer.Start();
        }


        private async void bnConnect_Click(object sender, RoutedEventArgs e)
        {
            ushort port = 0;
            if (UInt16.TryParse(this.txPort.Text, out port))
            {
                if (string.IsNullOrEmpty(this.txAddress.Text))
                {
                    this.txAddress.Text = this.txAddress.PlaceholderText;
                }

                this.connector = new Connector(this.txAddress.Text, port);
                if (this.connector != null)
                {
                    this.connection = await this.connector.ConnectAsync();
                    if (this.connection != null)
                    {
                        this.bnConnect.IsEnabled = false;
                        this.bnClose.IsEnabled = true;
                        this.bnStart.IsEnabled = true;
                        this.bnStop.IsEnabled = false;

                        this.connection.Disconnected += Connection_Disconnected;

                        var propertySet = new PropertySet();
                        var propertySetDictionary = propertySet as IDictionary<string, object>;
                        propertySet["Connection"] = this.connection;

                        RegisterSchemeHandler(propertySet);
                    }
                }
            }
        }

        private void bnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        private void bnStart_Click(object sender, RoutedEventArgs e)
        {
            StartPlayback();
        }

        private void bnStop_Click(object sender, RoutedEventArgs e)
        {
            StopPlayback();
        }

        private void Connection_Disconnected(Connection sender)
        {
            CloseConnection();
        }

        private void CloseConnection()
        {
            StopPlayback();

            this.bnStart.IsEnabled = false;

            this.bnConnect.IsEnabled = true;
            this.bnClose.IsEnabled = false;

            if (this.connection != null)
            {
                this.connection.Disconnected -= Connection_Disconnected;
                this.connection.Uninitialize();
                this.connection = null;
            }

            if (this.connector != null)
            {
                this.connector.Uninitialize();
                this.connector = null;
            }
        }

        private void StartPlayback()
        {
            this.videoPlayer.Source = new Uri(string.Format("mrvc://{0}:{1}", this.txAddress.Text, this.txPort.Text));

            this.bnStart.IsEnabled = false;
            this.bnStop.IsEnabled = true;
        }

        private void StopPlayback()
        {
            this.videoPlayer.Stop();
            this.videoPlayer.Source = null;

            this.bnStart.IsEnabled = true;
            this.bnStop.IsEnabled = false;
        }

        public void RegisterSchemeHandler(PropertySet propertySet)
        {
            if (this.mediaExtensionManager == null)
            {
                this.mediaExtensionManager = new MediaExtensionManager();
            }

            this.mediaExtensionManager.RegisterSchemeHandler("MixedRemoteViewCompositor.Media.MrvcSchemeHandler", "mrvc:", propertySet);
        }


        //private void Canvas_ClickPosition(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        //{
        //    Point3D holoPoint = HoloPoints.Instance.GetHoloPoint((float)e.GetCurrentPoint(PointerCanvas).Position.X, (float)e.GetCurrentPoint(PointerCanvas).Position.Y);
        //    DebugTxt.Text = holoPoint.x.ToString() + "  " + holoPoint.y.ToString() + "  " + holoPoint.z.ToString();
        //    PostHoloPoints(holoPoint);
        //    clickTimer.Start();

        //    DebugTxt.Text = "X :" + e.GetCurrentPoint(PointerCanvas).Position.X + "Y  " + e.GetCurrentPoint(PointerCanvas).Position.Y;
        //}

        bool moved;
        bool clicked;

        ArrayList holoPointArr = new ArrayList();

        private void Canvas_Moved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (clicked == true)
            {
                moved = true;
                Point3D holoPoint = HoloPoints.Instance.GetHoloPoint((float)e.GetCurrentPoint(PointerCanvas).Position.X, (float)e.GetCurrentPoint(PointerCanvas).Position.Y);
                if (holoPoint != null)
                {
                    holoPointArr.Add(holoPoint);
                }
            }

        }


        private void Canvas_Pressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            moved = false;
           
            //clickTimer.Start();
            clicked = true;

            holoPointArr.Clear();

            
            //DebugTxt.Text = "X :" + e.GetCurrentPoint(PointerCanvas).Position.X + "Y  " + e.GetCurrentPoint(PointerCanvas).Position.Y;
        }



        private void Canvas_Released(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if(!moved)
            {

                Point3D holoPoint = HoloPoints.Instance.GetHoloPoint((float)e.GetCurrentPoint(PointerCanvas).Position.X, (float)e.GetCurrentPoint(PointerCanvas).Position.Y);

                if (holoPoint != null)
                {
                    PostHoloPoints(holoPoint);
                }

                DebugTxt.Text = "X :" + e.GetCurrentPoint(PointerCanvas).Position.X + " Y  " + e.GetCurrentPoint(PointerCanvas).Position.Y + " holoPoint.x:  " + holoPoint.x+ " holoPoint.y:  " + holoPoint.y+ " holoPoint.z:  " + holoPoint.z;
            }
            else
            {
                if (holoPointArr.Count > 0)
                {
                    PostHoloAnatationPoints();
                }
            }
            
            //DebugTxt.Text = "Released " + moved.ToString() + "  " + clicked.ToString();

            moved = false;
            clicked = false;
            
            //clickTimer.Stop();
            interval = 0;
        }

        int interval;
       
        private void ClickTimer_Fun(object sender, object e)
        {

            //DebugTxt.Text = interval++.ToString();
        }

        private async void PostHoloAnatationPoints()
        {
            Dictionary<string, string>[] holoPtsDictArray = new Dictionary<string, string>[holoPointArr.Count];

            int i = 0;
            foreach (Point3D pt in holoPointArr)
            {
                Dictionary<string, string> holopointsAnaDict = new Dictionary<string, string>();
                holopointsAnaDict.Add("NearX", pt.x.ToString());
                holopointsAnaDict.Add("NearY", pt.y.ToString());
                holopointsAnaDict.Add("NearZ", pt.z.ToString());
                holoPtsDictArray[i] = holopointsAnaDict;
                i++;
            }

            Dictionary<string, Dictionary<string, string>[]> holoAnaFinalDict = new Dictionary<string, Dictionary<string, string>[]>();

            holoAnaFinalDict.Add("pointsArray", holoPtsDictArray);

            string jsonObject = JsonConvert.SerializeObject(holoAnaFinalDict);

            //DebugTxt.Text = jsonObject;

            var uri = new System.Uri("https://holotest-6cd6b.firebaseio.com/MobiAnnotationPoints.json");
            //var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var stringContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Always catch network exceptions for async methods
                try
                {
                    var response = await httpClient.PostAsync(uri, stringContent);
                }
                catch (Exception ex)
                {
                    DebugTxt.Text = ex.Message.ToString();
                }
            }
        }

        private async void PostHoloPoints(Point3D point3D)
        {
            Dictionary<string, string> holopointsDict = new Dictionary<string, string>();
            holopointsDict.Add("FarX", "0");
            holopointsDict.Add("FarY", "0");
            holopointsDict.Add("FarZ", "0");
            holopointsDict.Add("NearX", point3D.x.ToString());
            holopointsDict.Add("NearY", point3D.y.ToString());
            holopointsDict.Add("NearZ", point3D.z.ToString());
            holopointsDict.Add("TimeStamp", "123");
            holopointsDict.Add("SelectedTool", ToolIDTxt.Text);

            string jsonObject = JsonConvert.SerializeObject(holopointsDict);
            

            var uri = new System.Uri("https://holotest-6cd6b.firebaseio.com/MobiPoints.json");
            //var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var stringContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Always catch network exceptions for async methods
                try
                {
                    var response = await httpClient.PostAsync(uri,stringContent);
                }
                catch (Exception ex)
                {
                    DebugTxt.Text = ex.Message.ToString();
                }
            }
        }



        private async void GetHoloPoints(object sender, object e)
        {
            var uri = new System.Uri("https://holotest-6cd6b.firebaseio.com/HoloPoints.json");
            using (var httpClient = new Windows.Web.Http.HttpClient())
            {
                // Always catch network exceptions for async methods
                try
                {
                    //string result = "{\"FarBottomLeftX\":\" - 0.53\",\"FarBottomLeftY\":\" - 0.30\",\"FarBottomLeftZ\":\"2.08\",\"FarBottomRightX\":\"0.53\",\"FarBottomRightY\":\" - 0.30\",\"FarBottomRightZ\":\"2.08\",\"FarTopLeftX\":\" - 0.53\",\"FarTopLeftY\":\"0.30\",\"FarTopLeftZ\":\"2.08\",\"FarTopRightX\":\"0.53\",\"FarTopRightY\":\"0.30\",\"FarTopRightZ\":\"2.08\",\"NearBottomLeftX\":\" - 0.53\",\"NearBottomLeftY\":\" - 0.30\",\"NearBottomLeftZ\":\"0.00\",\"NearBottomRightX\":\"0.53\",\"NearBottomRightY\":\" - 0.30\",\"NearBottomRightZ\":\"0.00\",\"NearTopLeftX\":\" - 0.53\",\"NearTopLeftY\":\"0.30\",\"NearTopLeftZ\":\"0.00\",\"NearTopRightX\":\"0.53\",\"NearTopRightY\":\"0.30\",\"NearTopRightZ\":\"0.00\"}";//await httpClient.GetStringAsync(uri);
                    string result = await httpClient.GetStringAsync(uri);

                    Dictionary<string, Dictionary<string, string>> parentDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(result);

                    Dictionary<string, string> childDict = parentDict.Values.First();

                    WebServiceTxt.Text = childDict["FarBottomLeftX"];

                    Point3D topLeftpt=new Point3D();
                    topLeftpt.x= float.Parse(childDict["NearTopLeftX"]);
                    topLeftpt.y = float.Parse(childDict["NearTopLeftY"]);
                    topLeftpt.z = float.Parse(childDict["NearTopLeftZ"]);

                    Point3D topRightpt=new Point3D();
                    topRightpt.x = float.Parse(childDict["NearTopRightX"]);
                    topRightpt.y = float.Parse(childDict["NearTopRightY"]);
                    topRightpt.z = float.Parse(childDict["NearTopRightZ"]);

                    Point3D bottomLeftpt = new Point3D();
                    bottomLeftpt.x = float.Parse(childDict["NearBottomLeftX"]);
                    bottomLeftpt.y = float.Parse(childDict["NearBottomLeftY"]);
                    bottomLeftpt.z = float.Parse(childDict["NearBottomLeftZ"]);

                    Point3D bottomRightpt = new Point3D();
                    bottomRightpt.x = float.Parse(childDict["NearBottomRightX"]);
                    bottomRightpt.y = float.Parse(childDict["NearBottomRightY"]);
                    bottomRightpt.z = float.Parse(childDict["NearBottomRightZ"]);

                    DebugTxt.Text += " topLeftpt.x:  " + topLeftpt.x + " topLeftpt.y:   " + topLeftpt.y + " topLeftpt.z:   " + topLeftpt.z + "  " +
                        " topRightpt.x:  " + topRightpt.x + " topRightpt.y:  " + topRightpt.y + " topRightpt.z:  " + topRightpt.z + "  " +
                        " bottomLeftpt.x:  " + bottomLeftpt.x + " bottomLeftpt.y:  " + bottomLeftpt.y + " bottomLeftpt.z:  " + bottomLeftpt.z + "  " +
                        " bottomRightpt.x:  " + bottomRightpt.x + " bottomRightpt.y:  " + bottomRightpt.y + " bottomRightpt.z:  " + bottomRightpt.z;



                   HoloPoints.Instance.topLeftpt = topLeftpt;
                    HoloPoints.Instance.topRightpt = topRightpt;
                    HoloPoints.Instance.bottomLeftpt = bottomLeftpt;
                    HoloPoints.Instance.bottomRightpt = bottomRightpt;


                    Windows.Web.Http.HttpResponseMessage response = await httpClient.DeleteAsync(uri);

                }
                catch (Exception ex)
                {
                    WebServiceTxt.Text = ex.Message;
                }
            }

        }

    }
}
