using System;
using System.Collections.Generic;
using System.Text;
using Vision;
using System.Drawing;
using System.IO;

namespace VisionStatic {
    static class UserGUI {

        static public void highlightBlob(RAWImage rawImage, Blob blob) {

            int zLeft, zRight, zTop, zBottom;
            int zImgWidth, zImgHeight;

            int boxWidth, boxHeight;
            int i;
            int row;

            const byte hlColR = 0, hlColG = 255, hlColB = 0;

            zLeft = blob.Left * 3 * rawImage.zoomFactor;
            zRight = blob.Right * 3 * rawImage.zoomFactor;
            zTop = blob.Top * rawImage.zoomFactor;
            zBottom = (blob.Bottom + 1) * rawImage.zoomFactor;

            zImgWidth = rawImage.Width;
            zImgHeight = rawImage.Height;


            boxWidth = (zRight - zLeft);
            boxHeight = (zBottom - zTop);

            i = zTop * zImgWidth * 3 + zLeft + 3;
            do {
                rawImage.RawData[i] = hlColR;
                rawImage.RawData[i + 1] = hlColG;
                rawImage.RawData[i + 2] = hlColB;

                i += 3;
            } while (i < zTop * zImgWidth * 3 + zLeft + boxWidth + 3 * rawImage.zoomFactor - 3);

            row = zTop + 1;
            do {
                i = row * zImgWidth * 3 + zLeft;
                rawImage.RawData[i] = hlColR;
                rawImage.RawData[i + 1] = hlColG;
                rawImage.RawData[i + 2] = hlColB;
                row++;
            } while (row < zBottom);

            row = zTop + 1;
            do {
                i = row * zImgWidth * 3 + zRight + 3 * rawImage.zoomFactor - 3; //-3
                rawImage.RawData[i] = hlColR;
                rawImage.RawData[i + 1] = hlColG;
                rawImage.RawData[i + 2] = hlColB;
                row++;
            } while (row < zBottom);

            i = (zBottom - 1) * zImgWidth * 3 + zLeft - 3; // -3
            do {
                rawImage.RawData[i] = hlColR;
                rawImage.RawData[i + 1] = hlColG;
                rawImage.RawData[i + 2] = hlColB;

                i += 3;
            } while (i < (zBottom - 1) * zImgWidth * 3 + zLeft + boxWidth + 3 * rawImage.zoomFactor - 3);


        }

        static public Blob findBlobByColAndCenter(Blob[] blobArr, int totalBlobs, Color rgbCol, int centerX, int centerY, ColorCalibration colorCalibObj) {
            int i;
            Blob blob = null;

            i = 0;
            do {
                if (blobArr[i].ColorClass == colorCalibObj.RGBtoCCTable[rgbCol.R * 256 * 256 + rgbCol.G * 256 + rgbCol.B]) {
                    if (blob == null) {
                        blob = blobArr[i];
                    } else {
                        if (Math.Sqrt(Math.Pow(blobArr[i].CenterX - centerX, 2) + Math.Pow(blobArr[i].CenterY - centerY, 2)) <
                            Math.Sqrt(Math.Pow(blob.CenterX - centerX, 2) + Math.Pow(blob.CenterY - centerY, 2))) {
                            blob = blobArr[i];
                        }
                    }
                }
                i++;
            } while (i < totalBlobs);

            return blob;
        }



        static public void buildLookupTable(byte[] colorLookup) {
            int i, j, k;
            byte cmin, c;

            int distmin;


            for (i = 0; i <= 255; i++) {
                for (j = 0; j <= 255; j++) {
                    for (k = 0; k <= 255; k++) {
                        cmin = 0;
                        distmin = (i - ColorClasses.COLOR_CLASSES[cmin].R) * (i - ColorClasses.COLOR_CLASSES[cmin].R) +
                                 (j - ColorClasses.COLOR_CLASSES[cmin].G) * (j - ColorClasses.COLOR_CLASSES[cmin].G) +
                                 (k - ColorClasses.COLOR_CLASSES[cmin].B) * (k - ColorClasses.COLOR_CLASSES[cmin].B);
                        for (c = 0; c < ColorClasses.COLOR_CLASSES.Length; c++) {
                            if (((i - ColorClasses.COLOR_CLASSES[c].R) * (i - ColorClasses.COLOR_CLASSES[c].R) +
                                 (j - ColorClasses.COLOR_CLASSES[c].G) * (j - ColorClasses.COLOR_CLASSES[c].G) +
                                 (k - ColorClasses.COLOR_CLASSES[c].B) * (k - ColorClasses.COLOR_CLASSES[c].B)) < distmin
                                  || c == 0) {
                                cmin = c;
                                distmin = (i - ColorClasses.COLOR_CLASSES[cmin].R) * (i - ColorClasses.COLOR_CLASSES[cmin].R) +
                                 (j - ColorClasses.COLOR_CLASSES[cmin].G) * (j - ColorClasses.COLOR_CLASSES[cmin].G) +
                                 (k - ColorClasses.COLOR_CLASSES[cmin].B) * (k - ColorClasses.COLOR_CLASSES[cmin].B);
                            }
                        }
                        colorLookup[i * 65536 + j * 256 + k] = cmin;

                    }
                }

            }
        }



        static public void exportBlobInfoToFile(string fout, Blobber BlobWorkObj) {
            //area color x y ID <anything else>           
            int i;
            string[] strBlobInfo = new string[BlobWorkObj.totalBlobs];
            for (i = 0; i < BlobWorkObj.totalBlobs; i++) {
                strBlobInfo[i] = BlobWorkObj.blobs[i].Area + " " +
                                 BlobWorkObj.blobs[i].ColorClass + " " +
                                 BlobWorkObj.blobs[i].CenterWorldX + " " +
                                 BlobWorkObj.blobs[i].CenterWorldY + " " +
                                 BlobWorkObj.blobs[i].AvgColorR + " " +
                                 BlobWorkObj.blobs[i].AvgColorG + " " +
                                 BlobWorkObj.blobs[i].AvgColorB;
            }
            File.WriteAllLines(fout, strBlobInfo);
        }
    }
}