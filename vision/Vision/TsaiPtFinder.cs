using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Collections;
using System.Drawing;
using Robocup.Utilities;
using VisionStatic;
using System.IO;
using Robocup.Geometry;

namespace Vision
{

    public static class TsaiPtFinder
    {
        private class BlobCompCenterX : IComparer<Blob>
        {
            public int Compare(Blob b1, Blob b2)
            {
                return b2.CenterX.CompareTo(b1.CenterX);
            }
        }

        private class BlobCompCenterY : IComparer<Blob>
        {
            public int Compare(Blob b1, Blob b2)
            {
                return b1.CenterY.CompareTo(b2.CenterY);
            }
        }

        private static BlobCompCenterX _blobComparerX = new BlobCompCenterX();
        private static BlobCompCenterY _blobComparerY = new BlobCompCenterY();

        // corners: indeces for array
        private const int TOP_LEFT = 0;
        private const int TOP_RIGHT = 1;
        private const int BOTTOM_LEFT = 2;
        private const int BOTTOM_RIGHT = 3;

        // Constants 
        private static double GRID_UNIT;
        private static int ROW_Y_THRES;
        private static int TSAIPT_MIN_AREA;
        private static int TSAIPT_MAX_AREA;
        private static int CALIB_SQ_MARGIN;
        private static int EDGE_NEIGHBOR_DIST;
        private static double Y_X_VERTICALITY_THRES;
        private static Color CALIB_SQ_COLOR;
        private static double RANSAC_T, RANSAC_D;
        private static int RANSAC_K, RANSAC_N;
        private static int TSAI_SKIP;

        private static bool _paramsLoaded = false;

        private static RAWImage _origImg;

        public static bool ParamsLoaded {
            get { return _paramsLoaded; }
        }

        static TsaiPtFinder()
        {
            LoadParameters();
        }

        public static void LoadParameters()
        {
            _paramsLoaded = false;

            GRID_UNIT = Constants.get<double>("vision", "GRID_UNIT");
            TSAI_SKIP = Constants.get<int>("vision", "TSAI_SKIP");
            ROW_Y_THRES = Constants.get<int>("vision", "ROW_Y_THRES");
            TSAIPT_MIN_AREA = Constants.get<int>("vision", "TSAIPT_MIN_AREA");
            TSAIPT_MAX_AREA = Constants.get<int>("vision", "TSAIPT_MAX_AREA");
            CALIB_SQ_MARGIN = Constants.get<int>("vision", "CALIB_SQ_MARGIN");
            EDGE_NEIGHBOR_DIST = Constants.get<int>("vision", "EDGE_NEIGHBOR_DIST");  // 11; // in pixels
            Y_X_VERTICALITY_THRES = Constants.get<double>("vision", "Y_X_VERTICALITY_THRES");
            CALIB_SQ_COLOR = Color.FromArgb(Constants.get<int>("vision", "CALIB_SQ_COLOR"));

            // RANSAC
            RANSAC_T = Constants.get<double>("vision", "RANSAC_T");
            RANSAC_D = Constants.get<double>("vision", "RANSAC_D");
            RANSAC_K = Constants.get<int>("vision", "RANSAC_K");
            RANSAC_N = Constants.get<int>("vision", "RANSAC_N");

            _paramsLoaded = true;
        }

        public static void LoadImage(RAWImage img)
        {
            _origImg = img;
        }

