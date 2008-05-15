using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Collections;
using System.Drawing;
using Robocup.Utilities;

namespace Vision {

    public class TsaiPtFinder {
        private class BlobCompCenterX : IComparer<Blob> {
            public int Compare(Blob b1, Blob b2) {
                return b1.CenterX.CompareTo(b2.CenterX);
            }
        }

        private class BlobCompCenterY : IComparer<Blob> {
            public int Compare(Blob b1, Blob b2) {
                return b1.CenterY.CompareTo(b2.CenterY);
            }
        }

        private static BlobCompCenterX _blobComparerX = new BlobCompCenterX();
        private static BlobCompCenterY _blobComparerY = new BlobCompCenterY();

        // Constants - TODO: extract into the global constants
        private double GRID_UNIT;
        private int ROW_Y_THRES;
        private int TSAIPT_MIN_AREA;
        private int TSAIPT_MAX_AREA;

        private RAWImage _origImg;

        public TsaiPtFinder() {
            GRID_UNIT = Constants.get<double>("vision", "GRID_UNIT");
            ROW_Y_THRES = Constants.get<int>("vision", "ROW_Y_THRES");
            TSAIPT_MIN_AREA = Constants.get<int>("vision", "TSAIPT_MIN_AREA");
            TSAIPT_MAX_AREA = Constants.get<int>("vision", "TSAIPT_MAX_AREA");
        }

        public void LoadImage(RAWImage img) {
            _origImg = img;
        }

        // order squares
        public List<Pair<Point, DPoint>> orderSquares(Blob[] blobs) {

            // will hold tsai points
            List<Pair<Point, DPoint>> tsaiPoints = new List<Pair<Point, DPoint>>();

            // only keep blobs that are tsai points
            List<Blob> lstSquares = new List<Blob>();
            int i;
            for (i = 0; i < blobs.Length; i++)
                if (blobs[i] != null && blobs[i].Area >= TSAIPT_MIN_AREA && blobs[i].Area <= TSAIPT_MAX_AREA)
                    lstSquares.Add(blobs[i]);


            // sort by Y coordinate
            lstSquares.Sort(_blobComparerY);


            List<Blob> row = new List<Blob>();

            i = 0; // row counter
            while (lstSquares.Count > 0) {

                row.Clear();

                // extract row
                row.Add(lstSquares[lstSquares.Count - 1]);
                lstSquares.RemoveAt(lstSquares.Count - 1);
                double rowAvgY = row[0].CenterY;
                while (lstSquares.Count > 0 && Math.Abs(lstSquares[lstSquares.Count - 1].CenterY - rowAvgY) < ROW_Y_THRES) {
                    row.Add(lstSquares[lstSquares.Count - 1]);
                    lstSquares.RemoveAt(lstSquares.Count - 1);
                }

                // sort row by X coordinate
                row.Sort(_blobComparerX);

                int j; // column counter
                for (j = 0; j < row.Count; j++) {

                    // prepare bitmap with one individual square
                    int sqWidth = row[j].Right - row[j].Left + 1;
                    int sqHeight = row[j].Bottom - row[j].Top + 1;
                    byte[] rawData = new byte[sqWidth * sqHeight * 3];
                    int k = 0;
                    for (int ii = row[j].Top; ii <= row[j].Bottom; ii++) {
                        for (int jj = row[j].Left; jj <= row[j].Right; jj++) {
                            rawData[k++] = _origImg.rawData[ii * _origImg.Height + jj];
                            rawData[k++] = _origImg.rawData[ii * _origImg.Height + jj + 1];
                            rawData[k++] = _origImg.rawData[ii * _origImg.Height + jj + 2];
                        }
                    }
                    RAWImage img = new RAWImage(rawData, sqWidth, sqHeight, 1);

                    //Point[] corners = detectCorners(img);

                    Pair<Point, DPoint> tsaiPt = new Pair<Point, DPoint>(
                        //tsaiPt.First = corners[0];
                        new Point(row[j].CenterX, row[j].CenterY),
                        new DPoint(i * 2 * GRID_UNIT, j * 2 * GRID_UNIT));
                    tsaiPoints.Add(tsaiPt);
                    /*
                                                tsaiPt = new Pair<Point, DPoint>();
                                                tsaiPt.First = corners[1];
                                                tsaiPt.Second = new DPoint(i * 2 * SQUARE_HEIGHT, j * 2 * SQUARE_WIDTH + SQUARE_WIDTH);
                                                tsaiPoints.push(tsaiPt);

                                                tsaiPt = new Pair<Point, DPoint>();
                                                tsaiPt.First = corners[2];
                                                tsaiPt.Second = new DPoint(i * 2 * SQUARE_HEIGHT + SQUARE_HEIGHT, j * 2 * SQUARE_WIDTH);
                                                tsaiPoints.push(tsaiPt);

                                                tsaiPt = new Pair<Point, DPoint>();
                                                tsaiPt.First = corners[3];
                                                tsaiPt.Second = new DPoint(i * 2 * SQUARE_HEIGHT + SQUARE_HEIGHT, j * 2 * SQUARE_WIDTH + SQUARE_WIDTH);
                                                tsaiPoints.push(tsaiPt);
                                                */
                }

                i++;
            }
            return tsaiPoints;
        }

        // NOT IMPLEMENTED
        private Point[] detectCorners(RAWImage img) {
            Point[] corners = new Point[4];
            for (int i = 0; i < 4; i++)
                corners[i] = new Point(0, 0);
            return corners;
        }
    }
}

