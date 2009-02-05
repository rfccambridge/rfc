using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.CoreRobotics;
using Navigation;

namespace Robocup.MotionControl
{
    /// <summary>
    /// Takes an INavigator and an IMovement and puts them together to create an IMotionPlanner
    /// </summary>
    abstract public class NavigationPlanner : IMotionPlanner
    {
        private INavigator navigator;
        private IMovement movement;
        public NavigationPlanner(INavigator navigator, IMovement movement)
        {
            this.navigator = navigator;
            this.movement = movement;
        }

        public MotionPlanningResults PlanMotion(int id, RobotInfo desiredState, IPredictor predictor, double avoidBallRadius)
        {
            NavigationResults results = navigator.navigate(id, predictor.getCurrentInformation(id).Position,
                desiredState.Position, predictor.getOurTeamInfo().ToArray(), predictor.getTheirTeamInfo().ToArray(),
                predictor.getBallInfo(), avoidBallRadius);
            WheelSpeeds speeds = movement.calculateWheelSpeeds(predictor, id, predictor.getCurrentInformation(id), results);
            return new MotionPlanningResults(speeds);
        }

        public void DrawLast(System.Drawing.Graphics g, ICoordinateConverter c)
        {
            navigator.drawLast(g, c);
        }

        public void ReloadConstants() {}
    }
    /*public class CurrentPlanner : NavigationPlanner
    {
        public CurrentPlanner()
            :
            base(new Navigation.Current.CurrentNavigator(), new FourWheeledMovement()) { }
    }*/
}
