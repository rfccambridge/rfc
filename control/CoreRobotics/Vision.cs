using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using System.Threading;

namespace Robocup.CoreRobotics
{
    public class Vision
    {
        public event EventHandler<EventArgs<VisionMessage>> MessageReceived;
        public event EventHandler ErrorOccured;

        bool verbose = false;
        SSLVision.RoboCupSSLClientManaged _client;
        bool _clientOpen = false;
        bool _running = false;
        Thread _visionThread;

        public void Connect(string hostname, int port)
        {
            if (_clientOpen)
                throw new ApplicationException("Client already open.");

            _client = new SSLVision.RoboCupSSLClientManaged(port, hostname, "");
            _client.open(true);
            _clientOpen = true;
        }

        public void Disconnect()
        {
            if (!_clientOpen)
                throw new ApplicationException("Client not open.");
            if (_running)
                throw new ApplicationException("Must stop before closing client.");

            _client.close();
            _clientOpen = false;
        }

        public void Start()
        {
            if (_running)
                throw new ApplicationException("Vision already running!");
            if (!_clientOpen)
                throw new ApplicationException("Must open client before starting.");

            // Have to create a new Thread object every time
            _visionThread = new Thread(new ThreadStart(loop));
            _visionThread.Start();
            _running = true;
        }

        public void Stop()
        {
            if (!_running)
                throw new ApplicationException("Vision not running!");

            _visionThread.Abort();
            _running = false;
        }
        
        private void loop()
        {
            SSLVision.SSL_WrapperPacketManaged packet = new SSLVision.SSL_WrapperPacketManaged();            

            while (true)
            {
                if (!_client.receive(packet))
                    continue;

                //see if the packet contains a robot detection frame:
                if (packet.has_detection())
                {
                    SSLVision.SSL_DetectionFrameManaged detection = packet.detection();
                    //Display the contents of the robot detection results:
                    //double t_now = GetTimeSec();
                    double t_now = 0;

                    //Frame info:
                    int balls_n = detection.balls_size();
                    int robots_blue_n = detection.robots_blue_size();
                    int robots_yellow_n = detection.robots_yellow_size();

                    VisionMessage msg = new VisionMessage((int)detection.camera_id());
                    
                    //Ball info:
                    float maxBallConfidence = float.MinValue;
                    for (int i = 0; i < balls_n; i++)
                    {
                        SSLVision.SSL_DetectionBallManaged ball = detection.balls(i);

                        if (ball.has_confidence() && ball.confidence() > maxBallConfidence)
                        {
                            msg.Ball = new BallInfo(ConvertFromSSLVisionCoords(new Vector2(ball.x(), ball.y())));
                            maxBallConfidence = ball.confidence();
                        }
                    }
                    
                    //Blue robot info:
                    for (int i = 0; i < robots_blue_n; i++)
                    {
                        SSLVision.SSL_DetectionRobotManaged robot = detection.robots_blue(i);
                        msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id(), Team.Blue,
                            ConvertFromSSLVisionCoords(new Vector2(robot.x(), robot.y())), robot.orientation()));
                    }
                    
                    //Yellow robot info:
                    for (int i = 0; i < robots_yellow_n; i++)
                    {
                        SSLVision.SSL_DetectionRobotManaged robot = detection.robots_yellow(i);
                        msg.Robots.Add(new VisionMessage.RobotData((int)robot.robot_id(), Team.Yellow,
                          ConvertFromSSLVisionCoords(new Vector2(robot.x(), robot.y())), robot.orientation()));
                    }

                    if (MessageReceived != null)
                        MessageReceived(this, new EventArgs<VisionMessage>(msg));                    
                }

