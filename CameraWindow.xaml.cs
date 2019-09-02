using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Affdex;

namespace WpfAffdex
{
   
    public partial class CameraWindow : Window
    {
        private AffdexListener AffdexListener = new AffdexListener();
        private Affdex.Detector Detector { get; set; }
        public CameraWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ContentRendered += MainWindow_ContentRendered; 
        }
        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            StartCameraProcessing();
        }

        /// <summary>
        /// Starts the camera processing.
        /// </summary>
        private void StartCameraProcessing()
        {
            try
            { 
                // Instantiate CameraDetector using default camera ID
                const int cameraId = 0;
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;
                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);
               
                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath("C:\\Program Files\\Affectiva\\AffdexSDK\\data");

                //跟踪一些 我们预先设置的的分类器，比如开心，讨厌等等  
                TurnOnClassifiers();

                Detector.setImageListener(AffdexListener);//设置两个监听
                Detector.setProcessStatusListener(AffdexListener);

                Detector.start(); 
              
            }
            catch (Affdex.AffdexException ex)
            {
                //if (!String.IsNullOrEmpty(ex.Message))
                //{
                //    // If this is a camera failure, then reset the application to allow the user to turn on/enable camera
                //    if (ex.Message.Equals("Unable to open webcam."))
                //    {
                //        MessageBoxResult result = MessageBox.Show(ex.Message,
                //                                                "AffdexMe Error",
                //                                                MessageBoxButton.OK,
                //                                                MessageBoxImage.Error);
                //        StopCameraProcessing();
                //        return;
                //    }
                //}

                //String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                //ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                //String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                //ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Turns the on classifiers.
        /// </summary>
        private void TurnOnClassifiers()
        {
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
         
        }
    }


    public class AffdexListener : Affdex.ImageListener, Affdex.ProcessStatusListener
    {
        
        /// <summary>
        /// 保存图片到文件
        /// </summary>
        /// <param name="image">图片数据</param>
        /// <param name="filePath">保存路径</param>
        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = GetBitmapEncoder(filePath);
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
        /// <summary>
        /// 根据文件扩展名获取图片编码器
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>图片编码器</returns>
        private BitmapEncoder GetBitmapEncoder(string filePath)
        {
            var extName = System.IO.Path.GetExtension(filePath).ToLower();
            if (extName.Equals(".png"))
            {
                return new PngBitmapEncoder();
            }
            else
            {
                return new JpegBitmapEncoder();
            }
        }
        /// <summary>
        /// Handles the Image capture from source produced by Affdex.Detector
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
        public void onImageCapture(Affdex.Frame frame)
        { 
            byte[] imageData =frame.getBGRByteArray();
            int width = frame.getWidth();
            int height = frame.getHeight();
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    SaveImageToFile(imageSrc, System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, frame.getTimestamp().ToString() + ".png"));
                }
            }
            catch (Exception ex)
            {

            }
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
                    if(face.Expressions.InnerBrowRaise > 0) Console.WriteLine("内侧眉毛提起" + face.Expressions.InnerBrowRaise);//内侧眉毛提起
                    if(face.Expressions.BrowRaise > 0) Console.WriteLine("外侧眉毛提起" + face.Expressions.BrowRaise);     //外侧眉毛提起 
                    if(face.Expressions.BrowFurrow > 0) Console.WriteLine("眉毛降下" + face.Expressions.BrowFurrow);    //眉毛降下
                    if(face.Expressions.EyeWiden > 0) Console.WriteLine("上眼睑提起" + face.Expressions.EyeWiden);      //上眼睑提起
                    if(face.Expressions.CheekRaise > 0) Console.WriteLine("面颊提起" + face.Expressions.CheekRaise);    //面颊提起
                    if(face.Expressions.LidTighten > 0) Console.WriteLine("眼睑收紧" + face.Expressions.LidTighten);    //眼睑收紧 
                    if(face.Expressions.NoseWrinkle > 0) Console.WriteLine("鼻子起皱" + face.Expressions.NoseWrinkle);   //鼻子起皱
                    if(face.Expressions.UpperLipRaise > 0) Console.WriteLine("上嘴唇提起" + face.Expressions.UpperLipRaise); //上嘴唇提起 
                    if(face.Expressions.Dimpler > 0) Console.WriteLine("挤出酒窝" + face.Expressions.Dimpler);       //挤出酒窝
                    if(face.Expressions.LipCornerDepressor > 0) Console.WriteLine("嘴角下撇" + face.Expressions.LipCornerDepressor); //嘴角下撇 
                    if(face.Expressions.ChinRaise > 0) Console.WriteLine("下巴提起" + face.Expressions.ChinRaise);     //下巴提起
                    if(face.Expressions.LipPucker > 0) Console.WriteLine("嘴唇皱起" + face.Expressions.LipPucker);     //嘴唇皱起 
                    if(face.Expressions.LipStretch > 0) Console.WriteLine("嘴唇拉伸" + face.Expressions.LipStretch);    //嘴唇拉伸 
                    if(face.Expressions.LipPress > 0) Console.WriteLine("紧压嘴唇" + face.Expressions.LipPress);      //紧压嘴唇 
                    if(face.Expressions.JawDrop > 0) Console.WriteLine("下巴落下" + face.Expressions.JawDrop);       //下巴落下
                    if(face.Expressions.MouthOpen > 0) Console.WriteLine("嘴唇张大延伸" + face.Expressions.MouthOpen);     //嘴唇张大延伸
                    if(face.Expressions.LipSuck > 0) Console.WriteLine("抿嘴" + face.Expressions.LipSuck);       //抿嘴  
                    if(face.Expressions.EyeClosure > 0) Console.WriteLine("眼睛闭合" + face.Expressions.EyeClosure); //眼睛闭合 
                    //Emotions
                    if(face.Emotions.Joy > 0) Console.WriteLine("喜悦" + face.Emotions.Joy);       // - 喜悦
                    if(face.Emotions.Sadness > 0) Console.WriteLine("伤心" + face.Emotions.Sadness);   // - 伤心
                    if(face.Emotions.Anger > 0) Console.WriteLine("愤怒" + face.Emotions.Anger);     // - 愤怒
                    if(face.Emotions.Surprise > 0) Console.WriteLine("惊讶" + face.Emotions.Surprise);  // - 惊讶
                    if(face.Emotions.Fear > 0) Console.WriteLine("恐惧" + face.Emotions.Fear);      // - 恐惧
                    if(face.Emotions.Disgust > 0) Console.WriteLine("厌恶" + face.Emotions.Disgust);   // - 厌恶
                    if(face.Emotions.Contempt > 0) Console.WriteLine("轻蔑" + face.Emotions.Contempt);  // - 轻蔑
                    if(face.Emotions.Valence > 0) Console.WriteLine("效价" + face.Emotions.Valence);   // - 效价
                    if(face.Emotions.Engagement > 0) Console.WriteLine("参与度" + face.Emotions.Engagement); // - 参与度

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