        public static List<Pair<Point, DPoint>> FindTsaiPts(Blob[] blobs, DPoint offset)
        {
            
            // will hold tsai points: image <-> world
            List<Pair<Point, DPoint>> tsaiPoints = new List<Pair<Point, DPoint>>();

            // only keep blobs that are tsai points
            List<Blob> lstSquares = new List<Blob>();
            int i;
            for (i = 0; i < blobs.Length; i++)
                if (blobs[i] != null && blobs[i].Area >= TSAIPT_MIN_AREA && blobs[i].Area <= TSAIPT_MAX_AREA)
                    lstSquares.Add(blobs[i]);

            /* Order calibration squares into a grid */

            // sort by Y coordinate
            lstSquares.Sort(_blobComparerY);

            // Debug output
            Console.Write("Sorted by Y (" + lstSquares.Count.ToString() + " total): ");
            foreach (Blob sq in lstSquares)
                Console.Write(sq.BlobID + " ");
            Console.WriteLine();

            List<Blob> row = new List<Blob>();

            i = 0; // row counter
            while (lstSquares.Count > 0)
            {
                row.Clear();
				bool nextRow = false;
                
				// This is part of NO TSAI_COLS implementation
                /*row.Add(lstSquares[lstSquares.Count - 1]);
                /lstSquares.RemoveAt(lstSquares.Count - 1);

				float avgCenterY = row[0].CenterY;
				int prevCenterY = row[0].CenterY;

				while (!nextRow && lstSquares.Count > 0)
				{
					Blob b = lstSquares[lstSquares.Count - 1];
					nextRow = Math.Abs(b.CenterY - avgCenterY) > ROW_Y_THRES;
					if (!nextRow)
					{
						row.Add(b);
						prevCenterY = b.CenterY;
						avgCenterY = (b.CenterY + prevCenterY) / 2;
						lstSquares.RemoveAt(lstSquares.Count - 1);
					}
				}*/

                int TSAI_COLS = Constants.get<int>("vision", "TSAI_COLS");
                for (int j = 0; j < TSAI_COLS; j++)
                {
                    if (lstSquares.Count == 0) continue;
                    row.Add(lstSquares[lstSquares.Count - 1]);
                    lstSquares.RemoveAt(lstSquares.Count - 1);
                }

                // sort row by X coordinate
                row.Sort(_blobComparerX);

                // Debug output
                Console.Write("Row {0:G} sorted by X: ", i);
				foreach (Blob sq in row)
				{
					Console.Write(sq.BlobID + " ");
				}
                Console.WriteLine();

                if (i % TSAI_SKIP != 0)
                {
                    Console.WriteLine("Skipping row i = " + i.ToString());
                    i++;
                    continue; // cut down on number of tsai points
                }


            

                //int j; // column counter
                for (int j = 0; j < row.Count; j+=TSAI_SKIP)
                {

                    // location of the calibration square in the image
                    Point sqTopLeft = new Point(Math.Max(row[j].Left - CALIB_SQ_MARGIN, 0),
                                                Math.Max(row[j].Top - CALIB_SQ_MARGIN, 0));
                    Point sqBottRight = new Point(Math.Min(row[j].Right + CALIB_SQ_MARGIN, _origImg.Width),
                                                  Math.Min(row[j].Bottom + CALIB_SQ_MARGIN, _origImg.Height));
                    Rectangle square = new Rectangle(sqTopLeft, new Size(sqBottRight.X - sqTopLeft.X, sqBottRight.Y - sqTopLeft.Y));

                    
                    // prepare bitmap with one individual square
                    byte[] rawData = new byte[square.Height * square.Width * 3];
                    int k = 0;
                    for (int ii = square.Top; ii < square.Bottom; ii++)
                    {
                        for (int jj = square.Left; jj < square.Right; jj++)
                        {
                            rawData[k++] = _origImg.rawData[(ii * _origImg.Width + jj) * 3];     // B
                            rawData[k++] = _origImg.rawData[(ii * _origImg.Width + jj) * 3 + 1]; // G
                            rawData[k++] = _origImg.rawData[(ii * _origImg.Width + jj) * 3 + 2]; // R
                        }
                    }
                    RAWImage sqImg = new RAWImage(rawData, square.Width, square.Height, 1);

                    Point[] corners = detectCorners(sqImg);
                    
                    // find real-world coords for each corner
                    Pair<Point, DPoint> tsaiPt;
                    if (corners[TOP_LEFT] == new Point(-1, -1))
                    {
                        // default location of point (for manual placement)
                        corners[TOP_LEFT].Y = CALIB_SQ_MARGIN;
                        corners[TOP_LEFT].X = CALIB_SQ_MARGIN;
                    }
                    tsaiPt = new Pair<Point, DPoint>(
                                        new Point(square.Left + corners[TOP_LEFT].Y, square.Top + corners[TOP_LEFT].X),
                                        new DPoint(TsaiCalibrator.ORIGIN_OFFSET_X + offset.wx + j * 2 * GRID_UNIT + GRID_UNIT, 
                                                   TsaiCalibrator.ORIGIN_OFFSET_Y + offset.wy + i * 2 * GRID_UNIT + GRID_UNIT));
                        tsaiPoints.Add(tsaiPt);

                        if (corners[TOP_RIGHT] == new Point(-1, -1))
                        {
                            // default location of point (for manual placement)
                            corners[TOP_RIGHT].Y = square.Width - CALIB_SQ_MARGIN;
                            corners[TOP_RIGHT].X = CALIB_SQ_MARGIN;
                        }
                        tsaiPt = new Pair<Point, DPoint>(
                                        new Point(square.Left + corners[TOP_RIGHT].Y, square.Top + corners[TOP_RIGHT].X),
                                        new DPoint(TsaiCalibrator.ORIGIN_OFFSET_X + offset.wx + j * 2 * GRID_UNIT,
                                                   TsaiCalibrator.ORIGIN_OFFSET_Y + offset.wy + i * 2 * GRID_UNIT + GRID_UNIT));
                        tsaiPoints.Add(tsaiPt);

                        if (corners[BOTTOM_LEFT] == new Point(-1, -1))
                        {
                            // default location of point (for manual placement)
                            corners[BOTTOM_LEFT].Y = CALIB_SQ_MARGIN;
                            corners[BOTTOM_LEFT].X = square.Height - CALIB_SQ_MARGIN;
                        }
                        tsaiPt = new Pair<Point, DPoint>(
                                        new Point(square.Left + corners[BOTTOM_LEFT].Y, square.Top + corners[BOTTOM_LEFT].X),
                                        new DPoint(TsaiCalibrator.ORIGIN_OFFSET_X + offset.wx + j * 2 * GRID_UNIT + GRID_UNIT,
                                                   TsaiCalibrator.ORIGIN_OFFSET_Y + offset.wy + i * 2 * GRID_UNIT));
                        tsaiPoints.Add(tsaiPt);
                    
                    if (corners[BOTTOM_RIGHT] == new Point(-1, -1))
                    {
                        // default location of point (for manual placement)
                        corners[BOTTOM_RIGHT].Y = square.Width - CALIB_SQ_MARGIN;
                        corners[BOTTOM_RIGHT].X = square.Height - CALIB_SQ_MARGIN;
                    }
                        tsaiPt = new Pair<Point, DPoint>(
                                        new Point(square.Left + corners[BOTTOM_RIGHT].Y, square.Top + corners[BOTTOM_RIGHT].X),
                                        new DPoint(TsaiCalibrator.ORIGIN_OFFSET_X + offset.wx + j * 2 * GRID_UNIT,
                                                   TsaiCalibrator.ORIGIN_OFFSET_Y + offset.wy + i * 2 * GRID_UNIT));
                        tsaiPoints.Add(tsaiPt);
                    
                                                
                }

                i++;
            }

            return tsaiPoints;
        }

