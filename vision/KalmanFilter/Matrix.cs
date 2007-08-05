using System;
using System.Collections;
using System.Text;

namespace KalmanFilter
{
    
    class Matrix    //[i][j] (row,column)
    {
        ArrayList myMatrix;
        public Matrix(int n, int m)
        {
            initialize(n, m, 0);
        }

        public Matrix(int n, int m, double r0)
        {
            initialize(n, m, r0);
        }

        public Matrix()   //Default Matrix is 1X1 identity matrix
        {
            initialize(1, 1, 1);
        }

        public void initialize(int n, int m, double r0)
        {
            myMatrix = new ArrayList();

            for (int j = 0; j < n; j++)
            {
                ArrayList a = new ArrayList();
                for (int i = 0; i < m; i++)
                {
                    a.Add(r0);
                }
                myMatrix.Add(a);
            }
        }

        public int nRow()
        {
            return myMatrix.Count;
        }

        public int nCol()
        {
            // assert(numRows() != 0)
            return ((ArrayList)myMatrix[0]).Count;
        }

        public double at(int i, int j) //public int at(int i, int j)
        {
            return (double)((ArrayList)myMatrix[i])[j]; //return ((ArrayList)myMatrix[i])[j];
        }

        public void set(int i, int j, double r0)
        {
            ((ArrayList)myMatrix[i])[j] = r0;
        }

        public ArrayList multiply(ArrayList vec) // public Matrix multiply(ArrayList vec)
        {
            //assert(vec.Count == myMatrix.Count)
            //assert(vec.Count != 0)
            //assert(((ArrayList)myMatrix[0].Count) != 0)

            ArrayList result = new ArrayList();

            for(int i = 0; i < nRow() ; i++)
            {
                result.Add(0.0);
                for(int j = 0; j < nCol() ; j++)
                {
                    result[i] = (double)result[i] + (at(i, j) * (double)vec[j]); //result[i] += at(i,j) * vec[j];
                }
            }
            return result;
        }

        public Matrix multiply(Matrix m)
        {
            //assert(myMatrix[0].Capacity == m.Capacity)    
            //assert(m.Capacity != 0)                       //assert(myMatrix[0].Capacity != 0) 
            //assert(m[0].Capacity != 0)
            //assert(myMatrix.Capacity != 0)
            
            Matrix result = new Matrix(nRow(),m.nCol(),0);  //????

            for(int i = 0; i < nRow() ; i++)
            {
                for(int j = 0; j < nCol() ; j++)
                {
                    for(int k =0 ; k < nCol() ; k++)
                        result.set(i,j,(result.at(i,j) + at(i, k) * m.at(k, j))); //myMatrix[i][j] += at(i,k) * m.at(k,j);
                }
            }

            return result;
        }

        public void identity(int n)
        {
            //assert(n > 0)
            initialize(n, n, 0); //initialize(n, n)
            for (int i = 0; i < n; i++)
            {
                set(i, i, 1);
            }
        }

        public void display()
        {
            for (int i = 0; i < this.nRow(); i++)
            {
                for (int j = 0; j < this.nCol(); j++)
                {
                    Console.Write(this.at(i, j) + " ");
                }
                Console.Write("\n");
            }
        }
/*
        public void display()
        {
            for(int i = 0; i < nRow() ; i++)
            {
                for(int j = 0; j < nCol(); j++)
                {
                    Console.WriteLine(at(i,j) + " ");
                }
                Console.WriteLine("\n");
            }
        }
*/
        public bool check_valid()
        {
            if(nRow() == 0)  //#rows = 0
                return false;
            int n = nCol();
            if(n == 0)                  //#columns = 0
                return false;
            for(int i = 0 ; i < nRow(); i++)
            {
                if(((ArrayList)myMatrix[i]).Count != n)  //rows have different lengths
                    return false;
            }
            return true;

        }

        public static Matrix directSum(Matrix a, Matrix b)
        {
            Matrix result = new Matrix(a.nRow() + b.nRow(), a.nCol() + b.nCol(), 0);

            for(int i = 0; i < a.nRow(); i++)
            {
                for(int j = 0 ; j < a.nCol() ; j++)
                {
                    result.set(i, j, a.at(i,j));
                }
            }

            for(int i =  0 ; i < b.nRow(); i++)
            {
                for(int j = 0 ; j < b.nCol(); j++)
                {
                    result.set(i + a.nRow(), j + a.nCol(), b.at(i, j));
                }
            }

            return result;
        }

        public Matrix multiply(double c)
        {
            Matrix result = new Matrix(nRow(), nCol(), 0);
            for(int i = 0; i < nRow(); i++)
            {
                for(int j = 0; j < nCol(); j++)
                {
                    result.set(i, j, c*at(i, j));
                }
            }

            return result;
  
        }

