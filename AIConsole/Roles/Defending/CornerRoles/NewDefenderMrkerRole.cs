﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class NewDefenderMrkerRole : RoleBase, IMarkerDefender
    {
        static Position2D inthelastmoment = new Position2D();
        private static Position2D target = new Position2D();
        private Position2D InitialPos = new Position2D();
        private Position2D predictPos = new Position2D();
        static List<double> times = new List<double>();
        private bool initialPositioningFg = true;
        Position2D intersectG = new Position2D();
        private operation opr = operation.noth;
        Position2D markPos = new Position2D();
        HiPerfTimer time = new HiPerfTimer();
        Vector2D markspeed = new Vector2D();
        private bool firsttimedanger = true;
        private bool firstTimeAngle = false;
        private float staticAnglesize = 0;
        private double maxDistance = 5;
        private double markDistance = .18;
        private bool noIntersect = false;
        private bool staticAngle = true;
        private bool onlyOneTime = true;
        private bool dangerzone = true;
        private bool firstTime = true;
        private bool oceaTime = true;
        private bool oldcost = false;
        static int markID = 1000;
        bool isInZone = false;
        double angle = 0.00;
        double a = 3.50;
        bool testrole = false;
        private static Position2D FarTarget = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public void mark(GameStrategyEngine engine, WorldModel model, int RobotID, int? MarkID)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = model.BallState;
                ballStateFast = model.BallStateFast;
            }
            if (FreekickDefence.OppToMark1.HasValue && model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
            {
                markID = FreekickDefence.OppToMark1.Value;
                markspeed = model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                markPos = model.Opponents[FreekickDefence.OppToMark1.Value].Location;
                double Moveangle = 0.00;
                bool CutBall = false;

                Position2D target = CostFunction(model, RobotID, out Moveangle, out CutBall, true);

                Planner.Add(RobotID, target, Moveangle, PathType.UnSafe, false, true, true, false);
                DataBridge.MarkerRole1ID = RobotID;
                DataBridge.CornerRole1target = target;
                DrawingObjects.AddObject(new Circle(target, 0.10, new Pen(Brushes.HotPink, 0.01f)));
            }
            else
            {
                Planner.Add(RobotID, model.OurRobots[RobotID].Location, (model.BallState.Location - model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, true, true, true);
            }
        }

        public Position2D CostFunction(WorldModel model, int RobotID, out double retangle, out bool CutBall, bool forTarget)
        {
            Position2D lastTarget = new Position2D();
            CutBall = false;
            if (FreekickDefence.OppToMark1.HasValue && model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
            {
                markID = FreekickDefence.OppToMark1.Value;
                markspeed = model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                markPos = model.Opponents[FreekickDefence.OppToMark1.Value].Location;

                angle = 0.00;
                Position2D markRobot = model.Opponents[markID].Location;
                Circle min = new Circle();
                retangle = 0.00;
                Circle max = new Circle();
                Predict(model, RobotID, markID, out min, out max, true);
                Position2D target = min.Center;
                if (forTarget)
                {
                    FarTarget = target + (GameParameters.OurGoalCenter - target).GetNormalizeToCopy(.018);
                }

                if (opr == operation.DecreaseAcc)
                {
                    a -= 0.10;
                    opr = operation.noth;
                }
                else if (opr == operation.increaseAcc)
                {
                    a += 0.10;
                    opr = operation.noth;
                }

                Position2D marknearmin = new Position2D();
                if (CurrentState != (int)MarkState.Stop)
                {
                    firstTime = true;
                    staticAngle = true;
                }

                if (CurrentState == (int)MarkState.Cut)
                {
                    Circle CenterOnOppRobot = new Circle(model.Opponents[markID].Location, .3 + .11);
                    Line ballspeedLine = new Line(model.BallState.Location, model.BallState.Location + model.BallState.Speed.GetNormalizeToCopy(15));
                    Obstacles obs = new Obstacles(model);
                    obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                    bool meet = obs.Meet(model.BallState, model.Opponents[markID], .04);
                    List<Position2D> inters = CenterOnOppRobot.Intersect(ballspeedLine);
                    Position2D cutPos = new Position2D();
                    if (inters.Count > 0)
                    {
                        cutPos = inters.OrderBy(u => u.DistanceFrom(model.BallState.Location)).FirstOrDefault();
                    }
                    else
                    {
                        cutPos = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(.4);
                    }
                    lastTarget = cutPos;
                    retangle = -(180 - Math.Abs(model.BallState.Speed.AngleInDegrees));

                }
                #region Stop
                else if (CurrentState == (int)MarkState.Stop)
                {
                    firsttimedanger = true;
                    if (forTarget)
                        DrawingObjects.AddObject(new StringDraw("STOP", model.OurRobots[RobotID].Location.Extend(0.50, 0)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    if (firstTime)
                    {
                        firstTime = false;
                        inthelastmoment = model.OurRobots[RobotID].Location;
                    }
                    dangerzone = true;
                    target = inthelastmoment;
                    retangle = angle;
                    lastTarget = inthelastmoment;
                }
                #endregion
                #region InTheWay
                else if (CurrentState == (int)MarkState.IntheWay)
                {
                    firsttimedanger = true;
                    if (forTarget)
                        DrawingObjects.AddObject(new StringDraw("IN THE WAY", model.OurRobots[RobotID].Location.Extend(0.50, 0)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    noIntersect = false;
                    Line ourGoalCenterOppRobot = new Line(model.Opponents[markID].Location, model.Opponents[markID].Location + model.Opponents[markID].Speed);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    if (intersect.HasValue)
                    {
                        if (firstTimeAngle)
                        {
                            angle = model.OurRobots[RobotID].Angle.Value;
                        }
                        double x, y;
                        if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                            intersect = GameParameters.OurGoalCenter + (model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(model.Opponents[markID], 0.20));

                        intersectG = intersect.Value;
                        int counter = 0;
                        intersectG = intersect.Value;
                        bool ourconflict = true;
                        bool oppconflict = true;
                        while (!ourconflict && !oppconflict && counter < 15)
                        {
                            counter++;
                            intersectG = GameParameters.OurGoalCenter + (intersectG - GameParameters.OurGoalCenter).GetNormalizeToCopy((intersectG - GameParameters.OurGoalCenter).Size + 0.05);
                            Position2D? ourNearRobot = model.OurRobots.Where(u => u.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                            Position2D? oppNearRobot = model.Opponents.Where(t => t.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                            if (ourNearRobot != null || !((intersectG - GameParameters.OurGoalCenter).Size < (model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter).Size - 0.20))
                            {
                                ourconflict = true;
                            }
                            else
                            {
                                ourconflict = false;
                            }
                            if (oppNearRobot != null || !((intersectG - GameParameters.OurGoalCenter).Size < (model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter).Size - 0.20))
                            {
                                oppconflict = true;
                            }
                            else
                            {
                                oppconflict = false;
                            }
                        }
                        dangerzone = true;
                        target = intersectG;


                        //Planner.Add(RobotID, intersectG, angle, PathType.UnSafe, false, avoidness, true, true);
                    }
                    else
                    {
                        dangerzone = true;
                        noIntersect = true; // You Are Near Why go to Prependicular, Don't Fear go to Near Marking State 
                    }
                    retangle = angle;
                    lastTarget = intersectG;
                    //return intersectG;
                }
                #endregion
                #region FarFront
                else if (CurrentState == (int)MarkState.FarFront)
                {
                    if (forTarget)
                        DrawingObjects.AddObject(new StringDraw("FarFront", model.OurRobots[RobotID].Location.Extend(0.50, 0)), "376376537798");
                    Line ourGoalCenterOppRobot = new Line(model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    if (intersect.HasValue)
                    {
                        double dist1 = 0;
                        double dist2 = 0;
                        if (intersect.Value.DistanceFrom(GameParameters.OurGoalCenter) + 0.25 < model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) && intersect.Value.X < GameParameters.OurGoalCenter.X - .1 && GameParameters.IsInDangerousZone(intersect.Value, false, .2, out dist1, out dist2))// Hey man go to perpendicular point for opponent goes to Blindfold to Goal
                        {
                            noIntersect = false;

                            if (firstTimeAngle)
                            {
                                angle = model.OurRobots[RobotID].Angle.Value;
                            }
                            double x, y;
                            if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                                intersect = GameParameters.OurGoalCenter + (model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(model.Opponents[markID], 0.20));
                            intersectG = intersect.Value;

                            if ((markspeed.GetNormnalizedCopy().InnerProduct((perpendicular.Tail - perpendicular.Head).GetNormnalizedCopy()) > 0.80 && markspeed.Size > 1.00))
                            {
                                Line robotSpeed = new Line(model.OurRobots[RobotID].Location, model.OurRobots[RobotID].Location + model.BallState.Speed);
                                Position2D? speedIntersect = robotSpeed.IntersectWithLine(perpendicular);
                                if (intersect.HasValue)
                                {
                                    intersectG = speedIntersect.Value;
                                }
                                double x1 = 0;
                                double y1 = 0;
                                if (GameParameters.IsInDangerousZone(intersectG, false, FreekickDefence.AdditionalSafeRadi, out x1, out y1))
                                {
                                    PointOutOfdangerZone(model, RobotID, markID, intersectG, out intersectG);
                                }
                            }
                            Line speed = new Line(model.OurRobots[RobotID].Location, model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed.GetNormalizeToCopy(10.00));
                            Line oppRobot = new Line(model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                            Position2D? Intersect = speed.IntersectWithLine(oppRobot);
                            Position2D near = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markDistance);

                        }
                        else
                        {
                            dangerzone = true;
                            noIntersect = true; // You Are Near Why go to Prependicular, Don't Fear go to Near fucking Marking State 
                            intersectG = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markDistance);
                        }
                    }
                    else
                    {
                        intersectG = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markDistance);
                        noIntersect = true; // Are you Crazy??? two line don't have any intersect ???!! special state :)
                    }
                    retangle = angle;
                    lastTarget = intersectG;
                    if (lastTarget == new Position2D())
                    {
                        lastTarget = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markDistance);
                    }
                    if (model.Opponents[markID].Location.X < 1.5 && testrole)
                    {
                        Line l1 = new Line(GameParameters.OurGoalCenter, model.Opponents[markID].Location);
                        DrawingObjects.AddObject(l1, "as32d4s3a2d321sa432d");
                        Line l2 = new Line(new Position2D(1.5, -3), new Position2D(1.5, 3));
                        DrawingObjects.AddObject(l2, "asfdsaf564ds6f4awqwde");
                        Position2D? myIntersect = l1.IntersectWithLine(l2);
                        if (myIntersect.HasValue)
                        {
                            DrawingObjects.AddObject(new Circle(myIntersect.Value, 0.09), "sr5t3yg45rawtaetraetsre");
                            lastTarget = myIntersect.Value;
                        }
                    }
                    //return target;
                }
                #endregion
                #region goBack
                else if (CurrentState == (int)MarkState.goback)
                {
                    if (forTarget)
                        DrawingObjects.AddObject(new StringDraw("goback", model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    Line ourGoalCenterOppRobot = new Line(model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    Position2D extendedtarget = new Position2D();
                    if (intersect.HasValue)
                    {
                        noIntersect = false;

                        if (firstTimeAngle)
                        {
                            angle = model.OurRobots[RobotID].Angle.Value;
                        }
                        double x, y;
                        if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                            intersect = GameParameters.OurGoalCenter + (model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(model.Opponents[markID], 0.20));
                        intersectG = intersect.Value;

                        if (firsttimedanger)
                        {
                            firsttimedanger = false;
                            dangerzone = PointOutOfdangerZone(model, RobotID, markID, intersectG, out extendedtarget);
                        }
                        PointOutOfdangerZone(model, RobotID, markID, intersectG, out extendedtarget);
                    }
                    else
                    {
                        noIntersect = true; // Are you Crazy??? two line don't have any intersect ???!! special state :)
                    }
                    target = extendedtarget;
                    retangle = angle;
                    lastTarget = extendedtarget;

                    if (model.Opponents[markID].Location.X < 1.5 && testrole)
                    {
                        Line l1 = new Line(GameParameters.OurGoalCenter, model.Opponents[markID].Location);
                        DrawingObjects.AddObject(l1, "as32d4s3a2d321sa432d");
                        Line l2 = new Line(new Position2D(1.5, -3), new Position2D(1.5, 3));
                        DrawingObjects.AddObject(l2, "asfdsaf564ds6f4awqwde");
                        Position2D? myIntersect = l1.IntersectWithLine(l2);
                        if (myIntersect.HasValue)
                        {
                            DrawingObjects.AddObject(new Circle(myIntersect.Value, 0.09), "sr5t3yg45rawtaetraetsre");
                            lastTarget = myIntersect.Value;
                        }
                    }
                }
                #endregion
                #region NearFront
                else if (CurrentState == (int)MarkState.NearFront) // Sir you can go to near of the Opponent Robot and mark it with pressure
                {
                    double markdistance = markDistance;
                    Position2D checkpoint = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markdistance);

                    dangerzone = true;
                    if (forTarget)
                        DrawingObjects.AddObject(new StringDraw("NEarFront", model.OurRobots[RobotID].Location.Extend(.5, 0)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    firstTimeAngle = true;
                    if (model.Opponents[markID].Speed.Size > 1.00) // Don't change your angle beacause you are boozy and slow when move and change angle together
                    {
                        predictPos = min.Center;
                        onlyOneTime = true;
                    }
                    if (model.Opponents[markID].Speed.Size < 0.10 && onlyOneTime) // hallloooo your predict have very overshoooootttttttttttt
                    {
                        marknearmin = model.Opponents[markID].Location;
                        onlyOneTime = false;
                        if (predictPos.DistanceFrom(model.Opponents[markID].Location) > 0.30) // .3 is hi :) enougth for you
                        {
                            opr = operation.DecreaseAcc;
                        }
                    }
                    if (model.Opponents[markID].Speed.Size > 1.00)// I think Im ..... Robot I Pursuit it and .... it in this state
                    {
                        firstTime = true;
                        if (staticAngle)
                        {
                            staticAngle = false;
                            staticAnglesize = model.OurRobots[RobotID].Angle.Value;
                            angle = staticAnglesize;
                        }
                        double t = 0.00;
                        Position2D sPP = StopPossiblePoint(model, markID, out t);
                        marknearmin = min.Center + (GameParameters.OurGoalCenter - min.Center).GetNormalizeToCopy(markDistance);
                    }
                    else // Slow move and .... Opponent Robot 
                    {
                        staticAngle = true;
                        angle = (model.BallState.Location - model.OurRobots[RobotID].Location).AngleInDegrees;
                        marknearmin = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(markDistance);
                    }
                    target = marknearmin;
                    if (marknearmin.DistanceFrom(GameParameters.OurGoalCenter) > maxDistance)
                    {
                        marknearmin = GameParameters.OurGoalCenter + (marknearmin - GameParameters.OurGoalCenter).GetNormalizeToCopy(maxDistance);
                    }
                    retangle = angle;
                    retangle = angle;
                    lastTarget = marknearmin;
                    double x = 0;
                    double y = 0;
                    if (GameParameters.IsInDangerousZone(lastTarget, false, 0.1, out x, out y))
                    {
                        lastTarget = GameParameters.OurGoalCenter + (model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(model.Opponents[markID], FreekickDefence.AdditionalSafeRadi));
                    }


                    if (model.Opponents[markID].Location.X < 1.5 && testrole)
                    {
                        Line l1 = new Line(GameParameters.OurGoalCenter, model.Opponents[markID].Location);
                        DrawingObjects.AddObject(l1, "as32d4s3a2d321sa432d");
                        Line l2 = new Line(new Position2D(1.5, -3), new Position2D(1.5, 3));
                        DrawingObjects.AddObject(l2, "asfdsaf564ds6f4awqwde");
                        Position2D? intersect = l1.IntersectWithLine(l2);
                        if (intersect.HasValue)
                        {
                            DrawingObjects.AddObject(new Circle(intersect.Value, 0.09), "sr5t3yg45rawtaetraetsre");
                            lastTarget = intersect.Value;
                        }
                    }
                }
                #endregion
                #region Far Behind
                else if (CurrentState == (int)MarkState.FarBehind) // Stupid Idiot, opponent is in your behind, Skidoo! to mark it stupid
                {
                    retangle = 0.00;
                    lastTarget = new Position2D();
                    DrawingObjects.AddObject(new StringDraw("FarBehind", model.OurRobots[RobotID].Location.Extend(0.50, 0)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                }
                #endregion
                #region Near Behind
                else if (CurrentState == (int)MarkState.NearBehind) // Rotate and look back for understanding you are crazy and usless , stupid , .... , ...... , .....
                {
                    retangle = 0.00;
                    lastTarget = new Position2D();
                    DrawingObjects.AddObject(new StringDraw("NearBehind", model.OurRobots[RobotID].Location.Extend(0.50, 0)), model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                }
                #endregion
                #region Correctness
                if (lastTarget.DistanceFrom(GameParameters.OurGoalCenter) > maxDistance)
                {
                    lastTarget = GameParameters.OurGoalCenter + (lastTarget - GameParameters.OurGoalCenter).GetNormalizeToCopy(maxDistance);
                }

                Line directPath = new Line(model.OurRobots[RobotID].Location, lastTarget);
                Circle oppCenter = new Circle(model.Opponents[markID].Location, 0.20);
                List<Position2D> Positions = oppCenter.Intersect(directPath);
                bool correctness = true;
                if (Positions.Count > 0)
                {
                    if (Positions.Count == 1)
                    {
                        if (Positions[0].DistanceFrom(model.OurRobots[RobotID].Location) < model.OurRobots[RobotID].Location.DistanceFrom(lastTarget))
                        {
                            if (lastTarget.DistanceFrom(model.Opponents[markID].Location) < 0.185)
                                correctness = false;
                        }
                    }
                    else if (Positions.Count == 2)
                    {
                        if (Positions[0].DistanceFrom(model.OurRobots[RobotID].Location) < model.OurRobots[RobotID].Location.DistanceFrom(lastTarget) || Positions[1].DistanceFrom(model.OurRobots[RobotID].Location) < model.OurRobots[RobotID].Location.DistanceFrom(lastTarget))
                        {
                            if (target.DistanceFrom(model.Opponents[markID].Location) < 0.185)
                                correctness = false;
                        }
                    }
                }

                if (correctness == false && model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) + .10 < model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                {
                    lastTarget = model.Opponents[markID].Location + (GameParameters.OurGoalCenter - model.Opponents[markID].Location).GetNormalizeToCopy(0.26);
                }
                Position2D lsttgt = lastTarget;
                #endregion
                lastTarget = CommonDefenceUtils.CheckForStopZone(FreekickDefence.BallIsMoved, lastTarget, model);
                if (!FreekickDefence.BallIsMoved)
                {
                    double stop1 = Vector2D.AngleBetweenInDegrees(GameParameters.OurGoalCenter - model.BallState.Location, model.OurRobots[RobotID].Location - model.BallState.Location);
                    double radius = (Math.Abs(stop1) < 10) ? .85 : .8;

                    Position2D currentpos = model.OurRobots[RobotID].Location;
                    Position2D targetPos = lastTarget;
                    Circle stopCircle = new Circle(model.BallState.Location, radius);
                    Line targteLine = new Line(currentpos, targetPos);
                    List<Position2D> intersects = stopCircle.Intersect(targteLine);
                    List<Position2D> tangents = new List<Position2D>();
                    List<Line> lines = new List<Line>();
                    Line tangent = new Line();
                    if (Math.Abs(Vector2D.AngleBetweenInDegrees(lastTarget - model.BallState.Location, model.OurRobots[RobotID].Location - model.BallState.Location)) < 30 && lastTarget.DistanceFrom(model.OurRobots[RobotID].Location) < .35)
                    {
                        return lastTarget;
                    }
                    if (intersects.Count > 0 && model.OurRobots[RobotID].Location.DistanceFrom(lastTarget) > .3 && GameParameters.IsInField(model.BallState.Location, -.7))
                    {

                        Position2D? nearpos = intersects.OrderBy(y => y.DistanceFrom(model.OurRobots[RobotID].Location)).FirstOrDefault();
                        if (nearpos.HasValue)
                        {
                            if (nearpos.Value.DistanceFrom(model.OurRobots[RobotID].Location) < lastTarget.DistanceFrom(model.OurRobots[RobotID].Location))
                            {
                                int s = stopCircle.GetTangent(nearpos.Value, out lines, out tangents);
                                Position2D? neartangent = tangents.OrderBy(y => y.DistanceFrom(model.OurRobots[RobotID].Location)).FirstOrDefault();
                                if (neartangent.HasValue)
                                {
                                    lastTarget = neartangent.Value;
                                }
                                if (neartangent.HasValue && model.OurRobots[RobotID].Location.DistanceFrom(model.BallState.Location) < stopCircle.Radious + .05)
                                {
                                    if (lines.Count > 0)
                                    {
                                        tangent = lines.OrderBy(t => t.Angle).FirstOrDefault();

                                        Vector2D targetVector = Vector2D.FromAngleSize((model.BallState.Location - model.OurRobots[RobotID].Location).AngleInRadians + (Math.PI / 2), .5);
                                        lastTarget = neartangent.Value + targetVector;
                                    }

                                }

                            }
                        }
                    }

                }
                return lastTarget;
            }
            else
            {
                retangle = 0;
                return model.OurRobots[RobotID].Location;
            }
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
            {
                markID = FreekickDefence.OppToMark1.Value;
                markspeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;
                Line ourGoalCenterOppRobot = new Line(markPos, markPos + markspeed);
                Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(markPos);
                Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                if (intersect.HasValue)
                {
                    if (firstTimeAngle)
                    {
                        angle = Model.OurRobots[RobotID].Angle.Value;
                    }
                    double x, y;
                    if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                        intersect = GameParameters.OurGoalCenter + (markPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(markPos, markspeed, 0.0f), 0.20));
                    intersectG = intersect.Value;
                    int counter = 0;
                    bool ourconflict = true;
                    bool oppconflict = true;
                    while (!ourconflict && !oppconflict && counter < 7)
                    {
                        counter++;
                        intersectG = GameParameters.OurGoalCenter + (intersectG - GameParameters.OurGoalCenter).GetNormalizeToCopy((intersectG - GameParameters.OurGoalCenter).Size + 0.10);
                        Position2D? ourNearRobot = Model.OurRobots.Where(u => u.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                        Position2D? oppNearRobot = Model.Opponents.Where(t => t.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                        if (ourNearRobot != null)
                        {
                            ourconflict = true;
                        }
                        else
                        {
                            ourconflict = false;
                        }
                        if (oppNearRobot != null)
                        {
                            oppconflict = true;
                        }
                        else
                        {
                            oppconflict = false;
                        }
                    }
                }

                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 1, 1, new List<int>() { RobotID }, new List<int> { markID });
                Line ourGoalCenterOppRobot1 = new Line(Model.Opponents[markID].Location, Model.Opponents[markID].Location + Model.Opponents[markID].Speed);
                Line perpendicular1 = ourGoalCenterOppRobot1.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                Position2D? intersect1 = ourGoalCenterOppRobot1.IntersectWithLine(perpendicular1);
                bool cancelInTheWay = false;
                if (intersect1.HasValue)
                {
                    if (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(intersect1.Value, Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi))
                    {
                        cancelInTheWay = true;
                    }
                }

                Circle CenterOnOppRobot = new Circle(Model.Opponents[markID].Location, .3);
                Line ballspeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                Obstacles OObs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                bool meet = obs.Meet(Model.BallState, Model.Opponents[markID], .04);
                List<Position2D> inters = CenterOnOppRobot.Intersect(ballspeedLine);


                bool dd = false;
                if (markID != 1000)
                    dd = PointOutOfdangerZone(Model, RobotID, markID, intersectG, out intersectG);
                if (markPos != Position2D.Zero)
                {


                    Circle oppcircle = new Circle(Model.Opponents[markID].Location, .3 + .1);
                    Circle oppcircle3 = new Circle(Model.Opponents[markID].Location, .3 + .1);
                    Line BallSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                    Obstacles Obs = new Obstacles(Model);
                    Obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                    bool met = Obs.Meet(Model.BallState, Model.Opponents[markID], .04);
                    if (CurrentState == (int)MarkState.Cut)
                    {
                        if (met || oppcircle.Intersect(BallSpeedLine).Count < 1 || ballState.Speed.GetNormnalizedCopy().InnerProduct((Model.OurRobots[RobotID].Location - ballState.Location).GetNormnalizedCopy()) < 0 || ballState.Speed.Size < 0.80 || BallSpeedLine.Distance(Model.OurRobots[RobotID].Location) > .25 + .1)
                        {
                            CurrentState = (int)MarkState.NearFront;
                        }
                    }
                    else
                    {
                        /*if (CurrentState == (int)MarkState.NearFront && !met && oppcircle3.Intersect(BallSpeedLine).Count > 0 && BallSpeedLine.Distance(Model.OurRobots[RobotID].Location) < .25 && ballState.Speed.GetNormnalizedCopy().InnerProduct((Model.OurRobots[RobotID].Location - ballState.Location).GetNormnalizedCopy()) > .8 && ballState.Speed.Size > .5)
                        {
                            CurrentState = (int)MarkState.Cut;
                        }
                        else*/
                        if (((markspeed.GetNormnalizedCopy().InnerProduct((GameParameters.OurGoalCenter - markPos).GetNormnalizedCopy()) > 0.70 && markspeed.Size > 0.30)
                            && (markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > 0.090
                            && Math.Abs(Vector2D.AngleBetweenInDegrees(GameParameters.OurGoalCenter - markPos, Model.OurRobots[RobotID].Location - markPos)) < 20
                            && markPos.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.30))
                            ) // Opponent is very bull*** it goes to my face hehe i stand here forever
                        {
                            CurrentState = (int)MarkState.Stop;
                        }
                        else if ((markspeed.GetNormnalizedCopy().InnerProduct((new Vector2D(1.00, 0.00)).GetNormnalizedCopy()) > 0.60 && markspeed.Size > 1.00) && new Line(markPos + markspeed, markPos).Distance(Model.OurRobots[RobotID].Location) < 0.50 &&
                            (((new Line(markPos + markspeed, markPos).PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location)).IntersectWithLine(new Line(markPos + markspeed, markPos)).Value) - markPos).InnerProduct((markPos + markspeed) - markPos) > 0 &&
                            !cancelInTheWay) //&& (intersectG - GameParameters.OurGoalCenter).Size < (markPos - GameParameters.OurGoalCenter).Size - .2)
                        {
                            CurrentState = (int)MarkState.IntheWay;
                        }
                        //else if (!dangerzone && (CurrentState == (int)MarkState.goback || CurrentState == (int)MarkState.FarFront) && markspeed.Size > 0.50)
                        //{
                        //    CurrentState = (int)MarkState.NearFront;
                        //}
                        else if (!((FarTarget - GameParameters.OurGoalCenter).Size < (markPos - GameParameters.OurGoalCenter).Size) && CurrentState == (int)MarkState.IntheWay)
                        {
                            CurrentState = (int)MarkState.NearFront;
                        }
                        else if (CurrentState == (int)MarkState.goback && markspeed.Size < 0.50)
                        {
                            CurrentState = (int)MarkState.NearFront;
                        }
                        else if (Math.Abs(Vector2D.AngleBetweenInDegrees(markspeed, GameParameters.OurGoalCenter - markPos)) > 100 && markspeed.Size > 0.50 && Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) + .1 < Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                        {
                            CurrentState = (int)MarkState.goback;
                        }
                        else if (CurrentState == (int)MarkState.FarFront && new Line(FarTarget, GameParameters.OurGoalCenter).Distance(Model.OurRobots[RobotID].Location) < 0.6 && FarTarget != Position2D.Zero)
                        {
                            CurrentState = (int)MarkState.NearFront;
                        }
                        else if (new Line(FarTarget, GameParameters.OurGoalCenter).Distance(Model.OurRobots[RobotID].Location) > 0.5 && Model.OurRobots[RobotID].Location.DistanceFrom(markPos) > .5)
                        {
                            CurrentState = (int)MarkState.FarFront;
                        }
                        else if (noIntersect)
                        {
                            CurrentState = (int)MarkState.NearFront;
                        }
                        else if (markPos.DistanceFrom(GameParameters.OurGoalCenter) > Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) || ((GameParameters.OurGoalCenter - markPos).InnerProduct(Model.OurRobots[RobotID].Location - markPos) > 0))
                        {
                            double dist = markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter);
                            if (dist < 0.25)
                            {
                                CurrentState = (int)MarkState.NearFront;
                            }
                            else if (CurrentState != (int)MarkState.NearFront)
                            {
                                CurrentState = (int)MarkState.FarFront;
                            }
                        }
                        else
                        {
                            double dist = Math.Abs(markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter));
                            if (dist < 0.25)
                            {
                                CurrentState = (int)MarkState.NearFront;
                            }
                            else
                            {
                                CurrentState = (int)MarkState.NearFront;
                            }
                        }
                    }
                }
            }
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (!FreekickDefence.switchAllMode)
            {

                if (FreekickDefence.BallIsMoved && FreekickDefence.WeAreInCorner)
                {
                    List<RoleBase> res = new List<RoleBase>() { new DefenderMarkerRole(),
                                                        new DefenderMarkerRole2(),
                                                        new DefendGotoPointRole(),
                                                        new DefenderCornerRole2(),
                                                        new NewDefenderMrkerRole(),
                                                        new NewDefenderMarkerRole2(),
                                                        new DefenderCornerRole4()
                                                         };

                    if (FreekickDefence.SwitchToActiveMarker1)
                    {
                        res.Add(new ActiveRole());
                    }
                    if (FreekickDefence.SwitchDefender2Marker1)
                    {
                        res.Add(new DefenderNormalRole1());
                        res.Add(new DefenderCornerRole1());
                    }
                    if (FreekickDefence.SwitchDefender32Marker1)
                    {
                        res.Add(new DefenderCornerRole3());
                    }

                    //if (FreekickDefence.SwitchDefender42Marker1)
                    //{
                    //    res.Add(new DefenderCornerRole4());
                    //}

                    if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
                    {
                        res.Add(new DefenderNormalRole1());
                        res.Add(new DefenderCornerRole1());
                    }
                    if (FreekickDefence.LastSwitchDefender32Marker1)//New IO2014
                    {
                        res.Add(new DefenderCornerRole3());
                    }
                    //if (FreekickDefence.LastSwitchDefender42Marker1)//New IO2014
                    //{
                    //    res.Add(new DefenderCornerRole4());
                    //}
                    return res;
                }
                else
                {
                    List<RoleBase> res = new List<RoleBase>() { new DefenderMarkerRole(),
                                                        new DefenderMarkerRole2(),
                                                        new DefendGotoPointRole(),
                                                        new DefenderCornerRole2(),
                                                        new NewDefenderMrkerRole(),
                                                        new NewDefenderMarkerRole2(),
                                                        new NewDefenderMarkerRole3(),
                                                        new DefenderCornerRole4()
                                                         };

                    if (FreekickDefence.freeSwitchbetweenRegionalAndMarker || FreekickDefence.RearRegional)
                    {
                        res.Add(new RegionalDefenderRole());
                        res.Add(new RegionalDefenderRole2());
                    }

                    if (FreekickDefence.SwitchToActiveMarker1)
                    {
                        res.Add(new ActiveRole());
                    }
                    if (FreekickDefence.SwitchDefender2Marker1)
                    {
                        res.Add(new DefenderNormalRole1());
                        res.Add(new DefenderCornerRole1());
                    }
                    if (FreekickDefence.SwitchDefender32Marker1)
                    {
                        res.Add(new DefenderCornerRole3());
                    }

                    //if (FreekickDefence.SwitchDefender42Marker1)
                    //{
                    //    res.Add(new DefenderCornerRole4());
                    //}

                    if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
                    {
                        res.Add(new DefenderNormalRole1());
                        res.Add(new DefenderCornerRole1());
                    }
                    if (FreekickDefence.LastSwitchDefender32Marker1)//New IO2014
                    {
                        res.Add(new DefenderCornerRole3());
                    }
                    if (FreekickDefence.DefenderMarkerRole1ToActive)
                    {
                        res.Add(new ActiveRole());
                    }
                    if (!FreekickDefence.BallIsMoved)
                    {
                        res.Add(new StopRole1());
                    }

                    return res;
                }
            }
            else
            {
                List<RoleBase> res = new List<RoleBase>();
                res.Add(new DefenderCornerRole1());
                res.Add(new DefenderCornerRole2());
                res.Add(new DefenderCornerRole3());
                res.Add(new DefenderMarkerRole2());
                res.Add(new DefenderCornerRole4());
                res.Add(new DefenderMarkerRole());
                res.Add(new NewDefenderMrkerRole());
                res.Add(new NewDefenderMarkerRole2());
                res.Add(new RegionalDefenderRole());
                res.Add(new RegionalDefenderRole2());
                if (FreekickDefence.DefenderMarkerRole1ToActive)
                {
                    res.Add(new ActiveRole());
                }
                if (!FreekickDefence.BallIsMoved)
                {
                    res.Add(new StopRole1());
                }
                return res;
            }
        }

        private bool PointOutOfdangerZone(WorldModel model, int RobotID, int MarkID, Position2D targetreference, out Position2D targetvalue)
        {
            targetvalue = targetreference;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.ZoneCircle;
            obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            obs.State = new SingleObjectState(GameParameters.OurGoalCenter, new Vector2D(), null);
            bool meet = true;
            int counter = 0;
            while (meet && counter < 15 && targetreference.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                counter++;
                meet = obs.Meet(model.OurRobots[RobotID], new SingleObjectState(targetreference, Vector2D.Zero, 0.00f), RobotParameters.OurRobotParams.Diameter / 2.00);
                targetvalue = GameParameters.OurGoalCenter + (targetreference - GameParameters.OurGoalCenter).GetNormalizeToCopy((targetvalue - GameParameters.OurGoalCenter).Size + 0.10);
                if (counter > 14 || !(targetvalue.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                {
                    targetvalue = model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - model.Opponents[MarkID].Location).GetNormalizeToCopy(markDistance);
                    return false;

                }
                else if (!meet)
                {
                    return true;
                }
            }
            if (targetvalue.DistanceFrom(GameParameters.OurGoalCenter) - .2 > model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                targetvalue = model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - model.Opponents[MarkID].Location).GetNormalizeToCopy(markDistance);
            }
            targetvalue = targetreference;
            return false;
        }

        public void Predict(WorldModel Model, int RobotID, int markedID, out Circle minimumCircle, out Circle maximumCircle, bool isStopPosblePoint)
        {
            minimumCircle = new Circle();
            maximumCircle = new Circle();
            double BallSpeed = ballState.Speed.Size;
            double BallArriveTimeForRobot1Max = 0.40;
            double BallArriveTimeForRobot1Min = 0.40;
            Position2D FrontRobot1Max = new Position2D();
            Position2D FrontRobot1Min = new Position2D();
            Vector2D FirstRobotSpeedSize = Model.Opponents[markedID].Speed;
            List<Position2D> Robot1PosesMax = new List<Position2D>();
            List<Position2D> Robot1PosesMin = new List<Position2D>();
            if (!isStopPosblePoint)
            {
                for (int i = 0; i < 157; i++)
                {
                    double Convert = (double)i / 25.00;
                    double FrontRobot1XMax = 0.50 * a * Math.Cos(Convert) * (BallArriveTimeForRobot1Max * BallArriveTimeForRobot1Max) + (FirstRobotSpeedSize.Size * Math.Cos(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Max) + Model.Opponents[markedID].Location.X;
                    double FrontRobot1YMax = 0.50 * a * Math.Sin(Convert) * (BallArriveTimeForRobot1Max * BallArriveTimeForRobot1Max) + (FirstRobotSpeedSize.Size * Math.Sin(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Max) + Model.Opponents[markedID].Location.Y;
                    double FrontRobot1XMin = 0.50 * -a * Math.Cos(Convert) * (BallArriveTimeForRobot1Min * BallArriveTimeForRobot1Min) + (FirstRobotSpeedSize.Size * Math.Cos(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Min) + Model.Opponents[markedID].Location.X;
                    double FrontRobot1YMin = 0.50 * -a * Math.Sin(Convert) * (BallArriveTimeForRobot1Min * BallArriveTimeForRobot1Min) + (FirstRobotSpeedSize.Size * Math.Sin(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Min) + Model.Opponents[markedID].Location.Y;
                    FrontRobot1Max = new Position2D(FrontRobot1XMax, FrontRobot1YMax);
                    FrontRobot1Min = new Position2D(FrontRobot1XMin, FrontRobot1YMin);
                    Robot1PosesMax.Add(FrontRobot1Max);
                    Robot1PosesMin.Add(FrontRobot1Min);
                }

                if (Model.OurRobots.ContainsKey(RobotID))
                {
                    List<Position2D> YSortRobot1Max = Robot1PosesMax.OrderBy(g => g.Y).ToList();
                    List<Position2D> YSortRobot1Min = Robot1PosesMin.OrderBy(g => g.Y).ToList();
                    double SmallYRobot1Max = YSortRobot1Max.First().Y;
                    double BigYRobot1Max = YSortRobot1Max.Last().Y;

                    double SmallYRobot1Min = YSortRobot1Min.First().Y;
                    double BigYRobot1Min = YSortRobot1Min.Last().Y;

                    Position2D Robot1CenterCircleMax = new Position2D(YSortRobot1Max.First().X, (SmallYRobot1Max + BigYRobot1Max) / 2.00);
                    Position2D Robot1CenterCircleMin = new Position2D(YSortRobot1Min.First().X, (SmallYRobot1Min + BigYRobot1Min) / 2.00);
                    Circle Robot1CircleMax = new Circle();
                    Circle Robot1CircleMin = new Circle();
                    Robot1CircleMax = new Circle(Robot1CenterCircleMax, Math.Max(Robot1PosesMax.OrderBy(u => u.DistanceFrom(Robot1CenterCircleMax)).First().DistanceFrom(Robot1CenterCircleMax), 0.13) + 0.02);
                    Robot1CircleMin = new Circle(Robot1CenterCircleMin, Math.Max(Robot1PosesMin.OrderBy(u => u.DistanceFrom(Robot1CenterCircleMin)).First().DistanceFrom(Robot1CenterCircleMin), 0.13) + 0.02);

                    minimumCircle = Robot1CircleMin;
                    maximumCircle = Robot1CircleMax;

                }


            }
            double x = 0;
            minimumCircle = new Circle(StopPossiblePoint(Model, markedID, out x), .3);

        }

        public Position2D StopPossiblePoint(WorldModel model, int MarkID, out double t)
        {
            double v = model.Opponents[MarkID].Speed.Size;
            double displacement = (v * v) / (2.00 * a);
            Position2D target = model.Opponents[MarkID].Location + model.Opponents[MarkID].Speed.GetNormalizeToCopy(displacement);

            t = displacement / (v / 2.00);
            //if (target == new Position2D())
            //{
            //    double s = 0;
            //}
            return target;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            markID = FreekickDefence.OppToMark1.Value;
            markspeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
            markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;
            if (oldcost)
            {
                double ang = 0;
                bool avd = false;
                if (CostFunction(Model, RobotID, out ang, out avd, false) == new Position2D())
                {
                    int breake = 0;
                }
                double cost = Model.OurRobots[RobotID].Location.DistanceFrom(CostFunction(Model, RobotID, out ang, out avd, false));

                return cost * cost;
            }
            else
            {
                double cost = Model.OurRobots[RobotID].Location.DistanceFrom(CalculateTarget(engine, Model, RobotID, OldMarkerPos(engine, Model, markID), markID));
                return cost * cost;
            }

        }

        private Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, int oppid)
        {

            Position2D pos = new Position2D();
            SingleObjectState oppState = (Model.Opponents.ContainsKey(oppid)) ? Model.Opponents[oppid] : ballState;
            Position2D postosee = new Position2D(GameParameters.OppGoalCenter.X, -1 * Math.Sign(ballState.Location.Y) * 2.00);
            Circle OpprobotCenter = new Circle(Model.Opponents[oppid].Location, 0.22);
            Vector2D BallVectorSpeed = ballState.Speed.GetNormalizeToCopy(10.00);
            Line BallMotionLine = new Line(ballState.Location, ballState.Location + BallVectorSpeed);
            List<Position2D> Intersections = OpprobotCenter.Intersect(BallMotionLine);
            Position2D first = Intersections.OrderBy(o => o.DistanceFrom(ballState.Location)).FirstOrDefault();
            pos = first.Extend(0.05, 0);
            if (first == Position2D.Zero)
            {
                pos = oppState.Location + (ballState.Location - oppState.Location).GetNormalizeToCopy(OppFreeKickMarkerUtils.DistCutPassFromOpp);
            }
            Position2D targetforkick = (ballState.Location.Y > 0) ? GameParameters.OppLeftCorner : GameParameters.OppRightCorner;
            Position2D stoppos = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians, CommonDefenceUtils.StopZone);
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, null, Model.Opponents.Keys.ToList());
            if (pos.DistanceFrom(stoppos) < RobotParameters.OurRobotParams.Diameter + 0.02 && obs.Meet(new SingleObjectState(stoppos, Vector2D.Zero, 0), 0.10))
            {
                Vector2D vec = pos - stoppos;
                pos = stoppos + vec.GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter + 0.02);
            }

            double Mindist = GameParameters.SafeRadi(new SingleObjectState(pos, Vector2D.Zero, 0), OppFreeKickMarkerUtils.MinDistBehindFromZone);
            bool meet = false;
            double d = pos.DistanceFrom(GameParameters.OurGoalCenter);

            if (!isInZone && d < Mindist)
            {
                isInZone = true;
            }
            else if (isInZone && d > Mindist + 0.10)
            {
                isInZone = false;
            }
            if (isInZone)
            {
                Obstacles obstacles = new Obstacles(Model);

                List<int> exclude = new List<int> { RobotID, Model.GoalieID ?? 100 };
                obstacles.AddObstacle(1, 0, 0, 0, exclude, new List<int>() { oppid });

                meet = obstacles.Meet(Model.OurRobots[RobotID], new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, null), 0.07);
                if (meet)
                {
                    //kpos = (pos - GameParameters.OurGoalCenter).GetNormalizeToCopy(.1) + pos;
                }
            }
            pos = CommonDefenceUtils.CheckForStopZone(FreekickDefence.BallIsMoved, pos, Model);
            return pos;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        private static Position2D OldMarkerPos(GameStrategyEngine engine, WorldModel Model, int oppid)
        {

            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            state = Model.Opponents[oppid];

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);

            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, MarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;

            double MarkFromDist = MarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;

            Position2D maxpos = GameParameters.OurGoalCenter + (Target - GameParameters.OurGoalCenter).GetNormalizeToCopy(3);
            if (GameParameters.OurGoalCenter.DistanceFrom(Target) > GameParameters.OurGoalCenter.DistanceFrom(maxpos))
                Target = maxpos;
            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));
            target = Target;
            return target;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        enum operation
        {
            noth,
            increaseAcc,
            DecreaseAcc
        }

        public enum MarkState
        {
            FarFront,
            NearFront,
            NearBehind,
            FarBehind,
            goback,
            IntheWay,
            Stop,
            Cut
        }
    }
}
