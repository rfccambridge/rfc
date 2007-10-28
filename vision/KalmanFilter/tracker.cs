using System;
using System.Collections;
using System.Text;

namespace KalmanFilter
{
    class tracker
    {
        private const int STATE_SIZE = 4;

        // Model parameters
        private const double STD_DEV_ACCEL = 0.1;
        private const double STD_DEV_POSITION = 0.2; 


        //public ArrayList x_hat;  //x_hat
        public CSML.Matrix x_hat;  //x_hat
       // Matrix P;      //Covariance
        CSML.Matrix P;      //Covariance

        public tracker()
        {
            initialize(0.0, 0.0, 0.0, 0.0, 100.0);
        }

        public tracker(double x, double y)
        {
            initialize(x, 0.0, y, 0.0, 100.0);
        }

        public tracker(double x, double y, double init_state_doubt)
        {
            initialize(x, 0.0, y, 0.0, init_state_doubt);
        }

        public tracker(double x, double x_prime, double y, double y_prime, double init_state_doubt)
        {
            initialize(x, x_prime, y, y_prime, init_state_doubt);
        }

        public void initialize(double x, double x_prime, double y, double y_prime, double init_state_doubt)
        {
            /*x_hat = new ArrayList();
            x_hat.Add(x);
            x_hat.Add(x_prime);
            x_hat.Add(y);
            x_hat.Add(y_prime);*/
            x_hat = new CSML.Matrix(4, 1);
            x_hat[1, 1] = new CSML.Complex(x);
            x_hat[2, 1] = new CSML.Complex(x_prime);
            x_hat[3, 1] = new CSML.Complex(y);
            x_hat[4, 1] = new CSML.Complex(y_prime);

            //P = new Matrix(4,4,0);
            //P.identity(4);
            P = CSML.Matrix.Identity(4);
            P = P * init_state_doubt;
        }

        public void update(double delta_t, ArrayList observed_state) //omit velocities
        {
            //ArrayList z = observed_state;               // observed state
            //ArrayList x_pred = predict_state(delta_t);  // predicted state
            
            //ArrayList y = new ArrayList();                 // measurement residual
            //Matrix S = new Matrix(4, 4);             // residual covariance
            
            //Matrix P_pred = predict_P(delta_t);      // predictred estimate covariance
            
            
            //Matrix K = new Matrix(4,4);
            //Matrix I = new Matrix(4,4);
            //I.identity(4);

            //y = Matrix.addVector(z,Matrix.multiplyByScalar(H().multiply(x_pred),-1.0));             // calculate measurement residual
            //S = H().multiply(P_pred).multiply(H().transpose()) + R();
            
            //K = P_pred.multiply(H().transpose()).multiply(S.inverse());
            
            //x_hat = Matrix.addVector(x_pred,K*y);
            //P = (I - K*H())*P_pred; 

            //Matrices T and S are helper matrices - they don't have definitions in the Kalman filter
            //ArrayList L is a helper

            /*Matrix H = Hf();
            Matrix Ht = Hf().transpose();
            Matrix R = Rf();

            Matrix S = H.multiply(P_pred.multiply(Ht)) + R;
            Matrix Si = S.inverse();

            Matrix K = P_pred.multiply(Ht.multiply(Si));

            ArrayList L = H.multiply(x_pred);
            ArrayList delta = Matrix.addVector(z, Matrix.multiplyByScalar(L, -1));

            x_hat = Matrix.addVector(x_pred, K.multiply(delta));

            Matrix T = I - K.multiply(H);
            P = T.multiply(P_pred);
             */

            CSML.Matrix z = new CSML.Matrix((double[])observed_state.ToArray(typeof(double)));
            CSML.Matrix x_pred = predict_state(delta_t);

            CSML.Matrix P_pred = predict_P(delta_t);

            CSML.Matrix I = CSML.Matrix.Identity(4);

            CSML.Matrix H = Hf();
            CSML.Matrix Ht = H.Transpose();
            CSML.Matrix R = Rf();

            CSML.Matrix S = H * (P_pred * Ht) + R;
            CSML.Matrix Si = S.Inverse();

            CSML.Matrix K = P_pred * (Ht * Si);

            x_hat = x_pred + K * (z - H * x_pred);

            P = (I - K * H) * P_pred;


        }