        public Matrix submatrix(Matrix a, int i, int j, int m, int n)
        {
            Matrix result = new Matrix(m, n, 0);

            for(int k = i; k < a.nCol(); k++)
            {
                for(int l = j; l < a.nRow(); l++)
                {
                    result.set(k-i+1, l-i+1, a.at(i, j)); 
                }
            }           
 
            return result;
        }

        public Matrix transpose()
        {
            Matrix result = new Matrix(nRow(), nCol(), 0);
            for (int i = 0; i < nRow(); i++)
                for (int j = 0; j < nCol(); j++)
                       result.set(i, j, at(j, i));

            return result;
        }

        public static ArrayList addVector(ArrayList v1, ArrayList v2)
        {
            //v1.Count == v2.Count 

            ArrayList result = new ArrayList();
            for(int i = 0; i < v1.Count; i++) 
            {
                result.Add(0.0);
                //result[i] = (((double)(v1[i])) + ((double)(v2[i])));  //result[i] = (double)v1[i] + v2[i];
                result[i] = (double)v1[i] + (double)v2[i];
            }
            
            return result;
        }

        public static ArrayList multiplyByScalar(ArrayList v, double c)
        {
            ArrayList result = new ArrayList();
            for(int i = 0; i < v.Count; i++)
            {
                result.Add(c * (double)v[i]);   //result[i]=c*v[i];
            }
            
            return result;
        }

        public Matrix sum(Matrix b)
        {
            //nCol() == b.nCol()
            //nRow() == b.nRow()

            Matrix result = new Matrix(nRow(), nCol(), 0);

            for (int i = 0; i < nRow(); i++)
            {
                for (int j = 0; j < nCol(); j++)
                {
                    result.set(i, j, at(i,j) + b.at(i,j)); //result[i][j] = a[i][j] + b[i][j];
                }
            }

            return result;
        }

        public Matrix minor(int iRow, int iCol)
        {
            Matrix result = new Matrix(nRow() - 1, nCol() - 1);
            int m = 0, n = 0;
            for (int i = 0; i < nRow(); i++)
            {
                if (i == iRow)
                    continue;
                n = 0;
                for (int j = 0; j < nCol(); j++)
                {
                    if (j == iCol)
                        continue;
                    result.set(m, n, at(i, j));//result[m][n] = matrix[i][j];
                    n++;
                }
                m++;
            }
            return result;
        }

        public double determinent()
        {
            //Console.WriteLine("Getting Det of : ");
            //display();
            //matrix.nRow() == matrix.nCol()
            double result = 0;

            if (nRow() == 1)
                return at(0, 0);
            //if (matrix.nRow() == 2)
            //    return matrix.at(0, 0) * matrix.at(1, 1) - matrix.at(1, 0) * matrix.at(0, 1);
            for (int j = 0; j < nCol(); j++)
                result += (at(0, j) * minor(0, j).determinent() * (int)Math.Pow(-1, 0 + j));
            return result;
        }

		public Matrix adjoint()
		{
			//a.nRow() == a.nCol()
            Matrix AdjointMatrix = new Matrix(nRow(), nCol());
            for (int i = 0; i < this.nRow(); i++)
                for (int j = 0; j < this.nCol(); j++)
                    AdjointMatrix.set(i, j, (double)Math.Pow(-1, i + j) * (minor(i, j).determinent()));   //AdjointMatrix.at(i, j) = Math.Pow(-1, i + j) * (minor(this, i, j).determinent());
            AdjointMatrix = AdjointMatrix.transpose();
			return AdjointMatrix;
		}

        public Matrix inverse()
        {
            //nRow() == nCol()
            //determinent() !== 0
            return (adjoint().multiply(1.0 / (double)determinent()));     //return (a.adjoint() / a.determinent());
        }

        public Matrix subtract(Matrix b)
        {
            //nRow() == b.nRow()
            //nCol() == b.nCol()

            Matrix result = new Matrix(nRow(), nCol(), 0);

            for (int i = 0; i < nRow(); i++)
            {
                for (int j = 0; j < nCol(); j++)
                {
                    result.set(i, j, at(i, j) + b.at(i, j));    //result[i][j] = a[i][j] + b[i][j];
                }
            }

            return result;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            return a.multiply(b);
        }

        public static ArrayList operator *(Matrix a, ArrayList b)
        {
            return a.multiply(b);
        }

        public static Matrix operator *(Matrix a, double b)
        {
            return a.multiply(b);
        }

        public static Matrix operator *(double b, Matrix a)
        {
            return a.multiply(b);
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            return a.sum(b); 
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            return a.subtract(b);
        }

        /*public static ArrayList operator *(ArrayList a, double b)
        {
            return multiplyByScalar(a, b);
        }*/

        /*public static ArrayList operator +(ArrayList a, ArrayList b)
        {
            return addVector(a, b);
        }*/

        /*public static ArrayList operator -(ArrayList a, ArrayList b)
        {
            return addVector(a, multiplyByScalar(b, -1.0));
        }*/
        


    }

}
