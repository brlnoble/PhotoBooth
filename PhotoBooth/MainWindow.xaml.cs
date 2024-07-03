using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        Photo[] images;

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
        /// Number of pictures to print
        /// </summary>
        int num_prints = 1;

        public MainWindow()
        {
            InitializeComponent();

            tbPicDelay.Text = picture_delay.ToString();

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
            //Change to capture screen
            tcMainTabs.SelectedIndex = (int)eTab.capture;

            //Setup BackgroundWorker
            bgCapture = new BackgroundWorker();
            bgCapture.WorkerReportsProgress = true;
            bgCapture.DoWork += BgCapture_DoWork;
            bgCapture.ProgressChanged += BgCapture_ProgressChanged;
            bgCapture.RunWorkerCompleted += BgCapture_RunWorkerCompleted;

            images = new Photo[num_of_pictures];

            //Clear all currently displayed images
            for (int i = 0; i< num_of_pictures; i++)
            {
                captBoxes[i].Source = new BitmapImage(new Uri(test_pics[3]));
            }

            //Start countdown and capture
            bgCapture.RunWorkerAsync();
        }

        /// <summary>
        /// Display and format the results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgCapture_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tcMainTabs.SelectedIndex = (int)eTab.results;

            imgResOne.Source = images[0].Image;
            imgResTwo.Source = images[1].Image;
            imgResThree.Source = images[2].Image;

            num_prints = 1;
            lblNumPrints.Content = num_prints;
        }

        /// <summary>
        /// Update timer text and images on the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgCapture_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch(e.ProgressPercentage.ToString())
            {
                case "0":
                    lblCountdown.Content = "Smile!";
                    break;
                case "100":
                    lblCountdown.Content = "Finished!";
                    break;
                default:
                    lblCountdown.Content = e.ProgressPercentage.ToString();
                    break;
            }

            //Convert to Photo object to check if picture was taken
            Photo result = e.UserState as Photo;

            if(result != null)
            {
                //Display the image
                captBoxes[result.Index].Source = result.Image;
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
                timer = picture_delay; //Reset timer

                //Report timer, do not display image
                bgCapture.ReportProgress(timer, null);

                //Delay between picture
                while(timer > 0)
                {
                    timer--; //Decrease timer

                    Thread.Sleep(1000); //Delay

                    //Report timer, do not display image
                    bgCapture.ReportProgress(timer, null);
                }

                //Capture and save photo
                Photo result = new Photo(num_pics, new BitmapImage(new Uri(test_pics[num_pics])));
                images[num_pics] = result;

                bgCapture.ReportProgress(timer, result);
                num_pics++; //Picture taken, continue

                Thread.Sleep(1000);
            }

            bgCapture.ReportProgress(100, null);
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
        /// Change the delay on the settings screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetDelay_Click(object sender, RoutedEventArgs e)
        {
            int new_delay;

            if(int.TryParse(tbPicDelay.Text, out new_delay))
            {
                picture_delay = new_delay;
            }
            else
            {
                tbPicDelay.Text = picture_delay.ToString();
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
