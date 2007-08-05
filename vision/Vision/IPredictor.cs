using System;
using System.Collections.Generic;
using System.Text;

namespace Vision {
    public interface IPredictor {
        // locations of ball and robots now
        GameObjects getGameObjects();

        // locations of ball and robots in t seconds as predicted by e.g. Kalman filter
        // NOT IMPLEMENTED YET
        //GameObjects getGameObjects(int t);

    }
}
