﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace NesTry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Famicom famicom;
        private UInt32[] g_bg_data = new UInt32[256 * 256 + 256];
        private Image render = new Image();
        //Create the BitmapSource (a WriteableBitmap). 96 is the dpi setting.
        private WriteableBitmap backgroundBMP =
            new WriteableBitmap(256, 240, 96, 96, PixelFormats.Pbgra32, null);
        private bool IsRunning = true;
        private int Interval = 1;
        public MainWindow()
        {
            InitializeComponent();
            render.Source = backgroundBMP;
            RenderOptions.SetBitmapScalingMode(render, BitmapScalingMode.NearestNeighbor);
            GameArea.Children.Add(render);

            famicom = new Famicom();

            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            UpdateGame();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        { 
            famicom.User_Input(e.Key, 0);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            famicom.User_Input(e.Key, 1);
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsRunning = false;
        }
        [DllImport("kernel32", EntryPoint = "RtlMoveMemory", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
        private unsafe void CompositionTarget_Rendering(object sender, EventArgs e)
        {  
            famicom.main_loop(ref g_bg_data);
            backgroundBMP.Lock();
            Parallel.Invoke(() =>
            {
                fixed (UInt32* ptr = g_bg_data)
                {
                    var p = new IntPtr(ptr);
                    CopyMemory(backgroundBMP.BackBuffer, new IntPtr(ptr), (uint)256 * 240 * 4);
                }
            });
            backgroundBMP.AddDirtyRect(new Int32Rect(0, 0, backgroundBMP.PixelWidth, backgroundBMP.PixelHeight));
            backgroundBMP.Unlock();
        }
        private unsafe void UpdateGame()
        {
            new Thread(() =>
            {
                while (this.IsRunning)
                {
                    famicom.main_loop(ref g_bg_data);
                    GameArea.Dispatcher.Invoke(() =>
                    {
                        backgroundBMP.Lock();
                        fixed (UInt32* ptr = g_bg_data)
                        {
                            var p = new IntPtr(ptr);
                            CopyMemory(backgroundBMP.BackBuffer, new IntPtr(ptr), (uint)256 * 240 * 4);
                        }
                        backgroundBMP.AddDirtyRect(new Int32Rect(0, 0, backgroundBMP.PixelWidth, backgroundBMP.PixelHeight));
                        backgroundBMP.Unlock();
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                    SpinWait.SpinUntil(() => !this.IsRunning, this.Interval);
                }
            }).Start();
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Nes files (*.nes)|*.nes|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                famicom.LoadRom(openFileDialog.FileName);
                famicom.Reset();
            }
        }
    }
}