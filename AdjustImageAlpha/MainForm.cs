/*
  +MIT License
 +
 +Copyright (c) 2017 LiuKeith
 +
 +Permission is hereby granted, free of charge, to any person obtaining a copy
 +of this software and associated documentation files (the "Software"), to deal
 +in the Software without restriction, including without limitation the rights
 +to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 +copies of the Software, and to permit persons to whom the Software is
 +furnished to do so, subject to the following conditions:
 +
 +The above copyright notice and this permission notice shall be included in all
 +copies or substantial portions of the Software.
 +
 +THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 +IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 +FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 +AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 +LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 +OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 +SOFTWARE.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdjustImageAlpha
{
    public partial class MainForm : Form
    {
        private Bitmap imageBitmap; // Bitmap object for the Image

        protected Bitmap imageToSave;
        private ImageAttributes imageAttributes;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            trackBarImage.Enabled = false;
            btnSave.Enabled = false;

            pictureBoxImage.AllowDrop = true;
        }

        private void pictureBoxImage_Paint(object sender, PaintEventArgs e)
        {
            if (imageBitmap == null) return;
            Graphics g = e.Graphics;
            g.Clear(Color.Transparent);

            // Create a new color matrix with the alpha value set to the opacity specified in the slider
            ColorMatrix cm = new ColorMatrix();
            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
            cm.Matrix33 = (float)trackBarImage.Value / 100; // the matrix is of the form RGBA, where the (4,4)th element rep alpha

            // Create a new image attribute object and set the color matrix to the one you just created
            if (imageAttributes == null)
            {
                imageAttributes = new ImageAttributes();
            }
            imageAttributes.SetColorMatrix(cm);

            // Draw the original image with the image attributes specified
            g.DrawImage(imageBitmap, new Rectangle(0, 0, imageBitmap.Width, imageBitmap.Height), 0, 0, imageBitmap.Width, imageBitmap.Height, GraphicsUnit.Pixel, imageAttributes);
        }

        // Whenever the user changes the value of the sliders, call the respective pictureBox's Refresh()
        private void trackBarImage_Scroll(object sender, System.EventArgs e)
        {
            pictureBoxImage.Refresh();
        }

        private void LoadImage(string fileName)
        {
            imageBitmap = new Bitmap(fileName);
            trackBarImage.Enabled = true;
            btnSave.Enabled = true;
            trackBarImage.Value = 100;
            imageAttributes = new ImageAttributes();
            pictureBoxImage.Refresh();

        }

        private void pictureBoxImage_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = e.Data.GetData(DataFormats.FileDrop) as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is string))
                    {
                        string filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp"))
                        {
                            LoadImage(filename);
                        }
                    }
                }
            }
        }

        private void pictureBoxImage_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                SaveImageAsync(fileName);
            }
        }

        private async void SaveImageAsync(string fileName)
        {
            imageToSave = await Task.Run(() => GetCurrentImage());

            imageToSave.Save(fileName);
        }

        private Bitmap GetCurrentImage()
        {
            Bitmap output = new Bitmap(imageBitmap.Width, imageBitmap.Height);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(imageBitmap, new Rectangle(0, 0, imageBitmap.Width, imageBitmap.Height), 0, 0, imageBitmap.Width, imageBitmap.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            return output;
        }
    }
}
