using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfFSMonitorPOC {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            var nIcon = new NotifyIcon();
            nIcon.Icon = new System.Drawing.Icon("favicon.ico");
            nIcon.Visible = true;

            nIcon.DoubleClick += (sender, args) => {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
        }

        protected override void OnStateChanged( EventArgs e ) {
            if (WindowState == WindowState.Minimized) {
                this.Hide();
            }

            base.OnStateChanged( e );
        }

        protected override void OnClosing( CancelEventArgs e ) {
            this.Hide();
            e.Cancel = true;
        }

        private void browseButton_Click( object sender, RoutedEventArgs e ) {
            using (var browseDirDialog = new FolderBrowserDialog()) {
                DialogResult result = browseDirDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                    dirPathTextBox.Text = browseDirDialog.SelectedPath;
                }
            }
        }

        private void monitorButton_Click( object sender, RoutedEventArgs e ) {
            if (string.IsNullOrEmpty(dirPathTextBox.Text) || !Directory.Exists(dirPathTextBox.Text)) {
                System.Windows.MessageBox.Show("Please select a valid directory path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var fsWatcher = new FileSystemWatcher();
            fsWatcher.Path = dirPathTextBox.Text;

            fsWatcher.NotifyFilter = NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fsWatcher.IncludeSubdirectories = true;

            fsWatcher.Changed += new FileSystemEventHandler( OnChanged );
            fsWatcher.Created += new FileSystemEventHandler( OnChanged );
            fsWatcher.Deleted += new FileSystemEventHandler( OnChanged );
            fsWatcher.Renamed += new RenamedEventHandler( OnRenamed );
            

            fsWatcher.EnableRaisingEvents = true;

            outputTextBox.AppendText("Monitor started\n");
        }


        private void OnChanged( object sender, FileSystemEventArgs e ) {
            Dispatcher.Invoke(() => outputTextBox.AppendText( $"{DateTime.Now}: " + "File: " + e.FullPath + 
                " " + e.ChangeType  + "\n" ));
        }

        private void OnRenamed( object sender, RenamedEventArgs e ) {
            Dispatcher.Invoke( () => outputTextBox.AppendText( $"{DateTime.Now}: File: {e.OldFullPath} renamed to {e.FullPath }\n" ));
        }
    }
}
