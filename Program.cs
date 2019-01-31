using System;
using System.IO;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreyGradient
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            string fileName = args[0];
            var fileStream  = new FileStream(fileName,  FileMode.OpenOrCreate, FileAccess.ReadWrite);
            
            int width  = Convert.ToInt32(args[1]);
            int height = Convert.ToInt32(args[2]);

            char dType = Convert.ToChar(args[3]);

            const int intensity = 255;

            var buf  = new byte[width*height];
            var buf2 = new double[height, width];
            
            var header = "P5\n" + width + " " + height + "\n" + intensity + "\n";

            switch (dType)
            {
                    case '0':
                        Console.WriteLine("case 0:\nNo_Dithering method");
                        No_Dithering(fileStream, width, height, header, buf, buf2);
                        break;
                    case '1':
                        Console.WriteLine("case 1:\nOrdered_Dith method");
                        Ordered_Dith(fileStream, width, height, header, buf, buf2);
                        break;
                    case '2':
                        Console.WriteLine("case 2:\nRandomDither method");
                        RandomDither(fileStream, width, height, header, buf, buf2);
                        break;
                    case '3':
                        Console.WriteLine("case 3:\nFloyd_Dither method");
                        Floyd_Dither(fileStream, width, height, header, buf, buf2);
                        break;
                    case '4':
                        Console.WriteLine("case 4:\nJJNDithering method");
                        JjnDithering(fileStream, width, height, header, buf, buf2);
                        break;
                    default:
                        Console.WriteLine("Incorrect dithering type.");
                        return 1;
            }

            return 0;

        }

        /// <summary>
        /// "Идеальный" градиент
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="width">Требуемая ширина изображения</param>
        /// <param name="height">Требуемая высота изображения</param>
        /// <param name="header">Заранее подготовленная строка в файле</param>
        /// <param name="buf">Стартовый массив, он же конечный</param>
        /// <param name="buf2">Промежуточный массив</param>
        private static void No_Dithering(FileStream fileStream, int width, int height, string header, byte[] buf,
            double[,] buf2)
        {
            for (var j = 0; j < header.Length; j++)
            {
                fileStream.WriteByte((byte) header[j]);
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf2[i, j] = 255d * j / (width - 1d);
                }
            }
            
            for (var i = 0; i < width; i++)
            {    
                for (var j = 0; j < height; j++)
                {
                    buf[j + i * width] = (byte) buf2[i, j];
                }
            }

            fileStream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Упорядоченный дизеринг
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="width">Требуемая ширина изображения</param>
        /// <param name="height">Требуемая высота изображения</param>
        /// <param name="header">Заранее подготовленная строка в файле</param>
        /// <param name="buf">Стартовый массив, он же конечный</param>
        /// <param name="buf2">Промежуточный массив</param>
        private static void Ordered_Dith(FileStream fileStream, int width, int height, string header, byte[] buf,
            double[,] buf2)
        {
            for (var j = 0; j < header.Length; j++)
            {
                fileStream.WriteByte((byte) header[j]);
            }
            
            var ditherMatrix = new double[,]
            {
                {0     , 0.5   , 0.125,  0.625},
                {0.75  , 0.25  , 0.875,  0.375},
                {0.1875, 0.6875, 0.0625, 0.5625},
                {0.9375, 0.4375, 0.8125, 0.3125}
            };
            
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf2[i, j] = 255d * j / (width - 1d);
                }
            }

            var row = 0;
            var col = 0;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (col % 3 == 0 && col != 0) 
                        col = 0;
                    
                    buf2[i, j] = Math.Floor(buf2[i, j] + ditherMatrix[row, col]);

                    col++;
                }

                if (row % 3 == 0 && row != 0)
                    row = 0;
                row++;
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf[j + i * height] = (byte) buf2[i, j];
                }
            }
            
            fileStream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Случайный дизеринг
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="width">Требуемая ширина изображения</param>
        /// <param name="height">Требуемая высота изображения</param>
        /// <param name="header">Заранее подготовленная строка в файле</param>
        /// <param name="buf">Стартовый массив, он же конечный</param>
        /// <param name="buf2">Промежуточный массив</param>
        private static void RandomDither(FileStream fileStream, int width, int height, string header, byte[] buf,
            double[,] buf2)
        {
            for (var j = 0; j < header.Length; j++)
            {
                fileStream.WriteByte((byte) header[j]);
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf2[i, j] = 255d * j / (width - 1d);
                }
            }
            
            var random = new Random();

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var temp = Convert.ToDouble(random.Next(100)) / 100;
                    if (temp >= buf2[i, j] % 1)
                        buf2[i, j] = Math.Floor(buf2[i, j]);
                    else
                        buf2[i, j] = Math.Ceiling(buf2[i, j]);
                }
            }
            
            for (var i = 0; i < height; i++)
            {    
                for (var j = 0; j < width; j++)
                {
                    buf[j + i * height] = (byte) buf2[i, j];
                }
            }

            fileStream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Дизеринг Флойда-Штайнберга
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="width">Требуемая ширина изображения</param>
        /// <param name="height">Требуемая высота изображения</param>
        /// <param name="header">Заранее подготовленная строка в файле</param>
        /// <param name="buf">Стартовый массив, он же конечный</param>
        /// <param name="buf2">Промежуточный массив</param>
        private static void Floyd_Dither(FileStream fileStream, int width, int height, string header, byte[] buf,
            double[,] buf2)
        {
            for (var j = 0; j < header.Length; j++)
            {
                fileStream.WriteByte((byte) header[j]);
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf2[i, j] = 255d * j / (width - 1d);
                }
            }

            for (var i = 0; i < height - 1; i++)
            {
                for (var j = 1; j < width - 1; j++)
                {
                    var oldPixel = buf2[i, j];
                    var newPixel = Math.Round(oldPixel); // ... = Math.Round(oldPixel/255);
                    buf2[i, j] = newPixel;
                    var quantError = oldPixel - newPixel;
                    
                    buf2[i  ,   j+1] = buf2[i,   j+1] + quantError * 7d / 16d;
                    buf2[i+1,   j-1] = buf2[i+1, j-1] + quantError * 3d / 16d;
                    buf2[i+1,     j] = buf2[i+1,   j] + quantError * 5d / 16d;
                    buf2[i+1,   j+1] = buf2[i+1, j+1] + quantError * 1d / 16d;
                    
                }
            }
            
            for (var i = 0; i < height - 1; i++)
            {    
                for (var j = 0; j < width - 1; j++)
                {
                    buf[j + i * height] = (byte) buf2[i, j];
                }
            }

            fileStream.Write(buf, 0, buf.Length);
        }

        /// <summary>
        /// Дизеринг Джарвиса, Джудиса и Нинке
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="width">Требуемая ширина изображения</param>
        /// <param name="height">Требуемая высота изображения</param>
        /// <param name="header">Заранее подготовленная строка в файле</param>
        /// <param name="buf">Стартовый массив, он же конечный</param>
        /// <param name="buf2">Промежуточный массив</param>
        private static void JjnDithering(FileStream fileStream, int width, int height, string header, byte[] buf,
            double[,] buf2)
        {
            for (var j = 0; j < header.Length; j++)
            {
                fileStream.WriteByte((byte) header[j]);
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    buf2[i, j] = 255d * j / (width - 1d);
                }
            }

            for (var i = 0; i < height - 2; i++)
            {
                for (var j = 2; j < width - 2; j++)
                {
                    var oldPixel = buf2[i, j];
                    var newPixel = Math.Round(oldPixel); // ... = Math.Round(oldPixel/255);
                    buf2[i, j] = newPixel;
                    var quantError = oldPixel - newPixel;
                    
                    buf2[i,   j+1] = buf2[i, j+1] + quantError * 7d / 48d;
                    buf2[i, j + 2] = buf2[i, j+2] + quantError * 5d / 48d;
                    
                    buf2[i+1,   j-2] = buf2[i+1, j-2] + quantError * 3d / 48d;
                    buf2[i+1,   j-1] = buf2[i+1, j-1] + quantError * 5d / 48d;
                    buf2[i+1,     j] = buf2[i+1,   j] + quantError * 7d / 48d;
                    buf2[i+1,   j+1] = buf2[i+1, j+1] + quantError * 5d / 48d;
                    buf2[i+1,   j+2] = buf2[i+1, j+2] + quantError * 3d / 48d;
                    
                    buf2[i+2,   j-2] = buf2[i+2, j-2] + quantError * 1d / 48d;
                    buf2[i+2,   j-1] = buf2[i+2, j-1] + quantError * 3d / 48d;
                    buf2[i+2,   j]   = buf2[i+2,   j] + quantError * 5d / 48d;
                    buf2[i+2,   j+1] = buf2[i+2, j+1] + quantError * 3d / 48d;
                    buf2[i+2,   j+2] = buf2[i+2, j+2] + quantError * 1d / 48d;
                    
                }
            }
            
            for (var i = 0; i < height - 1; i++)
            {    
                for (var j = 0; j < width - 1; j++)
                {
                    buf[j + i * height] = (byte) buf2[i, j];
                }
            }

            fileStream.Write(buf, 0, buf.Length);
        }
        
    }
}
