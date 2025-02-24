﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
using Emgu.CV;
using Emgu.Util;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Windows.Controls.Image;

namespace PhotoBooth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Index of the main tab
        /// </summary>
        enum eTab
        {
            startup,
            capture,
            results,
            settings
        }

        /// <summary>
        /// Async worker for capturing images and counting down
        /// </summary>
        BackgroundWorker bgCapture;

        /// <summary>
        /// Custom object with index and image
        /// </summary>
        BitmapImage[] images;

        /// <summary>
        /// Capture boxes
        /// </summary>
        public Image[] captBoxes;

        /// <summary>
        /// Test pictures to be used until camera is integrated
        /// </summary>
        string[] test_pics = new string[]
        {
            @"C:\Users\brlno\Documents\Code\Programs\Photobooth\skogafoss6.png",
            @"C:\Users\brlno\Documents\Code\Programs\Photobooth\sunrise.png",
            @"C:\Users\brlno\Documents\Code\Programs\Photobooth\gulfoss6.png",
            @"C:\Users\brlno\Documents\Code\Programs\Photobooth\blank.png"
        };

        /// <summary>
        /// Delay between taking pictures. Can be updated on setting screen
        /// </summary>
        int picture_delay = 5;

        /// <summary>
        /// Number of pictures to take
        /// </summary>
        int num_of_pictures = 3;

        /// <summary>
        /// Number of frames per second to display in live feed
        /// </summary>
        int display_fps = 8;

        /// <summary>
        /// Camera index to use
        /// </summary>
        int display_camera = 0;

        /// <summary>
        /// Number of pictures to print
        /// </summary>
        int num_prints = 1;

        VideoCapture camera;

        System.Drawing.Size displaySize = new System.Drawing.Size(712, 480);

        public MainWindow()
        {
            InitializeComponent();

            tbPicDelay.Text = picture_delay.ToString();
            tbDisplayFps.Text = display_fps.ToString();
            tbCameraIndex.Text = display_camera.ToString();

            //Setup the array of Image boxes on the capture screen
            captBoxes = new Image[]
            {
                imgCaptOne,
                imgCaptTwo,
                imgCaptThree,
            };
        }

        /// <summary>
        /// Start the capture sequence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartPictures_Click(object sender, RoutedEventArgs e)
        {
            //Setup camera
            camera = new VideoCapture(display_camera); //, VideoCapture.API.DShow
            camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
            camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

            //Change to capture screen
            tcMainTabs.SelectedIndex = (int)eTab.capture;

            //Setup BackgroundWorker
            bgCapture = new BackgroundWorker();
            bgCapture.WorkerReportsProgress = true;
            bgCapture.DoWork += BgCapture_DoWork;
            bgCapture.ProgressChanged += BgCapture_ProgressChanged;
            bgCapture.RunWorkerCompleted += BgCapture_RunWorkerCompleted;

            images = new BitmapImage[num_of_pictures];

            //Start countdown and capture
            camera.Start();
            bgCapture.RunWorkerAsync();
        }

        /// <summary>
        /// Display and format the results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgCapture_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            camera.Stop();

            tcMainTabs.SelectedIndex = (int)eTab.results;

            num_prints = 1;
            lblNumPrints.Content = num_prints;

            imgResOne.Source = images[0];
            imgResTwo.Source = images[1];
            imgResThree.Source = images[2];
        }

        /// <summary>
        /// Update timer text and images on the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgCapture_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == -1)
            {
                lblCountdown.Content = "Finished!";
            }
            else if (e.ProgressPercentage / display_fps < 1.0)
            {
                lblCountdown.Content = "Smile!";
            }
            else
            {
                lblCountdown.Content = (e.ProgressPercentage / display_fps).ToString();
            }
            

            //Convert to Photo object to check if picture was taken
            Photo result = e.UserState as Photo;

            if(result != null)
            {
                if(result.Index == -1)
                {
                    imgCaptMain.Source = result.Image.Clone();
                }
                else
                {
                    //Display the image
                    images[result.Index] = result.Image.Clone();
                    captBoxes[result.Index].Source = result.Image.Clone();
                }
            }
        }

        /// <summary>
        /// Perform countdown and take the pictures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgCapture_DoWork(object sender, DoWorkEventArgs e)
        {
            int num_pics = 0;

            int timer; //time between pics

            //Take 3 pictures
            while(num_pics < num_of_pictures)
            {
                timer = (picture_delay + 1) * display_fps - 1; //Reset timer

                //Report timer, do not display image
                bgCapture.ReportProgress(timer, null);

                Mat frame = new Mat();
                Bitmap img = null;
                Bitmap imgSmall = null;

                //Delay between picture
                while(timer > 0)
                {
                    timer--; //Decrease timer
                    Console.WriteLine(timer);

                    Thread.Sleep(1000 / display_fps); //Delay

                    if(camera.Retrieve(frame)) 
                    {
                        //Display frame on interface, do not save
                        img = frame.ToBitmap();
                        imgSmall = new Bitmap(img, displaySize);
                        Photo mainDisplay = new Photo(-1, imgSmall);
                        bgCapture.ReportProgress(timer, mainDisplay);
                    }
                    else
                    {
                        //Report timer, do not display image
                        bgCapture.ReportProgress(timer, null);
                    }
                }

                //Capture and save photo
                //Photo result = new Photo(num_pics, new BitmapImage(new Uri(test_pics[num_pics])));
                Photo result = new Photo(num_pics, img);
                bgCapture.ReportProgress(timer, result);

                num_pics++; //Picture taken, continue

                Thread.Sleep(500);
            }

            bgCapture.ReportProgress(-1, null);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Decrease the number of prints
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDecreasePrints_Click(object sender, RoutedEventArgs e)
        {
            num_prints = num_prints > 1 ? num_prints-1 : 1;
            lblNumPrints.Content = num_prints;
        }

        /// <summary>
        /// Increase the number of prints
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIncreasePrints_Click(object sender, RoutedEventArgs e)
        {
            num_prints++;
            lblNumPrints.Content = num_prints;
        }

        /// <summary>
        /// Shortcuts for changing screens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch(e.Key)
            {
                case Key.D1:
                    tcMainTabs.SelectedIndex = (int)eTab.startup;
                    break;
                case Key.D2:
                    tcMainTabs.SelectedIndex = (int)eTab.capture;
                    break;
                case Key.D3:
                    tcMainTabs.SelectedIndex = (int)eTab.results;
                    break;
                case Key.D4:
                    tcMainTabs.SelectedIndex = (int)eTab.settings;
                    break;
            }
            }
        }

        /// <summary>
        /// Apply the values on the settings screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int new_delay;
            int new_fps;

            if(int.TryParse(tbPicDelay.Text, out new_delay))
            {
                picture_delay = new_delay;
            }
            else
            {
                tbPicDelay.Text = picture_delay.ToString();
            }

            if (int.TryParse(tbDisplayFps.Text, out new_fps))
            {
                display_fps = new_fps;
            }
            else
            {
                tbDisplayFps.Text = display_fps.ToString();
            }
        }

        private void btnTestCamera_Click(object sender, RoutedEventArgs e)
        {
            Mat frame = new Mat();
            BitmapImage img = new BitmapImage();

            imgCaptTest.Source = null;

            int cameraIndex = 0;

            if(int.TryParse(tbCameraIndex.Text, out cameraIndex))
            {
                try
                {
                    //Setup camera
                    VideoCapture cameraTest = new VideoCapture(cameraIndex); //, VideoCapture.API.DShow

                    cameraTest.Start();

                    //Wait for camera to initialize
                    Thread.Sleep(5000);

                    if (cameraTest.Retrieve(frame))
                    {
                        if (frame.ToBitmap() != null)
                        {
                            using (var memory = new MemoryStream())
                            {
                                frame.ToBitmap().Save(memory, ImageFormat.Png);
                                memory.Position = 0;

                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze();

                                img = bitmapImage;
                            }

                            imgCaptTest.Source = img;
                        }
                    }

                    cameraTest.Stop();
                    cameraTest.Dispose();

                    display_camera = cameraIndex;
                }
                catch { }
            }
            else
            {
                tbCameraIndex.Text = display_camera.ToString();
            }

        }

        /*private BitmapImage CombineImages(BitmapImage[] images)
        {
            int width = (int)images[0].Width; //Assume all the same width
            int height = (int)images.Sum(i => i.Height); //Combine the height of all of them

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(images[0], 0, 0);
                g.DrawImage(images[2], image1.Width, 0);
            }

            return bitmap;
        }*/
    }
}
