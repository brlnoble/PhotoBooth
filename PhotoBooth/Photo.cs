using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoBooth
{
    /// <summary>
    /// Custom object containing an Index and Image
    /// </summary>
    internal class Photo
    {
        int index;
        BitmapImage image;

        /// <summary>
        /// Create new Photo object
        /// </summary>
        /// <param name="i">Index of the picture sequence</param>
        /// <param name="img">Image captured</param>
        public Photo(int i, BitmapImage img)
        {
            index = i;
            image = img;
            image.Freeze(); //Allow access from multiple threads
        }

        /// <summary>
        /// Get the Index of this image in the picture sequence
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Get the Image that was captured
        /// </summary>
        public BitmapImage Image
        {
            get { return image; }
        }
    }
}