        private static Point[] detectCorners(RAWImage img)
        {

            // TODO: get rid of this step
            // for convenience copy image into a 2D color array
            Color[,] bmp = new Color[img.Height, img.Width];
            for (int i = 0; i < img.Height; i++)
                for (int j = 0; j < img.Width; j++)
                    bmp[i, j] = Color.FromArgb(img.RawData[i * (img.Width * 3) + j * 3 + 2], // R
                                               img.RawData[i * (img.Width * 3) + j * 3 + 1], // G
                                               img.RawData[i * (img.Width * 3) + j * 3]);    // B              

            // will hold edge pixels
            List<Point>[] edges = new List<Point>[4];
            for (int i = 0; i < 4; i++)
                edges[i] = new List<Point>();

            // edge indeces
            const int TOP = 0;
            const int LEFT = 1;
            const int BOTTOM = 2;
            const int RIGHT = 3;

            /* Mark edge pixels */

            byte[,] epixels = new byte[img.Height, img.Width];

            // initialize all pixels to non-edge
            for (int i = 0; i < img.Height; i++)
                for (int j = 0; j < img.Width; j++)
                    epixels[i, j] = 0;

            for (int i = 1; i < img.Height - 1; i++)
            {
                for (int j = 1; j < img.Width - 1; j++)
                {
                    // pixel outside of square
                    if (!isFillColor(bmp[i, j]))
                        continue;

                    // pixel inside and on the edge
                    if (!isFillColor(bmp[i - 1, j]) || !isFillColor(bmp[i, j + 1])
                        || !isFillColor(bmp[i, j - 1]) || !isFillColor(bmp[i + 1, j]))
                        epixels[i, j] = 1;
                }
            }
                    

            /* For each pixel get the closest neighbors and try to infer which 
             * edge it is on 
             */

            List<Point> neighbors = new List<Point>();
            Point v = new Point();
            for (int i = 1; i < img.Height - 1; i++)
            {
                for (int j = 1; j < img.Width - 1; j++)
                {
                    neighbors.Clear();
                    // radially advance away from the pixel, s = side of square at current dist.
                    for (int s = 3; s < EDGE_NEIGHBOR_DIST / 2; s += 2)
                    {
                        // look for neighbors (i.e. edge pixels at dist. d)
                        int i1, i2, j1, j2;
                        i1 = i - s / 2;  // top side
                        i2 = i + s / 2;  // bottom side
                        for (int jj = j - s / 2; jj < j + s / 2; jj++)
                        {
                            if (epixels[i1, jj] == 1)
                                neighbors.Add(new Point(i2 - i, jj - j)); // relative coords
                            if (epixels[i2, jj] == 1)
                                neighbors.Add(new Point(i2 - i, jj - j));
                        }
                        j1 = j - s / 2;  // left side
                        j2 = j + s / 2;  // right side
                        for (int ii = i - s / 2; ii < i + s / 2; ii++)
                        {
                            if (epixels[ii, j1] == 1)
                                neighbors.Add(new Point(ii - i, j1 - j));
                            if (epixels[ii, j2] == 1)
                                neighbors.Add(new Point(ii - i, j2 - j));
                        }
                    }

               
                        // compute the direction vector of the edge
                        v.X = v.Y = 0;
                        foreach (Point neighbor in neighbors)
                        {
                            v.X += Math.Abs(neighbor.X);
                            v.Y += Math.Abs(neighbor.Y);
                        }

                        // at this point, we have eliminated ambiguity that arises between 
                        // two adjacent edges.
                        // now, see which direction points outside of the square
                        Point epix = new Point(i, j);
                        if (v.X == 0 || (double)v.Y / (double)v.X > Y_X_VERTICALITY_THRES)
                        { // v is vertical
                            if (j - 1 >= 0 && j + 1 < img.Width &&
                                (!isFillColor(bmp[i, j - 1]) && isFillColor(bmp[i, j + 1])))
                            {
                                edges[LEFT].Add(epix);
                            }
                            else if (j - 1 >= 0 && j + 1 < img.Width &&
                                        (!isFillColor(bmp[i, j + 1])&& isFillColor(bmp[i, j - 1])))
                            {
                                edges[RIGHT].Add(epix);
                            }
                        }
                        else
                        { // v is horizontal
                            if (i - 1 >= 0 && i + 1 < img.Height &&
                                (!isFillColor(bmp[i - 1, j]) && isFillColor(bmp[i + 1, j])))
                            {
                                edges[TOP].Add(epix);
                            }
                            else if (i - 1 >= 0 && i + 1 < img.Width &&
                                (!isFillColor(bmp[i + 1, j]) && isFillColor(bmp[i - 1, j])))
                            {
                                edges[BOTTOM].Add(epix);
                            }
                        }


                     }
                
            }


            // corners stored here:
            Point[] corners = new Point[4];
            for (int e = 0; e < 4; e++)
                corners[e] = new Point(-1, -1);

            // weird blobs get detected with no edge pixels
            for (int e = 0; e < 4; e++)
                if (edges[e].Count <= 0 || edges[e].Count <= RANSAC_N)
                    return corners;

            double[][] models = new double[4][];
            for (int e = 0; e < 4; e++)
                models[e] = RANSAC(edges[e], RANSAC_T, RANSAC_D, RANSAC_K, RANSAC_N);
           
            
            // find intersections
            double x, y;

            

            // corner: TOP_LEFT
            if (models[TOP] != null && models[LEFT] != null)
            {
                x = (models[TOP][1] * models[LEFT][2] - models[LEFT][1] * models[TOP][2]) /
                    (models[TOP][0] * models[LEFT][1] - models[TOP][1] * models[LEFT][0]);
                if (models[LEFT][1] != 0)
                    y = -models[LEFT][0] * (models[TOP][1] * models[LEFT][2] - models[LEFT][1] * models[TOP][2]) /
                                      (models[LEFT][1] * (models[TOP][0] * models[LEFT][1] - models[TOP][1] * models[LEFT][0])) -
                        models[LEFT][2] / models[LEFT][1];
                else
                    y = -models[TOP][0] * (models[TOP][1] * models[LEFT][2] - models[LEFT][1] * models[TOP][2]) /
                                       (models[TOP][1] * (models[TOP][0] * models[LEFT][1] - models[TOP][1] * models[LEFT][0])) -
                        models[TOP][2] / models[TOP][1];
                Point pt = new Point((int)x, (int)y);
                if (pt.X >= 0 && pt.X < img.Width && pt.Y >= 0 && pt.Y < img.Height)
                    corners[TOP_LEFT] = pt;
            }

            // corner: TOP_RIGHT
            if (models[TOP] != null && models[RIGHT] != null)
            {
                x = (models[RIGHT][1] * models[TOP][2] - models[TOP][1] * models[RIGHT][2]) /
                    (models[RIGHT][0] * models[TOP][1] - models[RIGHT][1] * models[TOP][0]);
                if (models[TOP][1] != 0)
                    y = -models[TOP][0] * (models[RIGHT][1] * models[TOP][2] - models[TOP][1] * models[RIGHT][2]) /
                                      (models[TOP][1] * (models[RIGHT][0] * models[TOP][1] - models[RIGHT][1] * models[TOP][0])) -
                        models[TOP][2] / models[TOP][1];
                else
                    y = -models[RIGHT][0] * (models[RIGHT][1] * models[TOP][2] - models[TOP][1] * models[RIGHT][2]) /
                                       (models[RIGHT][1] * (models[RIGHT][0] * models[TOP][1] - models[RIGHT][1] * models[TOP][0])) -
                        models[RIGHT][2] / models[RIGHT][1];
                Point pt = new Point((int)x, (int)y);
                if (pt.X >= 0 && pt.X < img.Width && pt.Y >= 0 && pt.Y < img.Height)
                    corners[TOP_RIGHT] = pt;
            }

            // corner: BOTTOM_LEFT
            if (models[BOTTOM] != null && models[LEFT] != null)
            {
                x = (models[LEFT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[LEFT][2]) /
                    (models[LEFT][0] * models[BOTTOM][1] - models[LEFT][1] * models[BOTTOM][0]);
                if (models[BOTTOM][1] != 0)
                    y = -models[BOTTOM][0] * (models[LEFT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[LEFT][2]) /
                                      (models[BOTTOM][1] * (models[LEFT][0] * models[BOTTOM][1] - models[LEFT][1] * models[BOTTOM][0])) -
                        models[BOTTOM][2] / models[BOTTOM][1];
                else
                    y = -models[LEFT][0] * (models[LEFT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[LEFT][2]) /
                                       (models[LEFT][1] * (models[LEFT][0] * models[BOTTOM][1] - models[LEFT][1] * models[BOTTOM][0])) -
                        models[LEFT][2] / models[LEFT][1];
                Point pt = new Point((int)x, (int)y);
                if (pt.X >= 0 && pt.X < img.Width && pt.Y >= 0 && pt.Y < img.Height)
                    corners[BOTTOM_LEFT] = pt;

            }
            // corner: BOTTOM_RIGHT
            if (models[BOTTOM] != null && models[RIGHT] != null)
            {
                x = (models[RIGHT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[RIGHT][2]) /
                  (models[RIGHT][0] * models[BOTTOM][1] - models[RIGHT][1] * models[BOTTOM][0]);
                if (models[BOTTOM][1] != 0)
                    y = -models[BOTTOM][0] * (models[RIGHT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[RIGHT][2]) /
                                      (models[BOTTOM][1] * (models[RIGHT][0] * models[BOTTOM][1] - models[RIGHT][1] * models[BOTTOM][0])) -
                        models[BOTTOM][2] / models[BOTTOM][1];
                else
                    y = -models[RIGHT][0] * (models[RIGHT][1] * models[BOTTOM][2] - models[BOTTOM][1] * models[RIGHT][2]) /
                                       (models[RIGHT][1] * (models[RIGHT][0] * models[BOTTOM][1] - models[RIGHT][1] * models[BOTTOM][0])) -
                        models[RIGHT][2] / models[RIGHT][1];
                Point pt = new Point((int)x, (int)y);
                if (pt.X >= 0 && pt.X < img.Width && pt.Y >= 0 && pt.Y < img.Height)
                    corners[BOTTOM_RIGHT] = pt;
            }

            return corners;

        }

        // this function is abstracted away because it might be more complicated
        private static bool isFillColor(Color col)
        {
            if (col == CALIB_SQ_COLOR)
                return true;
            return false;
        }

        // returns model (a, b, c) for ax+by+c = 0
        // k: number of iterations
        // n: size of random subset
        // t: the fit error for a point to qualify into the consensus set
        // d: number of points fit better than t out of all points for a model to qualify
        private static double[] RANSAC(List<Point> data, double t, double d, int k, int n) {
            int iter = 0;
            double[] best_model = new double[3];
            bool best_model_set = false;
            List<Point> best_consensus_set;
            double best_error = double.MaxValue;
            
            Random rand = new Random();
            while (iter < k)  {
                List<Point> inliers = new List<Point>();
                bool[] chosen = new bool[data.Count];
                for (int i = 0; i < n; i++) {
                    int r;
                    while (chosen[r = rand.Next(data.Count)]);
                    chosen[r] = true;
                    inliers.Add(data[r]);
                }
                double[] model = fitModel(inliers);
                List<Point> consensus_set = new List<Point>(inliers);

                for (int i = 0; i < data.Count; i++) {
                    if (chosen[i]) continue;
                    double ptError;
                    // check for vertical line model
                    if (model[1] != 0)
                        ptError = Math.Abs(data[i].Y - (-model[0] * data[i].X - model[2]) / model[1]);
                    else
                        ptError = Math.Abs(data[i].X + model[2]);
                    if (ptError < t) {
                        consensus_set.Add(data[i]);
                    }
                }

              
                if (consensus_set.Count > (int)(RANSAC_D*data.Count)) {
                    double[] better_model = fitModel(consensus_set);
                    double error = 0;
                    foreach (Point pt in consensus_set)
                        if (model[1] != 0)
                            error += Math.Pow((pt.Y - (-better_model[0] * pt.X - better_model[2]) / model[1]), 2);
                        else
                            error += Math.Pow((pt.X + model[2]), 2);
                    if (error < best_error) {
                        for (int i = 0; i < best_model.Length; i++)
                            best_model[i] = better_model[i];
                        best_model_set = true;

                        best_consensus_set = new List<Point>(consensus_set);

                        best_error = error;
                    }
                }

                iter++;
            }

            //return best_model, best_consensus_set, best_error
            return (best_model_set ? best_model : null);
        }

        private static double[] fitModel(List<Point> data) {
    
            // fit least sq. lines for each edge: beta*x + gamma * y + alpha = 0
            double beta;
            double alpha;
            double gamma = -1;

                // get mean x and mean y
                double meanX = 0;
                double meanY = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    meanX += data[i].X;
                    meanY += data[i].Y;
                }
                meanX /= data.Count;
                meanY /= data.Count;

                // use linear regression formula
                // \beta = \frac{\sum(x_i - \overline{x})(y_i - \overline{y})}
                //	       {\sum(x_i - \overline{x})^2}
                // \alpha = \overline{y} - \overline{x}\beta

                double a = 0, b = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    a += (data[i].X - meanX) * (data[i].Y - meanY);
                    b += (data[i].X - meanX) * (data[i].X - meanX);
                }
                if (b != 0)
                {
                    beta = a / b;
                    alpha = meanY - meanX * beta;
                }
                else
                {
                    gamma = 0;
                    beta = 1;
                    alpha = -meanX;
                }
                
                double[] model = new double[] { beta, gamma, alpha };

                return model;
        }
    }
}

