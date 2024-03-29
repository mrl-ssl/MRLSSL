﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using StillDesign.PhysX;
using MRL.SSL.CommonClasses;
using StillDesign.PhysX.MathPrimitives;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.IO;
using System.Drawing;
using System.Diagnostics;
namespace Simulator
{
    public class MRLSimpleSimulator : System.IDisposable
    {
        public RectangleF field = new RectangleF(4.045f, 3.025f, 8.09f, 6.05f);
        private Scene _scene;
        Thread _sendThread, _reciveThread;
        private Dictionary<int, SingleObjectState> blueRobots;
        private Dictionary<int, SingleObjectState> yellowRobots;
        private Dictionary<int, SingleObjectState> balls;
        private int _recievePort = 10013;
        private CommunicationController _networkManager;
        private string _aiName = "mrl-ai-pc";
        private int _sendPort = 10013;
        public Actor robotActor { get; set; }
        public Scene Scene
        {
            get { return _scene; }
            set { _scene = value; }
        }


        public string AiName
        {
            get { return _aiName; }
            set { _aiName = value; }
        }

        public int SendPort
        {
            get { return _sendPort; }
            set { _sendPort = value; }
        }

        public int RecievePort
        {
            get { return _recievePort; }
            set { _recievePort = value; }
        }

