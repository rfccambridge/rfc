using System;
using System.Collections.Generic;
using System.Text;

namespace RAWImageApp {
    public class RgbColor {
        public int R;
        public int G;
        public int B;

        

        public RgbColor() {

        }
        public RgbColor(int newR, int newG, int newB) {
            R = newR;
            G = newG;
            B = newB;
        }

        public void setRgb(int newR, int newG, int newB) {
            R = newR;
            G = newG;
            B = newB;
        }

        public bool isSimilar(RgbColor color2) {
            if (Math.Sqrt(Math.Pow(R - color2.R, 2) + Math.Pow(G - color2.G, 2) + Math.Pow(B - color2.B, 2)) < 100) {
                return true;
            } else {
                return false;
            }
        }

        public RgbColor add(RgbColor color2) {
            R += color2.R;
            G += color2.G;
            B += color2.B;
            return this;
        }

        public RgbColor divideBy(int n) {
            R /= n;
            G /= n;
            B /= n;
            return this;
        }

        public RgbColor multBy(int n) {
            RgbColor copyColor = new RgbColor(R * n, G * n, B * n);
            //R *= n;
            //G *= n;
            //B *= n;
            return copyColor;
        }

    }
}
