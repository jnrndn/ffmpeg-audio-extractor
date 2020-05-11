using NReco.VideoConverter;
using System;
using System.IO;
using System.Net;

namespace WeirdAudioTest
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
                return;

            var path = args[0]; //@"C:\temp\Guido_Are_We_An_LMS.mp4";
            var output = args[1]; // @"c:\temp\Guido_Are_We_An_LMS.wav";

            Uri uri = new Uri(path);
            FileInfo fi = new FileInfo(uri.AbsolutePath);
            WebClient client = new WebClient();
            Guid tempGuid = Guid.NewGuid();
            string fileName = $@"c:\temp\{tempGuid}{fi.Extension}";
            client.DownloadFile(path, fileName);
            using (Stream audioStream = GetAudioFromVideoUrl(fileName))
            using (MemoryStream ms = new MemoryStream())
            {
                audioStream.CopyTo(ms);
                File.WriteAllBytes(output, ms.GetBuffer());
            }
            File.Delete(fileName);
        }

        public static Stream GetAudioFromVideoUrl(string filePath)
        {
            var audioStream = new MemoryStream();
            var duration = 0;
            try
            {
                var ffmpeg = new FFMpegConverter();

                ffmpeg.ConvertProgress += (o, args) =>
                {
                    duration = Convert.ToInt32(args.Processed.TotalSeconds);
                };

                ffmpeg.ConvertMedia(filePath, null, audioStream, "wav",
                new ConvertSettings
                {
                    CustomOutputArgs = "-acodec pcm_s16le -ac 1 -ar 16000"
                });
            }
            catch (FFMpegException convertionException)
            {
                System.Console.WriteLine(convertionException.Message);
            }
            audioStream.Seek(0, SeekOrigin.Begin);
            return audioStream;
        }
    }
}
