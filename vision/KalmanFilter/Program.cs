using System;
using System.Collections;
using System.Windows.Forms;

namespace KalmanFilter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void pAL(ArrayList a)
        {
            for (int i = 0; i < a.Count; i++)
            {
                Console.Write(a[i]);
                Console.Write(", ");
            }
            Console.Write("\n");
        }

        static double erfi(double z)
        {
            return ( Math.Sqrt(Math.PI)/2.0 )* (z + Math.PI * Math.Pow(z,3.0)/12.0 + 7.0*Math.Pow(Math.PI,2.0)*Math.Pow(z,5.0)/480.0 + 127.0* Math.Pow(Math.PI, 3.0) *Math.Pow(z, 7.0)/ 40320.0 + 4369.0* Math.Pow(Math.PI, 4.0) *Math.Pow(z, 9.0)/ 5806080.0);
        }

        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Console.WriteLine("George: hello robin");
            filter george = new filter();
            george.initialize(0, 0, 0, 0, 0, 100);
            Console.WriteLine("Initial State");
            pAL(george.get_state(0, 0));
            Console.WriteLine("Initial P has doubt = 100.0");

            Random n = new Random();
                                    
            for (double i = 1.0; i < 300.0; i = i + 1.0)
            {
                double e1 = n.NextDouble();
                double e2 = n.NextDouble();
                george.update(0, 1, i + erfi(2.0 * e1 - 1.0) * 0.2, i + erfi(2.0 * e2 - 1.0) * 0.2);
                Console.Write(i);
                Console.Write(", ");
                Console.Write(i + erfi(2.0 * e1 - 1.0) * 0.2);
                Console.Write(", ");
                Console.Write(i + erfi(2.0 * e2 - 1.0) * 0.2);
                Console.Write(", ");
                pAL(george.get_state(0, 0));
            }
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
    }
}