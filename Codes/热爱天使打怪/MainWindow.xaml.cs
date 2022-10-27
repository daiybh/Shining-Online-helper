﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 热爱天使打怪
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Thread clickThread = null;
        bool m_Running = true;
        public MainWindow()
        {
            InitializeComponent();
            clickThread = new Thread(calltoClickThread);
            clickThread.Start();
        }

        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_Running = false;
            clickThread.Join();
        }

        IntPtr gameWidnow = IntPtr.Zero;
        void findGameWindow()
        {
            gameWidnow = WindowAPIHelper.FindWindow("SlOnline", "重生");
            if (gameWidnow == IntPtr.Zero) this.Title = "can not found";
            else
                this.Title = $"{gameWidnow} --fond";

            tbResult.Text = this.Title;
        }
        Bitmap bitmap1=null, bitmap2;

        Int32 curPos = 0;
        void takePhoto()
        {
            var image = WindowAPIHelper.GetShotCutImage(gameWidnow);
            var bitmapImage = BitmapConveters.ConvertToBitmapImage(image);
            if (curPos++ % 2 == 0)
            {
                myImage.Source = bitmapImage;
                bitmap1 = image;
            }
            else
            {
                myImage2.Source = bitmapImage;
                bitmap2 = image;
            }
        }
        public Bitmap changeColor(Bitmap imgObj, SortedDictionary<int, List<System.Drawing.Rectangle>> vlist)
        {
            System.Drawing.Color color2 = System.Drawing.Color.Red;
            string xText = "";
            Dictionary<int, List<int>> newList = new Dictionary<int, List<int>>();
            int ia = 0;
            List<int> xList = new List<int>();
            int lastX = -1;
            foreach (var mapItem in vlist)
            {
                xText += $"{ia}--{mapItem.Key}-> \n";
                if(lastX==-1)
                {
                    lastX = mapItem.Key;
                    xList.Add(mapItem.Key);
                    continue;
                }
                if (mapItem.Key - lastX > 20)
                {
                    if (xList.Count > 4)
                        newList[mapItem.Key] = xList;
                    xList.Clear();
                }
                xList.Add(mapItem.Key);
                lastX = mapItem.Key;
                ia++;
            }
            xText+= "aa"+newList.Count().ToString();
            tbResult.Text = xText;
            foreach (var mapItem in vlist)
            {
                foreach (var v in mapItem.Value)
                {
                    xText += $"{mapItem.Key}-> {v.X} {v.Y} {v.Width} {v.Height}\n";
                    for (int i = v.X; i < v.Right; i++)
                    {
                        if (i < imgObj.Width && v.Y < imgObj.Height)
                            imgObj.SetPixel(i, v.Y, color2);
                        if (i < imgObj.Width && v.Bottom < imgObj.Height)
                            imgObj.SetPixel(i, v.Bottom, color2);
                        
                    }
                    for (int h = v.Y; h < v.Bottom; h++)
                    {
                        if (v.X < imgObj.Width && h < imgObj.Height)
                            imgObj.SetPixel(v.X, h, color2);
                        if (v.Right < imgObj.Width && h < imgObj.Height)
                            imgObj.SetPixel(v.Right, h, color2);
                    }
                }
            }
           
            return imgObj;
        }
        SortedDictionary<int, List<System.Drawing.Rectangle>> rectangles = null;
        void compareTwoPic()
        {
            if (curPos < 2) return;
            var aw1 = myImage.Source.Width;
            var ah1 = myImage.Source.Height;
            var aw2 = myImage2.Source.Width;
            var ah2 = myImage2.Source.Height;


            ComparePicture cp = new ComparePicture();

            rectangles = cp.compareTwo(bitmap1, bitmap2);

            myImage2.Source = BitmapConveters.ConvertToBitmapImage(changeColor(bitmap2, rectangles));
        }
        void findValidRect(List<System.Drawing.Rectangle> _rectangles)
        {

        }
        
        void calltoClickThread()
        {
            int offset = 100;
            while (m_Running )
            {
                if (bitmap1 != null)
                {
                    int x = bitmap1.Width / 2;
                    int y = bitmap1.Height / 2;
                    for(int i=x- offset; m_Running && i <x+ offset; i+=5)
                    {
                        for (int j = y - offset; m_Running&& j < y + offset; j += 5)
                        {
                            WindowAPIHelper.OnClickRButton(gameWidnow, i, j);
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        private IntPtr hWndOriginalParent;
        private void onRBTN(object sender, RoutedEventArgs e)
        {
            WindowAPIHelper.OnClickRButton(gameWidnow, 300, 300);

        }

        private void onsetParent(object sender, RoutedEventArgs e)
        {
            if(gameWidnow==null)
                gameWidnow = WindowAPIHelper.FindWindow("notepad", null);
            this.Title = $"{gameWidnow}";
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            hWndOriginalParent  = WindowAPIHelper.SetParent(gameWidnow, windowHandle);

        }

        private void onUnsetParent(object sender, RoutedEventArgs e)
        {
            if(gameWidnow!=null)
            WindowAPIHelper.SetParent(gameWidnow, hWndOriginalParent);
        }

        private void onFindWindowBTN(object sender, RoutedEventArgs e)
        {
            findGameWindow();
            if (gameWidnow == IntPtr.Zero) return;
            takePhoto();
            compareTwoPic();
            
            //this.Title = new WindowInteropHelper(this).Handle.ToString();
            //IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            //WindowFinder.SetParent(fwind, windowHandle);
        }
    }
}