        public MRLSimpleSimulator()
        {
            addBall(0, Position2D.Zero);
        }
        private List<messages_robocup_ssl_detection.SSL_DetectionFrame> GenerateCameraModel(Camera cam0, Camera cam1, Camera cam2, Camera cam3)
        {
            messages_robocup_ssl_detection.SSL_DetectionFrame packet0 = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            messages_robocup_ssl_detection.SSL_DetectionFrame packet1 = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            messages_robocup_ssl_detection.SSL_DetectionFrame packet2 = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            messages_robocup_ssl_detection.SSL_DetectionFrame packet3 = new messages_robocup_ssl_detection.SSL_DetectionFrame();
            if (cam0 != null && cam1 != null && cam2 != null && cam3 != null)
            {
                if (yellowRobots != null)
                {
                    foreach (var item in yellowRobots.Keys.ToList())
                    {
                        if (cam0.IsInCamera(new Vector3((float)yellowRobots[item].Location.X, 0, (float)yellowRobots[item].Location.Y)))
                            packet0.robots_yellow.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)yellowRobots[item].Location.X * -1000,
                                y = (float)yellowRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (yellowRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam1.IsInCamera(new Vector3((float)yellowRobots[item].Location.X, 0, (float)yellowRobots[item].Location.Y)))
                            packet1.robots_yellow.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)yellowRobots[item].Location.X * -1000,
                                y = (float)yellowRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (yellowRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam2.IsInCamera(new Vector3((float)yellowRobots[item].Location.X, 0, (float)yellowRobots[item].Location.Y)))
                            packet2.robots_yellow.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)yellowRobots[item].Location.X * -1000,
                                y = (float)yellowRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (yellowRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam3.IsInCamera(new Vector3((float)yellowRobots[item].Location.X, 0, (float)yellowRobots[item].Location.Y)))
                            packet3.robots_yellow.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)yellowRobots[item].Location.X * -1000,
                                y = (float)yellowRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (yellowRobots[item].Angle.Value + 90) / 180.0)
                            });
                    }
                }
                if (blueRobots != null)
                {
                    foreach (var item in blueRobots.Keys.ToList())
                    {
                        if (cam0.IsInCamera(new Vector3((float)blueRobots[item].Location.X, 0, (float)blueRobots[item].Location.Y)))
                            packet0.robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)blueRobots[item].Location.X * -1000,
                                y = (float)blueRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (blueRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam1.IsInCamera(new Vector3((float)blueRobots[item].Location.X, 0, (float)blueRobots[item].Location.Y)))
                            packet1.robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)blueRobots[item].Location.X * -1000,
                                y = (float)blueRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (blueRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam2.IsInCamera(new Vector3((float)blueRobots[item].Location.X, 0, (float)blueRobots[item].Location.Y)))
                            packet2.robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)blueRobots[item].Location.X * -1000,
                                y = (float)blueRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (blueRobots[item].Angle.Value + 90) / 180.0)
                            });
                        if (cam3.IsInCamera(new Vector3((float)blueRobots[item].Location.X, 0, (float)blueRobots[item].Location.Y)))
                            packet3.robots_blue.Add(new messages_robocup_ssl_detection.SSL_DetectionRobot()
                            {
                                confidence = 1,
                                x = (float)blueRobots[item].Location.X * -1000,
                                y = (float)blueRobots[item].Location.Y * 1000,
                                robot_id = (uint)item,
                                orientation = (float)(Math.PI * (blueRobots[item].Angle.Value + 90) / 180.0)
                            });
                    }
                }
                if (balls != null)
                {
                    foreach (int item in balls.Keys)
                    {
                        if (cam0.IsInCamera(new Vector3((float)balls[item].Location.X, 0, (float)balls[item].Location.Y)))
                            packet0.balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall()
                            {
                                confidence = 1,
                                x = (float)balls[item].Location.X * -1000,
                                y = (float)balls[item].Location.Y * 1000
                            });
                        if (cam1.IsInCamera(new Vector3((float)balls[item].Location.X, 0, (float)balls[item].Location.Y)))
                            packet1.balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall()
                            {
                                confidence = 1,
                                x = (float)balls[item].Location.X * -1000,
                                y = (float)balls[item].Location.Y * 1000
                            });
                        if (cam2.IsInCamera(new Vector3((float)balls[item].Location.X, 0, (float)balls[item].Location.Y)))
                            packet2.balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall()
                            {
                                confidence = 1,
                                x = (float)balls[item].Location.X * -1000,
                                y = (float)balls[item].Location.Y * 1000
                            });
                        if (cam3.IsInCamera(new Vector3((float)balls[item].Location.X, 0, (float)balls[item].Location.Y)))
                            packet3.balls.Add(new messages_robocup_ssl_detection.SSL_DetectionBall()
                            {
                                confidence = 1,
                                x = (float)balls[item].Location.X * -1000,
                                y = (float)balls[item].Location.Y * 1000
                            });
                    }
                }
                return new List<messages_robocup_ssl_detection.SSL_DetectionFrame>() { packet0, packet1, packet2, packet3 };
            }
            return null;
        }
        private float GetAngle(Quaternion q1)
        {
            ///** assumes q1 is a normalised quaternion */
            double test = q1.X * q1.Y + q1.Z * q1.W;
            double heading = 0, attitude = 0, bank = 0;
            if (test > 0.499)
            { // singularity at north pole
                heading = 2 * Math.Atan2(q1.W, q1.W);
                attitude = Math.PI / 2;
                bank = 0;
                return (float)heading;
            } if (test < -0.499)
            { // singularity at south pole
                heading = -2 * Math.Atan2(q1.X, q1.W);
                attitude = -Math.PI / 2;
                bank = 0;
                return (float)heading;
            } double sqx = q1.X * q1.X; double sqy = q1.Y * q1.Y; double sqz = q1.Z * q1.Z;
            heading = Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, 1 - 2 * sqy - 2 * sqz);
            attitude = Math.Asin(2 * test);

            heading *= 180 / Math.PI;
            bank *= 180 / Math.PI;
            attitude *= 180 / Math.PI;

            return (float)heading;
        }


        public void ConnectToAi()
        {
            _networkManager = new CommunicationController("any", _recievePort, _aiName, _sendPort);
            _sendThread = new Thread(new ThreadStart(SendDataRun));
            _reciveThread = new Thread(new ThreadStart(ReciveDataRun));
            _reciveThread.Start();
            _sendThread.Start();
            
        }
        void SendDataRun()
        {
            double timeCapture = 0;
            Camera cam0 = new Camera(field.Width / 4, field.Height / 4, 4, new RectangleF(field.X + 0.5f, field.Y + 0.5f, field.Width / 2 + 1f, field.Height / 2 + 1f), 0),
               cam1 = new Camera(field.Width / 4, -field.Height / 4, 4, new RectangleF(field.X  + 0.5f, 0.5f, field.Width / 2 + 1f, field.Height / 2 + 1f), 1),
               cam2 = new Camera(-field.Width / 4, field.Height / 4, 4, new RectangleF(0.5f, field.Y + 0.5f, field.Width / 2 + 1f, field.Height / 2 + 1f), 2),
               cam3 = new Camera(-field.Width / 4, -field.Height / 4, 4, new RectangleF(0.5f, 0.5f, field.Width / 2 + 1f, field.Height / 2 + 1f), 3);
            Stopwatch sw = new Stopwatch();
        //    HiPerfTimer t2 = new HiPerfTimer();

            long elapsed = 0;
          
            while (true)
            {
                elapsed = 0;
          
                sw.Restart();
                List<messages_robocup_ssl_detection.SSL_DetectionFrame> lists = GenerateCameraModel(cam0, cam1, cam2, cam3);
                messages_robocup_ssl_wrapper.SSL_WrapperPacket wrapper = new messages_robocup_ssl_wrapper.SSL_WrapperPacket();
                wrapper.detection = new messages_robocup_ssl_detection.SSL_DetectionFrame();

                wrapper.detection = lists[0];
                wrapper.detection.t_capture = timeCapture;
                wrapper.detection.camera_id = 0;

               
                MemoryStream stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream, wrapper);

                _networkManager.SendData(stream);
                
                elapsed = sw.ElapsedMilliseconds;
                wrapper.detection = lists[1];
                wrapper.detection.t_capture = timeCapture + elapsed;
                wrapper.detection.camera_id = 1;

                stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream, wrapper);
               
                _networkManager.SendData(stream);

                elapsed = sw.ElapsedMilliseconds;
                wrapper.detection = lists[2];
                wrapper.detection.t_capture = timeCapture + elapsed;
                wrapper.detection.camera_id = 2;

                stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream, wrapper);
                
                _networkManager.SendData(stream);

                elapsed = sw.ElapsedMilliseconds;
                wrapper.detection = lists[3];
                wrapper.detection.t_capture = timeCapture + elapsed;
                wrapper.detection.camera_id = 3;

                stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize<messages_robocup_ssl_wrapper.SSL_WrapperPacket>(stream, wrapper);
               
                _networkManager.SendData(stream);

                while ((elapsed = sw.ElapsedMilliseconds) < 16) ;
                
                timeCapture += elapsed;

                if (elapsed >20)
                {
                    
                }
            }
        }

        void ReciveDataRun()
        {
            GoogleSerializer deserilizer = new GoogleSerializer();
            while (true)
            {
                MemoryStream reciveStream = _networkManager.RecieveData();
                SimulatorParameters simParams = new SimulatorParameters();
                deserilizer.stream = reciveStream;
                simParams = deserilizer.DeserilializeSimParameters();
                if (simParams != null)
                {
                    ApplyForces(simParams);
                }

            }
        }

        Dictionary<int, bool> blueRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> blueUserMode = new Dictionary<int, bool>();

        Dictionary<int, bool> yellowRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> yellowUserMode = new Dictionary<int, bool>();

        Dictionary<int, bool> ballRunning = new Dictionary<int, bool>();
        Dictionary<int, bool> ballUserMode = new Dictionary<int, bool>();

        public void addRobot(int id, Color color, Position2D position, double angle)
        {
            if (color == Color.Blue)
            {
                if (blueRobots == null)
                    blueRobots = new Dictionary<int, SingleObjectState>();
                if (blueRunning == null)
                    blueRunning = new Dictionary<int, bool>();
                if (blueUserMode == null)
                    blueUserMode = new Dictionary<int, bool>();
                if (!blueRobots.ContainsKey(id))
                {
                    blueRunning.Add(id, false);
                    blueRobots.Add(id, new SingleObjectState(ObjectType.OurRobot, position, Vector2D.Zero, Vector2D.Zero, (float)angle, null));
                    blueUserMode.Add(id, false);
                }
            }
            else
            {
                if (yellowRobots == null)
                    yellowRobots = new Dictionary<int, SingleObjectState>();
                if (yellowRunning == null)
                    yellowRunning = new Dictionary<int, bool>();
                if (yellowUserMode == null)
                    yellowUserMode = new Dictionary<int, bool>();
                if (!yellowRobots.ContainsKey(id))
                {
                    yellowRunning.Add(id, false);
                    yellowRobots.Add(id, new SingleObjectState(ObjectType.OurRobot, position, Vector2D.Zero, Vector2D.Zero, (float)angle, null));
                    yellowUserMode.Add(id, false);
                }
            }
        }

        public void addBall(int id, Position2D position)
        {
            if (balls == null)
                balls = new Dictionary<int, SingleObjectState>();
            if (ballRunning == null)
                ballRunning = new Dictionary<int, bool>();
            if (ballUserMode == null)
                ballUserMode = new Dictionary<int, bool>();

            if (!balls.ContainsKey(id))
            {
                ballUserMode.Add(id, false);
                ballRunning.Add(id, false);
                balls.Add(id, new SingleObjectState(ObjectType.Ball, position, Vector2D.Zero, Vector2D.Zero, null, null));
            }
        }

        public void setBallPosition(int id, Position2D position)
        {
            if (balls != null && balls.ContainsKey(id))
            {
                while (ballRunning[id])
                {
                    ballUserMode[id] = true;
                }
                ballUserMode[id] = true;
                balls[id].Location = position;
                ballUserMode[id] = false;
            }
        }

        public void setBallSpeed(int id, Vector2D speed)
        {
            if (balls != null && balls.ContainsKey(id))
            {
                while (ballRunning[id])
                {
                    ballUserMode[id] = true;
                }
                ballUserMode[id] = true;
                balls[id].Speed = speed;
                ballUserMode[id] = false;
            }
        }


        public void removeRobot(int id, Color color)
        {
            if (color == Color.Blue)
            {
                if (blueRobots != null && blueRobots.ContainsKey(id))
                {
                    while (blueRunning[id])
                    {
                        blueUserMode[id] = true;
                    }
                    blueUserMode[id] = true;
                    blueRobots.Remove(id);
                }
            }
            else
            {
                {
                    if (yellowRobots != null && yellowRobots.ContainsKey(id))
                    {
                        while (yellowRunning[id])
                        {
                            yellowUserMode[id] = true;
                        }
                        yellowUserMode[id] = true;
                        yellowRobots.Remove(id);
                    }
                }
            }
        }
        public void setRobotPosition(int id, Color color, Position2D position, float Angle)
        {
            if (color == Color.Blue)
            {
                if (blueRobots != null && blueRobots.ContainsKey(id))
                {
                    while (blueRunning[id])
                    {
                        blueUserMode[id] = true;
                    }
                    blueUserMode[id] = true;
                    blueRobots[id].Location = position;
                    blueRobots[id].Angle = Angle;
                    blueUserMode[id] = false;
                }
            }
            else
            {
                if (yellowRobots != null && yellowRobots.ContainsKey(id))
                {
                    while (yellowRunning[id])
                    {
                        yellowUserMode[id] = true;
                    }
                    yellowUserMode[id] = true;
                    yellowRobots[id].Location = position;
                    yellowRobots[id].Angle = Angle;
                    yellowUserMode[id] = false;
                }
            }
        }
        private void ApplyForces(SimulatorParameters simParams)
        {
            if (blueRobots != null)
                foreach (int item in blueRobots.Keys)
                {
                    var swc = simParams.Commands.Where(c => c.Key == item && c.Value.Color.ToArgb() == Color.Blue.ToArgb()).Select(c => c.Value).FirstOrDefault();
                    if (swc != null && !blueUserMode[item])
                    {

                        Vector2D globalSpeed = new Vector2D();
                        double angle = blueRobots[item].Angle.Value;
                        angle *= (Math.PI / 180.0);
                        globalSpeed.Y = swc.Vy * Math.Cos(angle) + swc.Vx * Math.Sin(angle);
                        globalSpeed.X = swc.Vx * Math.Cos(angle) - swc.Vy * Math.Sin(angle);
                        blueRunning[item] = true;
                        blueRobots[item].Location.X *= -1;
                        blueRobots[item].Location = blueRobots[item].Location + globalSpeed * 0.016;// x = vt + x0
                        swc.W *= (180.0 / Math.PI);
                        blueRobots[item].Angle = blueRobots[item].Angle.Value + (float)(swc.W * 0.016);
                        blueRobots[item].Location.X *= -1;
                        blueRobots[item].Speed = new Vector2D();
                        blueRunning[item] = false;
                    }
                }

            if (yellowRobots != null)
                foreach (int item in yellowRobots.Keys)
                {
                    var swc = simParams.Commands.Where(c => c.Key == item && c.Value.Color.ToArgb() == Color.Yellow.ToArgb()).Select(c => c.Value).FirstOrDefault();
                    if (swc != null && !yellowUserMode[item])
                    {
                        Vector2D globalSpeed = new Vector2D();
                        double angle = yellowRobots[item].Angle.Value;
                        angle *= (Math.PI / 180.0);
                        globalSpeed.Y = swc.Vy * Math.Cos(angle) + swc.Vx * Math.Sin(angle);
                        globalSpeed.X = swc.Vx * Math.Cos(angle) - swc.Vy * Math.Sin(angle);
                        yellowRunning[item] = true;
                        yellowRobots[item].Location.X *= -1;
                        swc.W *= (180.0 / Math.PI);
                        yellowRobots[item].Location = yellowRobots[item].Location + globalSpeed * 0.016;// x = vt + x0
                        yellowRobots[item].Angle = yellowRobots[item].Angle.Value + (float)(swc.W * 0.016);
                        yellowRobots[item].Location.X *= -1;
                        yellowRobots[item].Speed = new Vector2D();
                        yellowRunning[item] = false;
                    }
                }
        }


        private void InitField()
        {

        }

        public void Dispose()
        {
            _sendThread.Abort();
            _reciveThread.Abort();
            _networkManager.Dispose();
            //   device.Dispose();
        }



        public float mass { get; set; }

        public ActorDescription actorDes { get; set; }

        public SlimDX.Direct3D10.Device device { get; set; }


    }

}
