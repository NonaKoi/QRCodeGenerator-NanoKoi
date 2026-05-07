using Microsoft.Win32;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;



namespace QRCodeGenerator

{

    public partial class MainWindow : Window

    {



        private Bitmap _qrCodeBitmap;



        private bool _isTextMode = true;



        public MainWindow()

        {

            InitializeComponent();

        }



        private void ContentType_Changed(object sender, RoutedEventArgs e)

        {

            if (rbText == null || rbImage == null) return;



            _isTextMode = rbText.IsChecked == true;





            txtInput.Visibility = _isTextMode ? Visibility.Visible : Visibility.Collapsed;

            btnSelectImage.Visibility = _isTextMode ? Visibility.Collapsed : Visibility.Visible;



            if (_isTextMode)

            {

                txtStatus.Text = "文字/链接模式";

            }

        }





        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)

        {

            OpenFileDialog openFileDialog = new OpenFileDialog

            {

                Title = "选择图片文件",

                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件|*.*",

                FilterIndex = 1

            };



            if (openFileDialog.ShowDialog() == true)

            {



                txtInput.Text = openFileDialog.FileName;

                txtStatus.Text = $"已选择图片: {Path.GetFileName(openFileDialog.FileName)}";

            }

        }



        private void BtnGenerate_Click(object sender, RoutedEventArgs e)

        {

            try

            {



                string content = GetContent();

                if (string.IsNullOrEmpty(content))

                {

                    MessageBox.Show("请输入内容或选择图片文件！", "提示",

                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;

                }




                int size = int.Parse(((ComboBoxItem)cmbSize.SelectedItem).Content.ToString());




                GenerateQRCode(content, size);





                DisplayQRCode();





                btnSave.IsEnabled = true;

                txtStatus.Text = "二维码生成成功！";

            }

            catch (Exception ex)

            {

                MessageBox.Show($"生成二维码失败：{ex.Message}", "错误",

                    MessageBoxButton.OK, MessageBoxImage.Error);

                txtStatus.Text = "生成失败";

            }

        }




        private string GetContent()

        {

            if (_isTextMode)

            {



                string text = txtInput.Text.Trim();

                if (text == "在此输入文字或链接...")

                    return string.Empty;

                return text;

            }

            else

            {

               

                string imagePath = txtInput.Text.Trim();

                if (!File.Exists(imagePath))

                    return string.Empty;



                

                using (Bitmap originalImage = new Bitmap(imagePath))

                {

                    

                    int maxSize = 200;

                    Bitmap resizedImage = ResizeImage(originalImage, maxSize);



                    using (MemoryStream ms = new MemoryStream())

                    {

                     

                        resizedImage.Save(ms, ImageFormat.Jpeg);

                        byte[] imageBytes = ms.ToArray();

                        return Convert.ToBase64String(imageBytes);

                    }

                }

            }

        }




        private Bitmap ResizeImage(Bitmap original, int maxSize)

        {

      
            double scale = Math.Min(

                (double)maxSize / original.Width,

                (double)maxSize / original.Height

            );



            int newWidth = (int)(original.Width * scale);

            int newHeight = (int)(original.Height * scale);



            Bitmap resized = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(resized))

            {

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(original, 0, 0, newWidth, newHeight);

            }

            return resized;

        }



       

        private void GenerateQRCode(string content, int size)

        {

       

            using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())

            {

             

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.M);



            
                using (QRCode qrCode = new QRCode(qrCodeData))

                {

                 

                    _qrCodeBitmap = qrCode.GetGraphic(20, Color.Black, Color.White, true);

                }

            }

        }



     

        private void DisplayQRCode()

        {

            if (_qrCodeBitmap == null) return;



        

            using (MemoryStream memory = new MemoryStream())

            {

                _qrCodeBitmap.Save(memory, ImageFormat.Png);

                memory.Position = 0;



                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();

                bitmapImage.StreamSource = memory;

                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                bitmapImage.EndInit();



        

                imgQRCode.Source = bitmapImage;

       

                txtPlaceholder.Visibility = Visibility.Collapsed;

            }

        }




        private void BtnSave_Click(object sender, RoutedEventArgs e)

        {

            if (_qrCodeBitmap == null)

            {

                MessageBox.Show("请先生成二维码！", "提示",

                    MessageBoxButton.OK, MessageBoxImage.Warning);

                return;

            }



            SaveFileDialog saveFileDialog = new SaveFileDialog

            {

                Title = "保存二维码图片",

                Filter = "PNG图片|*.png|JPEG图片|*.jpg|位图文件|*.bmp",

                FilterIndex = 1,

                FileName = "QRCode_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")

            };



            if (saveFileDialog.ShowDialog() == true)

            {

                try

                {

              

                    ImageFormat format = ImageFormat.Png;

                    switch (saveFileDialog.FilterIndex)

                    {

                        case 1:

                            format = ImageFormat.Png;

                            break;

                        case 2:

                            format = ImageFormat.Jpeg;

                            break;

                        case 3:

                            format = ImageFormat.Bmp;

                            break;

                    }



                    _qrCodeBitmap.Save(saveFileDialog.FileName, format);

                    txtStatus.Text = $"二维码已保存到: {saveFileDialog.FileName}";

                    MessageBox.Show("保存成功！", "提示",

                        MessageBoxButton.OK, MessageBoxImage.Information);

                }

                catch (Exception ex)

                {

                    MessageBox.Show($"保存失败：{ex.Message}", "错误",

                        MessageBoxButton.OK, MessageBoxImage.Error);

                }

            }

        }

    }

}