        //public ArrayList predict_state(double delta_t)
        public CSML.Matrix predict_state(double delta_t)
        {
            //display();
            CSML.Matrix F = Ff(delta_t);

            return F * x_hat;   // + B_k * u_k (which we leave out since we are not modeling control now)
            //return x_hat;
        }
        

        /*public Matrix predict_P(double delta_t)
        {
            //return F(delta_t).multiply(P).multiply(F(delta_t).transpose());  // + Q_k (which we leave out since we are not modeling random accelerations now)
            Matrix Q = new Matrix(4, 4);
            Q.identity(4);
            Q = Q * 0.00001;

            return P + Q;
        } */

        public CSML.Matrix predict_P(double delta_t) {
            CSML.Matrix F = Ff(delta_t);
            CSML.Matrix Ft = F.Transpose();


            CSML.Matrix Q = CSML.Matrix.Identity(4);
            Q = Q * (STD_DEV_ACCEL * STD_DEV_ACCEL);

            return F * (P * Ft) + Q;
            
            //return F(delta_t).multiply(P).multiply(F(delta_t).transpose());  // + Q_k (which we leave out since we are not modeling random accelerations now)
            
            

           // return P + Q;
        }

      /*  public static Matrix F(double delta_t)
        {
            Matrix m1 = new Matrix(2,2);
            m1.identity(2);
            m1.set(0,1,delta_t);
            Matrix m2 = new Matrix(2,2);
            m2.identity(2);
            m2.set(0,1,delta_t);
            return Matrix.directSum(m1,m2);
        }*/

        public static CSML.Matrix Ff(double delta_t) {

            CSML.Matrix M = CSML.Matrix.Identity(2);
            M[1, 2] = new CSML.Complex(delta_t);

           // CSML.Matrix M2 = CSML.Matrix.Identity(2);
          //  M2[1, 2] = new CSML.Complex(delta_t);

            // Direct sum implemented as a series of concatenations
            CSML.Matrix leftHalf = CSML.Matrix.VerticalConcat(M, CSML.Matrix.Zeros(2));
            CSML.Matrix rightHalf = CSML.Matrix.VerticalConcat(CSML.Matrix.Zeros(2), M);

            // Get acceleration (a_k = 0 + noise)
            Random rand = new Random();
            double a_k = 0 + STD_DEV_ACCEL * Math.Sqrt(2) * Helpers.erfi(2.0 * rand.NextDouble() - 1.0);

            CSML.Matrix halfG = new CSML.Matrix(2, 1);
            halfG[1, 1] = new CSML.Complex(delta_t * delta_t / 2);
            halfG[2, 1] = new CSML.Complex(delta_t);

            CSML.Matrix G = CSML.Matrix.VerticalConcat(halfG, halfG) * a_k;

            return CSML.Matrix.HorizontalConcat(leftHalf, rightHalf);
        }

       /* public static Matrix Rf()
        {
            Matrix ret = new Matrix();
            ret.identity(4);
            //return ret*(std_dev*std_dev);
            return ret * 100;
            
        }*/
        public static CSML.Matrix Rf() {
            CSML.Matrix R = CSML.Matrix.Identity(STATE_SIZE);

            // Adjust strength of believe into measurements HERE
            // [the smaller the factor the stronger the belief]
            return R * (STD_DEV_POSITION * STD_DEV_POSITION);
            //return ret * 0.0001;
            //return ret * 1000;

        }
      
       /* public static Matrix Hf()
        {
            Matrix ret = new Matrix(4,4,0);
            ret.set(0,0,1);
            ret.set(2,2,1);
            return ret;
        }*/

        public static CSML.Matrix Hf() {
            CSML.Matrix ret = new CSML.Matrix(STATE_SIZE, STATE_SIZE);
            ret[1, 1] = new CSML.Complex(1);
            ret[3, 3] = new CSML.Complex(1);
            return ret;
        }

      /*  public void display()
        {
            //x_hat!=null and p!=null 
            Console.WriteLine("State:");
            for (int i = 0; i < 4; i++)
            {
                Console.Write(x_hat[i]);
            }
            Console.Write("\n");
            Console.WriteLine("P:");
            P.display();
        }*/

    }
}
