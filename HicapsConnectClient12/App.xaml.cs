using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using Microsoft.Win32;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            bindHicapsConnectDLL();

            InitializeComponent();
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Window))
            });
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Page))
            });
        }
        private void bindHicapsConnectDLL()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromHicapsFolder);
        }

        static Assembly LoadFromHicapsFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = "";
            string value = (string) Registry.GetValue("HKEY_CURRENT_USER\\Software\\HicapsConnect","","");
            if (!string.IsNullOrEmpty(value)) { folderPath = value; }
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath) == false)
            {
                folderPath = @"c:\Program Files\HicapsConnect\";
                assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            }
            if (File.Exists(assemblyPath) == false)
            {
                folderPath = @"c:\Program Files (x86)\HicapsConnect\";
                assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            }
            if (File.Exists(assemblyPath) == false)
            {
                folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            }
            if (File.Exists(assemblyPath) == false)
            {
                return null;
            }
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

    }
}