                //see if packet contains geometry data:
                /*
                if (packet.has_geometry())
                {
                    SSLVision.SSL_GeometryDataManaged geom = packet.geometry();
                    if (verbose) Console.Write(String.Format("-[Geometry Data]-------\n"));

                    SSLVision.SSL_GeometryFieldSizeManaged field = geom.field();
                    if (verbose) Console.Write(String.Format("Field Dimensions:\n"));
                    if (verbose) Console.Write(String.Format("  -line_width={0:G} (mm)\n", field.line_width()));
                    if (verbose) Console.Write(String.Format("  -field_length={0:G} (mm)\n", field.field_length()));
                    if (verbose) Console.Write(String.Format("  -field_width={0:G} (mm)\n", field.field_width()));
                    if (verbose) Console.Write(String.Format("  -boundary_width={0:G} (mm)\n", field.boundary_width()));
                    if (verbose) Console.Write(String.Format("  -referee_width={0:G} (mm)\n", field.referee_width()));
                    if (verbose) Console.Write(String.Format("  -goal_width={0:G} (mm)\n", field.goal_width()));
                    if (verbose) Console.Write(String.Format("  -goal_depth={0:G} (mm)\n", field.goal_depth()));
                    if (verbose) Console.Write(String.Format("  -goal_wall_width={0:G} (mm)\n", field.goal_wall_width()));
                    if (verbose) Console.Write(String.Format("  -center_circle_radius={0:G} (mm)\n", field.center_circle_radius()));
                    if (verbose) Console.Write(String.Format("  -defense_radius={0:G} (mm)\n", field.defense_radius()));
                    if (verbose) Console.Write(String.Format("  -defense_stretch={0:G} (mm)\n", field.defense_stretch()));
                    if (verbose) Console.Write(String.Format("  -free_kick_from_defense_dist={0:G} (mm)\n", field.free_kick_from_defense_dist()));
                    if (verbose) Console.Write(String.Format("  -penalty_spot_from_field_line_dist={0:G} (mm)\n", field.penalty_spot_from_field_line_dist()));
                    if (verbose) Console.Write(String.Format("  -penalty_line_from_spot_dist={0:G} (mm)\n", field.penalty_line_from_spot_dist()));

                    int calib_n = geom.calib_size();
                    for (int i = 0; i < calib_n; i++)
                    {
                        SSLVision.SSL_GeometryCameraCalibrationManaged calib = geom.calib(i);
                        if (verbose) Console.Write(String.Format("Camera Geometry for Camera ID {0:G}:\n", calib.camera_id()));
                        if (verbose) Console.Write(String.Format("  -focal_length={0:F2}\n", calib.focal_length()));
                        if (verbose) Console.Write(String.Format("  -principal_point_x={0:F2}\n", calib.principal_point_x()));
                        if (verbose) Console.Write(String.Format("  -principal_point_y={0:F2}\n", calib.principal_point_y()));
                        if (verbose) Console.Write(String.Format("  -distortion={0:F2}\n", calib.distortion()));
                        if (verbose) Console.Write(String.Format("  -q0={0:F2}\n", calib.q0()));
                        if (verbose) Console.Write(String.Format("  -q1={0:F2}\n", calib.q1()));
                        if (verbose) Console.Write(String.Format("  -q2={0:F2}\n", calib.q2()));
                        if (verbose) Console.Write(String.Format("  -q3={0:F2}\n", calib.q3()));
                        if (verbose) Console.Write(String.Format("  -tx={0:F2}\n", calib.tx()));
                        if (verbose) Console.Write(String.Format("  -ty={0:F2}\n", calib.ty()));
                        if (verbose) Console.Write(String.Format("  -tz={0:F2}\n", calib.tz()));

                        if (calib.has_derived_camera_world_tx() && calib.has_derived_camera_world_ty() && calib.has_derived_camera_world_tz())
                        {
                            if (verbose) Console.Write(String.Format("  -derived_camera_world_tx={0:F}\n", calib.derived_camera_world_tx()));
                            if (verbose) Console.Write(String.Format("  -derived_camera_world_ty={0:F}\n", calib.derived_camera_world_ty()));
                            if (verbose) Console.Write(String.Format("  -derived_camera_world_tz={0:F}\n", calib.derived_camera_world_tz()));
                        }
                    }
                }*/
            }
        }

        private void loopErrorHandler(IAsyncResult result)
        {
            if (ErrorOccured != null)
                ErrorOccured(this, new EventArgs());
        }

        private Vector2 ConvertFromSSLVisionCoords(Vector2 v)
        {
            return new Vector2(v.X / 1000, v.Y / 1000);
        }

        private void printRobotInfo(SSLVision.SSL_DetectionRobotManaged robot)
        {
            if (verbose) Console.Write(String.Format("CONF={0,4:F2} ", robot.confidence()));
            if (robot.has_robot_id())
            {
                if (verbose) Console.Write(String.Format("ID={0,3:G} ", robot.robot_id()));
            }
            else
            {
                if (verbose) Console.Write(String.Format("ID=N/A "));
            }
            if (verbose) Console.Write(String.Format(" HEIGHT={0,6:F2} POS=<{1,9:F2},{2,9:F2}> ", robot.height(), robot.x(), robot.y()));
            if (robot.has_orientation())
            {
                if (verbose) Console.Write(String.Format("ANGLE={0,6:F3} ", robot.orientation()));
            }
            else
            {
                if (verbose) Console.Write(String.Format("ANGLE=N/A    "));
            }
            if (verbose) Console.Write(String.Format("RAW=<{0,8:F2},{1,8:F2}>\n", robot.pixel_x(), robot.pixel_y()));
        }
    }
}
