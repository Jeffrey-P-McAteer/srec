using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Formats.Asn1;

namespace srec
{
    public class Srec
    {
        public static void Main(string[] args)
        {

            Console.WriteLine("Hello, World!");

            using (var recorder = new Recorder())
            {
                Thread.Sleep(2500);
            }


            Console.WriteLine("Done!");

        }



    }

    public class Recorder: IDisposable
    {
        private int LeftOffset;
        private int TopOffset;
        private int ScreenWidth;
        private int ScreenHeight;

        private Thread ScreenReaderThread;
        private ManualResetEvent stopThread;


        private int FramesPerSecond;

        public Recorder()
        {
            this.LeftOffset = 0;
            this.TopOffset = 0;
            this.ScreenWidth = 1280;
            this.ScreenHeight = 1080;

            this.FramesPerSecond = 8;

            this.stopThread = new ManualResetEvent(false);
            this.ScreenReaderThread = new Thread(RecordScreen)
            {
                Name = typeof(Recorder).Name + ".RecordScreen",
                IsBackground = true
            };

            this.ScreenReaderThread.Start();
        }

        public void Dispose()
        {
            stopThread.Set();
            this.ScreenReaderThread.Join();

            // Close writer: the remaining data is written to a file and file is closed
            // writer.Close();

            stopThread.Dispose();
        }

        void RecordScreen()
        {
            var frameInterval = TimeSpan.FromSeconds(1.0 / (double) this.FramesPerSecond);
            var buffer = new byte[this.ScreenWidth * this.ScreenHeight * 4];
            Task videoWriteTask = null;
            var timeTillNextFrame = TimeSpan.Zero;

            while (!stopThread.WaitOne(timeTillNextFrame))
            {
                var timestamp = DateTime.Now;

                Screenshot(buffer);

                // Wait for the previous frame is written
                videoWriteTask?.Wait();

                // Start asynchronous (encoding and) writing of the new frame
                // videoWriteTask = videoStream.WriteFrameAsync(true, buffer, 0, buffer.Length);

                timeTillNextFrame = timestamp + frameInterval - DateTime.Now;
                if (timeTillNextFrame < TimeSpan.Zero)
                {
                    timeTillNextFrame = TimeSpan.Zero;
                }
            }

            // Wait for the last frame is written
            videoWriteTask?.Wait();
        }

        public void Screenshot(byte[] Buffer)
        {
            using (var BMP = new Bitmap(this.ScreenWidth, this.ScreenHeight))
            {
                using (var g = Graphics.FromImage(BMP))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, new Size(this.ScreenWidth, this.ScreenHeight), CopyPixelOperation.SourceCopy);

                    g.Flush();

                    var bits = BMP.LockBits(new Rectangle(this.LeftOffset, this.TopOffset, this.ScreenWidth, this.ScreenHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                    Marshal.Copy(bits.Scan0, Buffer, 0, Buffer.Length);
                    BMP.UnlockBits(bits);
                }
            }
        }
    }

}