using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using Affdex;
using Utility;

namespace WpfAffdex
{ 
    public partial class PhotoWindow : Window, Affdex.ImageListener
    { 
        private Affdex.PhotoDetector Detector { get; set; }
        public PhotoWindow()
        {
            InitializeComponent(); 
            uint maxNumFaces = 1;//最多识别图片中几张脸
            Detector = new Affdex.PhotoDetector(maxNumFaces, Affdex.FaceDetectorMode.SMALL_FACES);   
            Detector.setImageListener(this); 
            Detector.start(); 
            
            byte[] bytes = FileHelper.FileToBytes(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "timg.jpg"));
            BitmapSource bitmapSource = ImageHelper.BytesToBitmapImage(bytes); 
            var w = bitmapSource.Width;
            var h = bitmapSource.Height;
            var stride = bitmapSource.Format.BitsPerPixel * (int)w / 8; //计算Stride  
            byte[] byteList = new byte[(int)h * stride];
            bitmapSource.CopyPixels(byteList, stride, 0);   
            Affdex.Frame frame = new Affdex.Frame((int)w, (int)h, byteList, Affdex.Frame.COLOR_FORMAT.BGRA);  
            Detector.process(frame);
        }  
         
        public void onImageCapture(Affdex.Frame frame)
        {
            #region 使用一下代码测试帧捕获的图片是否可以正常生成图片，如果不能则是输入给帧的像素的数组有问题
            var len = frame.getBGRByteArrayLength();
            byte[] imageData = frame.getBGRByteArray();//这里捕获的数据 不同于生成该frame时的buff 并且是3通道的数据 
            int width = frame.getWidth();
            int height = frame.getHeight();
            var ColorFormat = frame.getColorFormat();

            if (imageData != null && imageData.Length > 0)
            {
                var _stride = (width * System.Windows.Media.PixelFormats.Rgb24.BitsPerPixel + 7) / 8;  
                var imageSrc = System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96d, 96d, System.Windows.Media.PixelFormats.Bgr24,
                    null, imageData, _stride);

                System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imageSrc)); 
                using (var stream =
                new System.IO.FileStream(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                    "我是分析前图片.png"), System.IO.FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
            #endregion
        }
        public void onImageResults(Dictionary<int, Face> faces, Affdex.Frame frame)
        {
            Face face = null;
            if (faces != null && faces.Values != null && faces.Values.Count() > 0)
            {
                face = faces.Values.First();//因为我们的Detector只识别了一个脸，所以这里最多只有一个数据
            }
            int top = (int)face.FeaturePoints.Min(r => r.X);
            int left = (int)face.FeaturePoints.Min(r => r.Y);
            int bottom = (int)face.FeaturePoints.Max(r => r.X);
            int right = (int)face.FeaturePoints.Max(r => r.Y);
            ImageHelper.cutPicture(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "timg.jpg"),
             left, top, right , bottom - top);
        }
        public void onProcessingException(AffdexException ex)
        {

        } 
        public void onProcessingFinished()
        {

        }
    }


}
