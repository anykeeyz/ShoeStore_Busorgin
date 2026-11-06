using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ShoeStore_Бусоргин.Views
{
   

    public static class ImageHelper
    {
        public static string ImagesFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res");

        public static string DefaultImagePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "picture.jpg");

        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(ImagesFolder)) Directory.CreateDirectory(ImagesFolder);
        }

        public static string ResizeAndSaveImage(string sourcePath)
        {
            EnsureDirectoriesExist();

            string ext = Path.GetExtension(sourcePath);

            string fileName = $"{Guid.NewGuid()}{ext}";
            string destPath = Path.Combine(ImagesFolder, fileName);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(sourcePath, UriKind.Relative);
            bitmap.DecodePixelWidth = 300;
            bitmap.DecodePixelHeight = 200;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream fs = new FileStream(destPath, FileMode.Create))
            {
                encoder.Save(fs);
            }

            return Path.Combine("Res", fileName);
        }

        public static void DeleteOldImage(string oldPath)
        {
            if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath))
            {
                if (oldPath != DefaultImagePath)
                {
                    try { File.Delete(oldPath); } catch (Exception ex) { MessageBox.Show(ex.Message);}
                }
            }
        }
    }
}
