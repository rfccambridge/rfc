using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;

namespace KalmanFilter
{
    static class Program
    {
        // All output goes to this file
        const string OUTPUT_FILENAME = "kalman_out.txt";

       
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // All output goes to file OUTPUT_FILENAME
            TextWriter twOut = new StreamWriter(OUTPUT_FILENAME);

            filter f = new filter();
            f.initialize(0, 0, 0, 0, 0, 100);

            //twOut.WriteLine("Initial State: " + ALToString(f.get_state(0, 0)));
            twOut.WriteLine("Initial State: " + ALToString(f.get_state(0)));
            twOut.WriteLine("Initial P has doubt = 100.0");
            twOut.WriteLine();

            Random n = new Random();

            double x = 0, y = 0;

            for (double i = 1.0; i < 300.0; i = i + 1.0)
            {
                double e1 = n.NextDouble();
                double e2 = n.NextDouble();
                x = i + erfi(2.0 * e1 - 1.0) * 0.2;
                y = i + erfi(2.0 * e2 - 1.0) * 0.2;
                f.update(0, 1, x, y);

                twOut.Write(i + ":\t" +
                            String.Format("{0:G4}", i + erfi(2.0 * e1 - 1.0) * 0.2) + "\t" +
                            String.Format("{0:G4}", i + erfi(2.0 * e2 - 1.0) * 0.2) + "\t\t" +
                            //"STATE: " + ALToString(f.get_state(0,0)));
                            "STATE: " + ALToString(f.get_state(0)));
                
            }

            twOut.WriteLine();

            for (double i = 1.0; i < 200.0; i = i + 1.0)
            {
                double e1 = n.NextDouble();
                double e2 = n.NextDouble();
                f.update(0, 1, x + 2*i + erfi(2.0 * e1 - 1.0) * 0.2, y + 2*i + erfi(2.0 * e2 - 1.0) * 0.2);

                twOut.Write(i + ":\t" +
                            String.Format("{0:G4}", x + 2*i + erfi(2.0 * e1 - 1.0) * 0.2) + "\t" +
                            String.Format("{0:G4}", y + 2*i + erfi(2.0 * e2 - 1.0) * 0.2) + "\t\t" +
                            //"STATE: " + ALToString(f.get_state(0, 0)));
                            "STATE: " + ALToString(f.get_state(0)));
            }
            
            twOut.Close();

            //george.display();
            /*george.update(0, 1, 2, 2);
            ArrayList george_state = george.get_state(0, 1);
            Console.WriteLine("Output:");
            Console.WriteLine(george_state[0]);
            Console.WriteLine(george_state[2]);
            */
            /*Matrix a = new Matrix(3, 3, 0);
            a.set(0, 0, 1);
            a.set(0, 1, 2);
            a.set(0, 2, 3);
            a.set(1, 0, 4);
            a.set(1, 1, 5);
            a.set(1, 2, 6);
            a.set(2, 0, 7);
            a.set(2, 1, 8);
            a.set(2, 2, 10);*/
            /*Matrix a = new Matrix(2, 2, 0);
            a.set(0, 0, 1);
            a.set(0, 1, 2);
            a.set(1, 0, 3);
            a.set(1, 1, 4);*/
            /*Matrix a = new Matrix(1, 1, 0);
            a.set(0, 0, 1);*/
            /*
            Console.WriteLine("Matrix A is \n");

            a.display();
            */
            

            //Console.WriteLine("M(A)(1,0) = ");

            //a.minor(1, 0).display();


            //double det = a.determinent();

            //Console.WriteLine("The determinent of A is ... ");
            //Console.WriteLine(det);

            /*Matrix aInverse = a.inverse();

            Console.WriteLine("\n The inverse of Matrix A is\n");

            aInverse.display();

            
            Matrix aTranspose = a.transpose();

            Console.WriteLine("\n The transpose of Matrix A is\n");

            aTranspose.display();


            Matrix b = new Matrix(3, 3, 0);
            b.set(0, 0, 0);
            b.set(0, 1, 1);
            b.set(0, 2, 2);
            b.set(1, 0, 3);
            b.set(1, 1, 4);
            b.set(1, 2, 5);
            b.set(2, 0, 6);
            b.set(2, 1, 7);
            b.set(2, 2, 8);

            Console.WriteLine("\n Matrix B is \n");

            b.display();


            Matrix sum = a.sum(b);

            Console.WriteLine("\n The sum of A and B is \n");

            sum.display();

            Matrix product = a.multiply(b);

            Console.WriteLine("\n The product of A.B is \n");

            product.display();*/
        }

        static string ALToString(ArrayList arList) {
            string sOut = "";

            for (int i = 0; i < arList.Count; i++)
                sOut += String.Format("{0:G4}\t", ((CSML.Complex)arList[i]).Re);
            sOut += Environment.NewLine;

            return sOut;
        }

        static void printAL(ArrayList a) {

            for (int i = 0; i < a.Count; i++) {
                Console.Write("{0:G4}, ", a[i]);
            }
            Console.WriteLine();

        }
        
        static double erfi(double z) {
            return (Math.Sqrt(Math.PI) / 2.0) * (z + Math.PI * Math.Pow(z, 3.0) / 12.0 + 
                7.0 * Math.Pow(Math.PI, 2.0) * Math.Pow(z, 5.0) / 480.0 + 
                127.0 * Math.Pow(Math.PI, 3.0) * Math.Pow(z, 7.0) / 40320.0 + 
                4369.0 * Math.Pow(Math.PI, 4.0) * Math.Pow(z, 9.0) / 5806080.0);
        }
    }

}


