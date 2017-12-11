using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using System.Windows;


namespace HicapsConnectClient12
{
    public class ItemHttpSource
    {
        private readonly string apiUrl = Properties.Settings.Default.ApiBaseAddress;
        private readonly string apiItemsPath = Properties.Settings.Default.ApiItemsPath;

        public delegate void DownloadCompletedHandler(object sender, DownloadCompletedEventArgs e);
        public event DownloadCompletedHandler DownloadCompleted;
        
        public ItemHttpSource()
        {
        }

        public void StartUpdate()
        {
            // create an action to do the update (in another thread since 
            // BeginGetResponse is not entirely asynchronous)
            Action DoUpdate = () =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl + "/" + apiItemsPath);
                Debug.WriteLine("using proxy: " + request.Proxy.GetProxy(new Uri(apiUrl + "/" + apiItemsPath)));
                request.ContentType = "application/xml";
                try
                {
                    request.BeginGetResponse(new AsyncCallback(_DownloadCompleted), request);
                }
                catch {
                    Debug.WriteLine("Item download failed");
                }
                
            };

            // Invoke the action (runs in another thread) and set up 
            // an action to call EndInvoke.
            DoUpdate.BeginInvoke(new AsyncCallback((i) =>
            {
                var action = (Action)i.AsyncState;
                action.EndInvoke(i);
            }), DoUpdate);
            
        }

        // this gets called by the updater thread when the operation is completed
        public void _DownloadCompleted(IAsyncResult result)
        {
            DownloadCompletedEventArgs e = new DownloadCompletedEventArgs();
            try
            {
                HttpWebResponse response = (HttpWebResponse)((HttpWebRequest)result.AsyncState).EndGetResponse(result);
                string data = new StreamReader(response.GetResponseStream()).ReadToEnd();

                // parse out the json into a string of xml and add to the event args
                try
                {
                    Debug.WriteLine(data.Length + " characters received from API");
                    e.schedule = JsonConvert.DeserializeObject<Schedule>(data);

                    // fire the DownloadCompleted event, in the UI thread
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Debug.WriteLine("_DownloadCompleted: firing event!");
                            DownloadCompleted(this, e);
                        }));
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem parsing item API result");
                    throw ex;
                }
            }
            catch
            {
                Debug.WriteLine("item download failed");
            }
        }
    }

    public class DownloadCompletedEventArgs : EventArgs
    {
        public Schedule schedule { get; set; }
    }
}