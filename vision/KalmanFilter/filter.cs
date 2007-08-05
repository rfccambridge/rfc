using System;
using System.Collections;
using System.Text;

namespace KalmanFilter
{
    class filter
    {
        tracker[] myTrackers = new tracker[11];

        public void initialize(int object_index, double x_init, double x_prime_init, double y_init, double y_prime_init, double init_state_doubt)
        {
            myTrackers[object_index] = new tracker();
            myTrackers[object_index].initialize(x_init, x_prime_init, y_init, y_prime_init, init_state_doubt);
        }

        public void initialize(int object_index, double x_init, double y_init, double init_state_doubt)
        {
            myTrackers[object_index] = new tracker();
            myTrackers[object_index].initialize(x_init, 0.0, y_init, 0.0, init_state_doubt);
        }

        public void initialize(int object_index, double x_init, double y_init)
        {
            myTrackers[object_index] = new tracker();
            myTrackers[object_index].initialize(x_init, 0.0, y_init, 0.0, 100.0);
        }

        public void update(int object_index, double delta_t, ArrayList observed_state)
        {
            myTrackers[object_index].update(delta_t, observed_state);
        }

        public void update(int object_index, double delta_t, double x, double y)
        {
            ArrayList observed_state = new ArrayList();
            observed_state.Add(x);
            observed_state.Add(0.0);
            observed_state.Add(y);
            observed_state.Add(0.0);

            update(object_index, delta_t, observed_state);
        }

        public ArrayList get_state(int object_index, double delta_t)
        {
            return myTrackers[object_index].predict_state(delta_t);
        }
    }
}
