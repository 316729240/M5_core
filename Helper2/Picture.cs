using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWMS.Helper
{
    class Picture
    {
        public struct thumbnailSize
        {
            public int width;
            public int height;
            public bool force;
            public bool saveRemoteImages;//是否保存远程图片
        }
        public static ErrInfo saveImage(HttpPostedFile file, string filePath)
        {
            ErrInfo err = new ErrInfo();

            try
            {

                string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM") + "/";
                if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));
                string kzm = "";
                if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (!Regex.IsMatch(kzm, "(jpg|gif|png)"))
                {
                    err.errNo = -1;
                    err.errMsg = "文件类型不合法，只能上传jpg,gif,png";
                    return err;
                }
                string fileName = API.GetId() + "." + kzm;
                file.SaveAs(HttpContext.Current.Server.MapPath(path + fileName));
                err.userData = path + fileName;
                return err;
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
                return err;
            }
        }
        public static string watermark(string oldfilename)
        {

            XmlNodeList list = Config.userConfig["watermark"];
            if (list == null) return oldfilename;
            string markpic = HttpContext.Current.Server.MapPath(list[0].InnerText);
            if (!System.IO.File.Exists(markpic)) return oldfilename;

            string _fileName = oldfilename;
            oldfilename = HttpContext.Current.Server.MapPath("~" + oldfilename);
            string filename = oldfilename;
            FileInfo f = new FileInfo(oldfilename);
            int zl = int.Parse(list[6].InnerText);


            try
            {
                float proportion = float.Parse(list[5].InnerText), transparency = float.Parse(list[4].InnerText) / 100;
                int margins = int.Parse(list[3].InnerText), X = int.Parse(list[1].InnerText), Y = int.Parse(list[2].InnerText);
                System.Drawing.Image copyImage = System.Drawing.Image.FromFile(markpic);
                System.Drawing.Image image = System.Drawing.Image.FromFile(oldfilename);
                Bitmap bm = new Bitmap(image);
                image.Dispose();
                if ((bm.Width * proportion) > copyImage.Width && (bm.Height * proportion) > copyImage.Height)
                {
                    if (list[7].InnerText == "1")
                    {
                        filename = f.Directory.FullName + @"\" + f.Name.Replace(f.Extension, "") + "_1" + f.Extension;
                        _fileName = _fileName.Replace(f.Name, f.Name.Replace(f.Extension, "") + "_1" + f.Extension);
                    }
                    int top = margins, left = margins;
                    Graphics g = Graphics.FromImage(bm);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    ColorMatrix cmatrix = new ColorMatrix();
                    cmatrix.Matrix33 = transparency;
                    ImageAttributes imgattributes = new ImageAttributes();
                    imgattributes.SetColorMatrix(cmatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    if (X == 2) left = bm.Width - copyImage.Width - margins;
                    else if (X == 1) { left = (bm.Width - copyImage.Width) / 2; }
                    if (Y == 2) top = bm.Height - copyImage.Height - margins;
                    else if (Y == 1) { top = (bm.Height - copyImage.Height) / 2; }
                    g.DrawImage(copyImage, new Rectangle(left, top, copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel, imgattributes);
                    g.Dispose();
                    MemoryStream ms = new MemoryStream();

                    // 以下代码为保存图片时，设置压缩质量
                    EncoderParameters encoderParams = new EncoderParameters();
                    long[] quality = new long[1];
                    quality[0] = zl;

                    EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    encoderParams.Param[0] = encoderParam;

                    //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICI = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICI = arrayICI[x];//设置JPEG编码
                            break;
                        }
                    }
                    if (jpegICI != null)
                    {
                        bm.Save(filename, jpegICI, encoderParams);
                    }
                    else
                    {
                        bm.Save(filename);
                    }
                }
                bm.Dispose();
                copyImage.Dispose();
            }
            catch
            {
            }
            return _fileName;
        }
        public struct ImgSize
        {
            public int Height;
            public int Width;
            public int OldHeight;
            public int OldWidth;
        }

        public static ImgSize GetPictureSize(string PicPath, float MaxWidth, float MaxHeight)
        {
            return (GetPictureSize(PicPath, MaxWidth, MaxHeight, "min"));
        }
        public static ImgSize GetPictureSize(string PicPath, float MaxWidth, float MaxHeight, string tag)
        {
            PicPath = HttpContext.Current.Server.MapPath("~" + PicPath);
            ImgSize V;
            V.Height = 0;
            V.Width = 0;
            V.OldHeight = 0;
            V.OldWidth = 0;
            double BL = 1;
            //try
            //{
            if (File.Exists(PicPath))
            {

                System.Drawing.Image myImage = System.Drawing.Image.FromFile(PicPath);
                #region
                double OldWidth = myImage.Width, OldHeight = myImage.Height, NewWidth = MaxWidth, NewHeight = MaxHeight;
                //double OldWidth = 960, OldHeight = 670, NewWidth = MaxWidth, NewHeight = MaxHeight;
                int NewWidth2 = (int)NewWidth, NewHeight2 = (int)NewHeight;
                if (tag == "min")
                {
                    if (OldWidth > NewWidth || OldHeight > NewHeight)
                    {
                        BL = OldWidth / NewWidth;
                        NewHeight2 = (int)(OldHeight / BL);
                        if (NewHeight2 <= NewHeight)
                        {
                            V.Height = NewHeight2;
                            V.Width = (int)NewWidth;
                        }
                        BL = OldHeight / NewHeight;
                        NewWidth2 = (int)(OldWidth / BL);
                        if (NewWidth2 <= NewWidth)
                        {
                            if (NewWidth2 > V.Width)
                            {
                                V.Height = (int)NewHeight;
                                V.Width = NewWidth2;
                            }
                        }
                    }
                    else
                    {
                        V.Height = (int)OldHeight;
                        V.Width = (int)OldWidth;
                    }
                }
                else
                {
                    //throw new NullReferenceException(
                    //NewWidth = 126; NewHeight = 1250;
                    //OldWidth = 1; OldHeight = 200;

                    BL = OldWidth / NewWidth;
                    V.Height = (int)(OldHeight / BL);
                    V.Width = (int)(OldWidth / BL);
                    if (V.Height < NewHeight || V.Width < NewWidth)
                    {
                        BL = OldHeight / NewHeight;
                        V.Height = (int)(OldHeight / BL);
                        V.Width = (int)(OldWidth / BL);
                    }


                }
                #endregion
                myImage.Dispose();
            }
            //}
            //catch{}

            return (V);
        }
        public static string PictureSize(string fileName, string newFile, int maxWidth, int maxHeight, int PZ)
        {
            return (PictureSize(fileName, newFile, maxWidth, maxHeight, PZ, false));
        }
        public static string PictureSize(string fileName, string newFile, int maxWidth, int maxHeight, int PZ, bool tag)
        {
            if (maxWidth == 0 || maxHeight == 0) return fileName;
            if (System.IO.File.Exists(HttpContext.Current.Server.MapPath("~" + fileName)))
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~" + fileName));
                //if ((img.Width > maxWidth && img.Height > maxHeight) || (tag && (img.Width != maxWidth || img.Height != maxHeight)))
                if (img.Width != maxWidth || img.Height != maxHeight)
                {
                    System.Drawing.Imaging.ImageFormat thisFormat = img.RawFormat;
                    string t = "min";
                    if (tag) t = "max";
                    ImgSize newSize = GetPictureSize(fileName, maxWidth, maxHeight, t);
                    /*
                    if (newSize.Width == img.Width && newSize.Height == img.Height)
                    {
                        if(!tag ||( newSize.Width== maxWidth && newSize.Width == newSize.Height))
                        {

                            img.Dispose();
                            return (fileName);
                        }
                    }*/
                    int CanvasWidth = newSize.Width, CanvasHeight = newSize.Height, CanvasTop = 0, CanvasLeft = 0;
                    if (tag)
                    {
                        CanvasWidth = maxWidth; CanvasHeight = maxHeight;
                        if (newSize.Width > maxWidth) CanvasLeft = (newSize.Width - maxWidth) / 2;
                        //if (newSize.Height > maxHeight) CanvasTop = (newSize.Height - maxHeight) / 2;
                    }
                    else
                    {
                        CanvasWidth = maxWidth; CanvasHeight = maxHeight;
                        if (newSize.Width < maxWidth) CanvasLeft = (newSize.Width - maxWidth) / 2;
                        if (newSize.Height < maxHeight) CanvasTop = (newSize.Height - maxHeight) / 2;
                    }
                    Bitmap outBmp = new Bitmap(CanvasWidth, CanvasHeight);
                    Graphics g = Graphics.FromImage(outBmp);
                    Color color = Color.FromArgb(242, 242, 242);
                    Brush brush = new SolidBrush(color);
                    g.FillRectangle(brush, 0, 0, CanvasWidth, CanvasHeight);
                    // 设置画布的描绘质量
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //throw new NullReferenceException(newSize.Width.ToString() + "," + newSize.Height.ToString() + "," + CanvasLeft.ToString());
                    g.DrawImage(img, new Rectangle(-CanvasLeft, -CanvasTop, newSize.Width, newSize.Height),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                    g.Dispose();

                    // 以下代码为保存图片时，设置压缩质量
                    EncoderParameters encoderParams = new EncoderParameters();
                    long[] quality = new long[1];
                    quality[0] = PZ;

                    EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    encoderParams.Param[0] = encoderParam;

                    //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICI = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICI = arrayICI[x];//设置JPEG编码
                            break;
                        }
                    }
                    img.Dispose();
                    if (jpegICI != null)
                    {
                        outBmp.Save(HttpContext.Current.Server.MapPath("~" + newFile), jpegICI, encoderParams);
                    }
                    else
                    {
                        outBmp.Save(HttpContext.Current.Server.MapPath("~" + newFile), thisFormat);
                    }
                    outBmp.Dispose();
                    return (newFile);
                }
                return fileName;

            }
            return ("");

        }
    }
}
