using System;
using System.Collections;
using System.Text;

namespace KalmanFilter
{
    class tracker
    {
        const double std_dev = 0.2; 
        ArrayList x_hat;  //x_hat
        Matrix P;      //Covariance

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
            x_hat = new ArrayList();
            x_hat.Add(x);
            x_hat.Add(x_prime);
            x_hat.Add(y);
            x_hat.Add(y_prime);

            P = new Matrix(4,4,0);
            P.identity(4);
            P = P * init_state_doubt;
        }

        public void update(double delta_t, ArrayList observed_state) //omit velocities
        {
            ArrayList z = observed_state;               // observed state
            ArrayList x_pred = predict_state(delta_t);  // predicted state
            ArrayList y = new ArrayList();                 // measurement residual
            Matrix S = new Matrix(4, 4);             // residual covariance
            Matrix P_pred = predict_P(delta_t);      // predictred estimate covariance
            Matrix K = new Matrix(4,4);
            Matrix I = new Matrix(4,4);
            I.identity(4);

            y = Matrix.addVector(z,Matrix.multiplyByScalar(H().multiply(x_pred),-1.0));             // calculate measurement residual
            S = H().multiply(P_pred).multiply(H().transpose()) + R();
            K = P_pred.multiply(H().transpose()).multiply(S.inverse());
            x_hat = Matrix.addVector(x_pred,K*y);
            P = (I - K*H())*P_pred; 
        }

        public ArrayList predict_state(double delta_t)
        {
            //display();
            return F(delta_t).multiply(x_hat);   // + B_k * u_k (which we leave out since we are not modeling control now)
        }

        public Matrix predict_P(double delta_t)
        {
            return F(delta_t).multiply(P).multiply(F(delta_t).transpose());  // + Q_k (which we leave out since we are not modeling random accelerations now) 
        }

        public static Matrix F(double delta_t)
        {
            Matrix m1 = new Matrix(2,2);
            m1.identity(2);
            m1.set(0,1,delta_t);
            Matrix m2 = new Matrix(2,2);
            m2.identity(2);
            m2.set(0,1,delta_t);
            return Matrix.directSum(m1,m2);
        }

        public static Matrix R()
        {
            Matrix ret = new Matrix();
            ret.identity(4);
            return ret*(std_dev*std_dev);
        }
      
        public static Matrix H()
        {
            Matrix ret = new Matrix(4,4,0);
            ret.set(0,0,1);
            ret.set(2,2,1);
            return ret;
        }

        public void display()
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
        }

    }
}
