﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RFACleaner
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowVM mwVM;

        public MainWindow()
        {
            InitializeComponent();
            mwVM = new MainWindowVM();
            this.DataContext = mwVM;
        }

        private void ShutDownButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MamimizeButtonImage.Source = ResxBitmap(Properties.Resources.window_maximize);
                MainGrid.Margin = new Thickness(0);
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MamimizeButtonImage.Source = ResxBitmap(Properties.Resources.window_restore);
                MainGrid.Margin = new Thickness(10);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Minimized;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mwVM.mwm.Select((sender as Button).Uid);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private BitmapSource ResxBitmap(Bitmap img)
        {
            BitmapSource btmSrc;

            btmSrc = Imaging.CreateBitmapSourceFromHBitmap(
                img.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return btmSrc;
        }
    }
}
