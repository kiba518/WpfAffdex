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
    public partial class PhotoWindowAdvance : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    { 
        private Affdex.PhotoDetector Detector { get; set; }
        public PhotoWindowAdvance()
        {
            InitializeComponent(); 
            uint maxNumFaces = 1;//最多识别图片中几张脸
            Detector = new Affdex.PhotoDetector(maxNumFaces, Affdex.FaceDetectorMode.SMALL_FACES);
            //Set location of the classifier data files, needed by the SDK
            Detector.setClassifierPath("C:\\Program Files\\Affectiva\\AffdexSDK\\data");
            //String newPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data"); //可以使用其它路径，但需要把Data复制过去
            //Detector.setClassifierPath(newPath);

            //跟踪一些 我们预先设置的的分类器，比如开心，讨厌等等  
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectAllEmojis(true);
            Detector.setDetectGender(true);
            Detector.setDetectGlasses(true);

            //以下为分类器
            Detector.setDetectJoy(true);
            Detector.setDetectSadness(true);
            Detector.setDetectAnger(true);
            Detector.setDetectDisgust(true);
            Detector.setDetectSurprise(true);
            Detector.setDetectFear(true);


            Detector.setImageListener(this);//设置两个监听
            Detector.setProcessStatusListener(this);
            Detector.start();


            //Bitmap bmpt = (Bitmap)System.Drawing.Image.FromFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "timg.jpg"));
            //bmpt.Save(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "timg_1.png"));

            //==================================================================================================================================
            byte[] bytes = FileHelper.FileToBytes(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "timg.jpg"));
            BitmapSource bitmapSource = ImageHelper.BytesToBitmapImage(bytes);


            //var imageSrc = bitmapSource;
            //System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            //encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imageSrc));
            //using (var stream = new System.IO.FileStream(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
            //    "timg_new.png"), System.IO.FileMode.Create))
            //{
            //    encoder.Save(stream); 导出 bitmapsource已经存储成功 证明图片是正常导入进了bitmapsource
            //}
            //==================================================================================================================================
            var w = bitmapSource.Width;
            var h = bitmapSource.Height;
            var stride = bitmapSource.Format.BitsPerPixel * (int)w / 8; //计算Stride  
            byte[] byteList = new byte[(int)h * stride];
            bitmapSource.CopyPixels(byteList, stride, 0);  //调用CopyPixels 
            Affdex.Frame frame = new Affdex.Frame((int)w, (int)h, byteList, Affdex.Frame.COLOR_FORMAT.BGRA);
            //==================================================================================================================================
            //var len = frame.getBGRByteArrayLength();
            //byte[] imageData = frame.getBGRByteArray(); 
            //Console.WriteLine($"onImageCapture帧的buf len{imageData.Length} len{len}");
            //int width = frame.getWidth();
            //int height = frame.getHeight();
            //var ColorFormat = frame.getColorFormat();

            //if (imageData != null && imageData.Length > 0)
            //{
            //    var _stride = (width * System.Windows.Media.PixelFormats.Rgb24.BitsPerPixel + 7) / 8;//_stride 1791
            //    Console.WriteLine($"_stride{_stride}");
            //    var imageSrc = System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96d, 96d, System.Windows.Media.PixelFormats.Bgr24,
            //        null, imageData, _stride);

            //    System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            //    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imageSrc));

            //    using (var stream =
            //    new System.IO.FileStream(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
            //        "我是分析前.png"), System.IO.FileMode.Create))
            //    {
            //        encoder.Save(stream);
            //    }
            //}
            //==================================================================================================================================
            Detector.process(frame);
        }  
        /// <summary>
        /// Handles the Image capture from source produced by Affdex.Detector
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
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

        /// <summary>
        /// Handles the Image results event produced by Affdex.Detector
        /// </summary>
        /// <param name="faces">The detected faces.</param>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image analyzed.</param>
        public void onImageResults(Dictionary<int, Face> faces, Affdex.Frame frame)
        {
            if (faces.Count > 0)
            {
                foreach (Face face in faces.Values)
                {
                    if (face.Expressions.InnerBrowRaise > 0) Console.WriteLine("内侧眉毛提起" + face.Expressions.InnerBrowRaise);//内侧眉毛提起
                    if (face.Expressions.BrowRaise > 0) Console.WriteLine("外侧眉毛提起" + face.Expressions.BrowRaise);     //外侧眉毛提起 
                    if (face.Expressions.BrowFurrow > 0) Console.WriteLine("眉毛降下" + face.Expressions.BrowFurrow);    //眉毛降下
                    if (face.Expressions.EyeWiden > 0) Console.WriteLine("上眼睑提起" + face.Expressions.EyeWiden);      //上眼睑提起
                    if (face.Expressions.CheekRaise > 0) Console.WriteLine("面颊提起" + face.Expressions.CheekRaise);    //面颊提起
                    if (face.Expressions.LidTighten > 0) Console.WriteLine("眼睑收紧" + face.Expressions.LidTighten);    //眼睑收紧 
                    if (face.Expressions.NoseWrinkle > 0) Console.WriteLine("鼻子起皱" + face.Expressions.NoseWrinkle);   //鼻子起皱
                    if (face.Expressions.UpperLipRaise > 0) Console.WriteLine("上嘴唇提起" + face.Expressions.UpperLipRaise); //上嘴唇提起 
                    if (face.Expressions.Dimpler > 0) Console.WriteLine("挤出酒窝" + face.Expressions.Dimpler);       //挤出酒窝
                    if (face.Expressions.LipCornerDepressor > 0) Console.WriteLine("嘴角下撇" + face.Expressions.LipCornerDepressor); //嘴角下撇 
                    if (face.Expressions.ChinRaise > 0) Console.WriteLine("下巴提起" + face.Expressions.ChinRaise);     //下巴提起
                    if (face.Expressions.LipPucker > 0) Console.WriteLine("嘴唇皱起" + face.Expressions.LipPucker);     //嘴唇皱起 
                    if (face.Expressions.LipStretch > 0) Console.WriteLine("嘴唇拉伸" + face.Expressions.LipStretch);    //嘴唇拉伸 
                    if (face.Expressions.LipPress > 0) Console.WriteLine("紧压嘴唇" + face.Expressions.LipPress);      //紧压嘴唇 
                    if (face.Expressions.JawDrop > 0) Console.WriteLine("下巴落下" + face.Expressions.JawDrop);       //下巴落下
                    if (face.Expressions.MouthOpen > 0) Console.WriteLine("嘴唇张大延伸" + face.Expressions.MouthOpen);     //嘴唇张大延伸
                    if (face.Expressions.LipSuck > 0) Console.WriteLine("抿嘴" + face.Expressions.LipSuck);       //抿嘴  
                    if (face.Expressions.EyeClosure > 0) Console.WriteLine("眼睛闭合" + face.Expressions.EyeClosure); //眼睛闭合 
                    //Emotions
                    if (face.Emotions.Joy > 0) Console.WriteLine("喜悦" + face.Emotions.Joy);       // - 喜悦
                    if (face.Emotions.Sadness > 0) Console.WriteLine("伤心" + face.Emotions.Sadness);   // - 伤心
                    if (face.Emotions.Anger > 0) Console.WriteLine("愤怒" + face.Emotions.Anger);     // - 愤怒
                    if (face.Emotions.Surprise > 0) Console.WriteLine("惊讶" + face.Emotions.Surprise);  // - 惊讶
                    if (face.Emotions.Fear > 0) Console.WriteLine("恐惧" + face.Emotions.Fear);      // - 恐惧
                    if (face.Emotions.Disgust > 0) Console.WriteLine("厌恶" + face.Emotions.Disgust);   // - 厌恶
                    if (face.Emotions.Contempt > 0) Console.WriteLine("轻蔑" + face.Emotions.Contempt);  // - 轻蔑
                    if (face.Emotions.Valence > 0) Console.WriteLine("效价" + face.Emotions.Valence);   // - 效价
                    if (face.Emotions.Engagement > 0) Console.WriteLine("参与度" + face.Emotions.Engagement); // - 参与度

                }
                Console.WriteLine($"faces.Count= { faces.Count }");
            }
        }
        /// <summary>
        /// Handles occurence of exception produced by Affdex.Detector
        /// </summary>
        /// <param name="ex">The <see cref="Affdex.AffdexException"/> instance containing the exception details.</param>
        public void onProcessingException(AffdexException ex)
        {

        }
        /// <summary>
        /// Handles the end of processing event; not used
        /// </summary>
        public void onProcessingFinished()
        {

        }
    }


}
