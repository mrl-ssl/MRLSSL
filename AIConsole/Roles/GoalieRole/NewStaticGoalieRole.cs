﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles
{
    class NewStaticGoalieRole : RoleBase
    {
#if NEW
        public Position2D Target = new Position2D();
        bool calculateCost = false;
        Position2D intermediatePos = new Position2D();

        bool gotoperp = false;

        public static Position2D lasttarget = new Position2D();
        public static Position2D lastinitpos = new Position2D();
        private static bool wehaveintersect = false;
        private static int lastRobotID;
        private bool gotointermediatepos = true;
        private bool gotointermediateposBreak = true;
        private bool gotoBreakpos = false;

        private static int kickerOpponent = 0;

        public static Position2D lasttargetPoint = new Position2D();
        private bool firstTime3 = true;

        public static bool BallCutSituationr = false;
        public static Position2D BallCutPosr = new Position2D();
        public static Position2D BallBreakPosr = new Position2D();
        public static int CutBallRobotIDr = 1000;
        public static double balltimer = 0;
        public static double Robottimer = 0;
        public static Position2D InitialDefenderCutr = new Position2D();
        public static Position2D TargetDefenderCutr = new Position2D();
        public static bool getActiver = false;
        public static bool farFlagr = false;

        private Position2D lastrobotpos = new Position2D();
        private Position2D lastintersect = new Position2D();

        private Position2D currentRobot = new Position2D();
        private Position2D lastposRobot = new Position2D();

        private static Position2D initialpos = new Position2D();

        private static Position2D lastPositionInPredive = new Position2D();

        int counterBalInFront = 0;
        private bool firstTtime = true;
        private double counter;
        private double counter1 = 0;
        private double Deccelcounter;
        private bool firstTime2 = true;


        private RBstate LastRB = RBstate.Ball;
        private bool Defender1Delayed = false;
        private bool Defender2Delayed = false;

        Position2D StaticDefenderCurrentPos = new Position2D();
        Position2D StaticDefender2CurrentPos = new Position2D();
        Position2D GoalieCurrentPos = new Position2D();
        Position2D GoalieTargetPos = new Position2D();

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState BallState = new SingleObjectState();

        int? StaticDefender1IDG = null;
        int? StaticDefender2IDG = null;

        double OpengapSize = 95;
        double ClosegapSize = 0.35;

        Queue<double> robotAngle = new Queue<double>(100);
        Queue<double> robotDistance = new Queue<double>(100);
        Queue<Position2D> ballstates = new Queue<Position2D>(100);


        #region new functions
        public void CalculateGoalIntervals(WorldModel Model, out List<VisibleGoalInterval> ourGoalIntervals, out List<VisibleGoalInterval> oppGoalIntervals, bool useOpp, bool useOur, List<int> OppRobotIDToExclude, List<int> OurRobotIDToExclude)
        {
            ourGoalIntervals = GetVisibleGoalIntervals(Model, Model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, useOpp, useOur, OppRobotIDToExclude, OurRobotIDToExclude);
            oppGoalIntervals = GetVisibleGoalIntervals(Model, Model.BallState.Location, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, useOpp, useOur, OppRobotIDToExclude, OurRobotIDToExclude);
        }

        private List<VisibleGoalInterval> GetVisibleGoalIntervals(WorldModel Model, Position2D FromLocation, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, List<int> oppRobotIDsToExclude, List<int> ourRobotIDsToExclude)
        {
            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Position2D goalCenter = Position2D.Interpolate(GoalStart, GoalEnd, 0.5);
            if (Model == null)
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), 1));
            }
            else
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), (float)(GoalEnd - GoalStart).Size * (float)Math.Sin(Math.Abs(Vector2D.AngleBetweenInRadians(GoalEnd - GoalStart, goalCenter - FromLocation)))));
                Vector2D centerDirection = goalCenter - FromLocation;
                if (UseOpponents)
                    if (Model.Opponents != null)
                        foreach (int oppID in Model.Opponents.Keys)
                            if (!oppRobotIDsToExclude.Contains(oppID))
                                if (centerDirection.InnerProduct(Model.Opponents[oppID].Location - FromLocation) > 0)
                                    ExcludeObstacle(intervals, new Circle(Model.Opponents[oppID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));

                if (UseOurRobots)
                {
                    List<int> robotIDsToExcludeList = new List<int>(ourRobotIDsToExclude);
                    foreach (int RobotID in Model.OurRobots.Keys)
                        if (!robotIDsToExcludeList.Contains(RobotID))
                            if (centerDirection.InnerProduct(Model.OurRobots[RobotID].Location - FromLocation) > 0)
                                ExcludeObstacle(intervals, new Circle(Model.OurRobots[RobotID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                }
            }

            return intervals;
        }

        void ExcludeObstacle(List<VisibleGoalInterval> intervals, Circle obstacle, Position2D fromLocation, Vector2D centerDirection, Position2D goalCenter, Line goalLine)
        {
            if (intervals.Count == 0)
                return;
            List<Line> tangentLines;
            List<Position2D> tangentPoints;
            int tangents = obstacle.GetTangent(fromLocation, out tangentLines, out tangentPoints);
            Interval toExclude;
            if (tangents == 2)
                toExclude = new Interval(
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme(fromLocation, tangentPoints[1], goalCenter, tangentLines[1], true, goalLine));
            else if (tangents == 1)
                toExclude = new Interval(
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], false, goalLine)
                    );
            else //tangents == 0
            {
                Line l = new Line(fromLocation, obstacle.Center).PerpenducilarLineToPoint(fromLocation);
                toExclude = new Interval(
                    GetExtreme(fromLocation, fromLocation, goalCenter, l, true, goalLine),
                    GetExtreme(fromLocation, fromLocation, goalCenter, l, false, goalLine)
                    );
            }
            int i = 0;
            while (i < intervals.Count && intervals[i].interval.End <= toExclude.Start)
                i++;
            if (i < intervals.Count)
                if (intervals[i].interval.Start < toExclude.Start)
                {
                    double temp = intervals[i].interval.End;
                    intervals[i] = new VisibleGoalInterval(new Interval(intervals[i].interval.Start, toExclude.Start), intervals[i].ViasibleWidth);
                    i++;
                    if (temp > toExclude.End)
                    {
                        intervals.Insert(i, new VisibleGoalInterval(new Interval((float)toExclude.End, (float)temp), 0));
                        i++;
                    }
                }
            while (i < intervals.Count && intervals[i].interval.End < toExclude.End)
                intervals.RemoveAt(i);
            if (i < intervals.Count && intervals[i].interval.Start < toExclude.End)
                intervals[i] = new VisibleGoalInterval(new Interval(toExclude.End, intervals[i].interval.End), intervals[i].ViasibleWidth);
        }

        float GetExtreme(GPosition2D fromLocation, GPosition2D tangentPoint, GPosition2D goalCenter, GLine l, bool Pos, GLine goalLine)
        {
            GVector2D vect = tangentPoint.Sub(fromLocation);
            if (vect.SquareSize() == 0)
                vect = (Pos ? new GVector2D(l.B, l.A) : new GVector2D(-l.B, -l.A));
            if (sign(vect.X) == sign(goalCenter.X - fromLocation.X))
            {
                bool HasValue;
                GPosition2D pos = goalLine.IntersectWithLine(l, out HasValue);
                if (HasValue)
                    return pos.Y;
            }
            if (vect.Y < 0)
                return -1000;
            else
                return 1000;
        }

        static float sign(float x)
        {
            return ((x == 0) ? 0 : (x / (float)Math.Abs(x)));
        }
        #endregion

        #region old Perform
        bool spin = true;
        public SingleWirelessCommand perform(GameStrategyEngine engine, WorldModel model, int robotID, SingleObjectState defenceSate, int? StaticDefender1ID, int? StaticDefender2ID)
        {
            StaticDefender1IDG = StaticDefender1ID;
            StaticDefender2IDG = StaticDefender2ID;
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                BallState = DefenceTest.currentBallState;
            }
            else
            {
                ballState = model.BallState;
                BallState = model.BallState;
            }
            if (StaticDefender1ID.HasValue)
                StaticDefenderCurrentPos = model.OurRobots[StaticDefender1ID.Value].Location;
            if (StaticDefender2ID.HasValue)
                StaticDefender2CurrentPos = model.OurRobots[StaticDefender2ID.Value].Location;
            int? id = StaticRB(engine, model);
            DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

            defenceSate = (id.HasValue) ? model.Opponents[id.Value] : ballState;
            SingleWirelessCommand SWc = new SingleWirelessCommand();
            #region Normal
            if (CurrentState == (int)GoalieStates.Normal)
            {

                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-0.4);
                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                postoGo = new Position2D(x, postoGo.Y);
                SWc = GetSkill<GotoPointSkill>().GotoPoint(model, robotID, postoGo, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, false, false, 3, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
                Planner.Add(robotID, s, false);

            }
            #endregion
            #region InPenaltyArea
            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                Vector2D ballSpeed = BallState.Speed;
                double g = Vector2D.AngleBetweenInRadians(ballSpeed, (model.OurRobots[robotID].Location - BallState.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(g) * Math.Cos(g))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(g)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = 0.1;
                Position2D IntersectPoint = new Position2D();
                double distToBall = ballState.Location.DistanceFrom(model.OurRobots[robotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);
                Line ballspeed = new Line(model.BallState.Location, model.BallState.Location + model.BallState.Speed.GetNormalizeToCopy(15));
                Line goalline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                Position2D? intersect = ballspeed.IntersectWithLine(goalline);
                bool skip = false;
                bool goActive = false;
                bool Gointersect = false;
                bool togoal = true;
                Position2D target = new Position2D();

                if (intersect.HasValue)
                {
                    Position2D intersects = intersect.Value;
                    if (((intersects.Y > GameParameters.OurGoalLeft.Y + .15 && intersects.Y < 1.15) || (intersects.Y < GameParameters.OurGoalLeft.Y - .15 && intersects.Y > -1.15)) && model.BallState.Speed.Size > .3 && model.BallState.Speed.InnerProduct(GameParameters.OurGoalCenter - model.BallState.Location) > 0)
                    {
                        skip = true;
                    }
                    else
                    {
                        skip = false;
                    }
                    if (((intersects.Y < GameParameters.OurGoalLeft.Y + .15 && intersects.Y > 0) || (intersects.Y > GameParameters.OurGoalLeft.Y - .15 && intersects.Y < 0)))
                    {
                        togoal = true;
                    }
                    else
                    {
                        togoal = false;
                    }
                }
                else
                {
                    skip = false;
                }

                if ((acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                {
                    goActive = true;
                }
                else
                {
                    goActive = false;
                }
                if (acceptable2 * 1.2 < ballSpeed.Size)
                {
                    Gointersect = true;
                }
                else
                {
                    Gointersect = false;
                }
                if (Gointersect)
                {

                }

                if (skip)
                {
                    DrawingObjects.AddObject(new StringDraw("skip", GameParameters.OurGoalCenter.Extend(0.6, 0)), "5645646465564");
                    //Circle s = new Circle(GameParameters.OurGoalCenter, .5);
                    //Line ballrobot = new Line(model.BallState.Location, GameParameters.OurGoalCenter);
                    //List<Position2D> interscts = s.Intersect(ballrobot);
                    //if (interscts.Count > 0)
                    //{
                    //    Position2D? intersectp = interscts.OrderBy(y => y.DistanceFrom(model.BallState.Location)).FirstOrDefault();
                    //    if (intersectp.HasValue && GameParameters.IsInDangerousZone(intersectp.Value, false, -.5, out dist, out dist2) && GameParameters.IsInField(intersectp.Value, 0))
                    //    {
                    //        target = new Position2D(Math.Min(intersectp.Value.X, GameParameters.OurGoalCenter.X - .1), intersectp.Value.Y);
                    //        Planner.Add(robotID, target, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                    //    }
                    //    else
                    //    {
                    //        skip = false;
                    //    }
                    //}
                    //else
                    //{
                    //    skip = false;
                    //}
                    Position2D targetforSkip = new Position2D();
                    Position2D intersectforCollision = new Line(model.BallState.Location, model.BallState.Location + model.BallState.Speed.GetNormalizeToCopy(10)).IntersectWithLine(new Line(GameParameters.OurGoalCenter, model.BallState.Location)).Value;
                    if ((intersectforCollision - model.BallState.Location).InnerProduct(model.BallState.Speed) > 0)
                    {
                        targetforSkip = model.OurRobots[robotID].Location;
                    }
                    else
                    {
                        targetforSkip = GameParameters.OurGoalCenter.Extend(-.2, 0);
                    }
                    Planner.Add(robotID, targetforSkip, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);

                }
                else if (goActive)
                {
                    DrawingObjects.AddObject(new StringDraw("goActive", GameParameters.OurGoalCenter.Extend(0.7, 0)), "654564654565464");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = new Position2D();
                    double s = 0;
                    double s2 = 0;
                    if (GameParameters.IsInDangerousZone(model.BallState.Location, false, 0, out s, out s2) && model.BallState.Speed.Size < .1)
                    {
                        tar = TargetToKick(model, robotID);// Position2D.Zero;// model.OurRobots[7].Location;
                    }
                    else
                    {
                        tar = TargetToKick(model, robotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(model, robotID, tar);
                }
                else if (Gointersect)
                {
                    DrawingObjects.AddObject(new StringDraw("gointersect", GameParameters.OurGoalCenter.Extend(0.8, 0)), "987989856654564");
                    if (id == null)
                    {
                        Line ballSpeedLine = new Line(model.BallState.Location, model.BallState.Location + model.BallState.Speed.GetNormalizeToCopy(10));
                        List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, model.OurRobots[robotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, model.OurRobots[robotID].Location).Y);
                                ;
                                if (perp.DistanceFrom(model.OurRobots[robotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    Planner.Add(robotID, perp, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    Planner.Add(robotID, s, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(model.BallState.Location, -.1))
                            {
                                pos = model.BallState.Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, pos, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {

                            Position2D pos = model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                                double s = 0;
                                double s2 = 0;
                                Position2D tar = TargetToKick(model, robotID);
                                if (GameParameters.IsInDangerousZone(model.BallState.Location, false, 0, out s, out s2) && model.BallState.Speed.Size < .1)
                                {
                                    tar = Position2D.Zero;
                                }
                                else
                                {
                                    tar = TargetToKick(model, robotID);
                                }
                                GetSkill<GetBallSkill>().OutGoingSideTrack(model, robotID, tar);
                            }
                        }
                    }
                    else if (id != null && id.HasValue && model.Opponents.ContainsKey(id.Value))
                    {
                        Line ballSpeedLine = new Line(model.BallState.Location, model.Opponents[id.Value].Location);
                        List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Planner.ChangeDefaulteParams(robotID, false);
                                Planner.SetParameter(robotID, 8, 8);
                                Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, model.OurRobots[robotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, model.OurRobots[robotID].Location).Y);
                                ;
                                if (perp.DistanceFrom(model.OurRobots[robotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    Planner.Add(robotID, perp, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    Planner.Add(robotID, s, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(model.Opponents[id.Value].Location, -.1))
                            {
                                pos = model.Opponents[id.Value].Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, pos, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {
                            Position2D pos = model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }


                    }
                }
                else
                {
                    DrawingObjects.AddObject(new StringDraw("Nothing", GameParameters.OurGoalCenter.Extend(0.8, 0)), "213213213213212321");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    double s = 0;
                    double s2 = 0;
                    Position2D tar = TargetToKick(model, robotID);
                    if (GameParameters.IsInDangerousZone(model.BallState.Location, false, 0, out s, out s2) && model.BallState.Speed.Size < .1)
                    {
                        tar = Position2D.Zero;
                    }
                    else
                    {
                        tar = TargetToKick(model, robotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(model, robotID, tar);
                }
                Obstacles obs = new Obstacles(model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { robotID }, null);
                Vector2D v = Vector2D.FromAngleSize(model.OurRobots[robotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 3;

                if (model.BallState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(model.OurRobots[robotID].Angle.Value) < 100 || obs.Meet(model.BallState, new SingleObjectState(model.BallState.Location + v, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(robotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);

            }
            #endregion
            #region KickToGoal
            else if (CurrentState == (int)GoalieStates.KickToGoal)
            {
                //Position2D intersect = IntersectFind(model, robotID, initialpos, inf.DefenderPosition.Value);// bayad dorost she hatman bayad ba khate robot intersect gerfte shavad 

                //double velocity = model.BallState.Speed.Size;
                //double ballcoeff = root(-.3, velocity, model.BallState.Location.DistanceFrom(intersect)); // TODO: intersect


                if (model.BallState.Speed.InnerProduct(model.OurRobots[robotID].Location - model.BallState.Location) > 0)
                    SWc = GetSkill<GoaliDiveSkill>().Dive(engine, model, robotID, true, 200);//NewDive(engine, model, robotID, true, 200);
                else
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(model, robotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(model, robotID, tar);
                }
            }
            #endregion
            #region KickToRobot
            else if (CurrentState == (int)GoalieStates.KickToRobot)
                Planner.Add(robotID, model.OurRobots[robotID].Location, (model.BallState.Location - model.OurRobots[robotID].Location).AngleInDegrees, false);
            #endregion
            #region EatTheBall
            else if (CurrentState == (int)GoalieStates.EathTheBall)
            {
                Planner.Add(robotID, GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.2)), (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
            }
            #endregion
            #region Rahmati
            if (CurrentState == (int)GoalieStates.Rahmati)
            {

                Position2D posongoal = new Position2D();

                DefenceInfo inf1 = null;
                if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                    inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();
                DefenceInfo inf2 = null;
                //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                //    inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();


                Defender1Delayed = false;
                if (StaticDefender1ID.HasValue && inf1 != null && inf1.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender1ID.Value))
                {
                    if (model.OurRobots[StaticDefender1ID.Value].Location.DistanceFrom(inf1.DefenderPosition.Value) > .1)
                    {
                        Defender1Delayed = true;
                    }
                }
                Defender2Delayed = false;
                if (StaticDefender2ID.HasValue && inf2 != null && inf1.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender2ID.Value))
                {
                    if (/*model.OurRobots[StaticDefender2ID.Value].Location.DistanceFrom(inf2.DefenderPosition.Value) > .1*/false)
                    {
                        Defender2Delayed = true;
                    }
                }

                if (!StaticDefender1ID.HasValue)
                {
                    Defender1Delayed = true;
                }
                if (!StaticDefender2ID.HasValue)
                {
                    Defender2Delayed = true;
                }

                posongoal = GameParameters.OurGoalCenter;
                if (Defender1Delayed && Defender2Delayed)
                {
                    posongoal = GameParameters.OurGoalCenter;
                }
                else if (Defender1Delayed)
                {
                    //Position2D intersect1 = inf2.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - inf2.DefenderPosition.Value).AngleInRadians - (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                    //Position2D? intersect2 = new Line(defenceSate.Location, intersect1).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));

                    //if (intersect2.HasValue)
                    //{
                    //    if (intersect2.Value.Y > GameParameters.OurGoalRight.Y && intersect2.Value.Y < GameParameters.OurGoalLeft.Y)
                    //    {
                    //        posongoal = intersect2.Value;//new Position2D(GameParameters.OurGoalCenter.X, (intersect2.Value.Y));// + GameParameters.OurGoalLeft.Y) / 2);
                    //        DrawingObjects.AddObject(posongoal, "545649845646");
                    //    }
                    //    else
                    //    {
                    //        posongoal = GameParameters.OurGoalCenter;
                    //    }
                    //}
                    //else
                    //{
                    posongoal = GameParameters.OurGoalCenter;
                    //}
                }
                else if (Defender2Delayed)
                {

                    Position2D intersect1 = inf1.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - inf1.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                    DrawingObjects.AddObject(intersect1, "9879878979879");
                    Position2D? intersect2 = new Line(defenceSate.Location, intersect1).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));
                    DrawingObjects.AddObject(intersect2, "68796464654");

                    if (intersect2.HasValue)
                    {
                        if (intersect2.Value.Y > GameParameters.OurGoalRight.Y && intersect2.Value.Y < GameParameters.OurGoalLeft.Y)
                        {
                            posongoal = new Position2D(GameParameters.OurGoalCenter.X, (intersect2.Value.Y + GameParameters.OurGoalRight.Y) / 2);
                            DrawingObjects.AddObject(posongoal, "654564564");
                        }
                        else
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }
                    }
                    else
                    {
                        posongoal = GameParameters.OurGoalCenter;
                    }
                }
                else
                {
                    posongoal = GameParameters.OurGoalCenter;
                }

                DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                Position2D currentPos = model.OurRobots[robotID].Location;

                DefenceInfo delayedrobot = new DefenceInfo();

                if (Defender2Delayed)
                {
                    delayedrobot = inf1;
                }
                else if (Defender1Delayed)
                {
                    delayedrobot = inf2;
                }
                else
                {
                    delayedrobot = inf1;
                }
                Position2D tempintersect1 = delayedrobot.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - delayedrobot.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                //DrawingObjects.AddObject(intersect1, "9879878979879");
                Position2D? tempintersect2 = new Line(defenceSate.Location, delayedrobot.DefenderPosition.Value).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));

                Circle goalcenter = new Circle(GameParameters.OurGoalCenter, GameParameters.SafeRadi(defenceSate, -.20));
                //Position2D? mainuntersect = goalcenter.Intersect(new Line(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X - .05), defenceSate.Location.Y), posongoal)).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(model.BallState.Location)).FirstOrDefault();
                //DrawingObjects.AddObject(mainuntersect, "6465464545");
                Vector2D leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                Vector2D rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                Position2D? mainuntersect = new Position2D();

                if (!GameParameters.IsInField(defenceSate.Location, 0))
                {
                    if (defenceSate.Location.X > 0)
                    {
                        if (defenceSate.Location.Y > 0)
                            defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                        else
                        {
                            defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                        }
                    }
                    else
                    {
                        if (defenceSate.Location.Y > 0)
                            defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                        else
                        {
                            defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                        }
                    }

                }


                if (Defender1Delayed && tempintersect2.HasValue)
                {
                    leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                    rightVector = tempintersect2.Value - defenceSate.Location;
                }
                if (Defender2Delayed && tempintersect2.HasValue)
                {
                    leftVector = tempintersect2.Value - defenceSate.Location;
                    rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                }
                if (Defender1Delayed && Defender2Delayed)
                {
                    leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                    rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                }
                Line bisector = Vector2D.Bisector(leftVector, rightVector, defenceSate.Location);
                mainuntersect = goalcenter.Intersect(bisector).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(defenceSate.Location)).FirstOrDefault();

                #region Normal
                Position2D go = GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.40));
                #endregion


                DrawingObjects.AddObject(mainuntersect, "654564655454564");
                Position2D mainTarget = (mainuntersect.HasValue && mainuntersect.Value != new Position2D()) ? mainuntersect.Value : go;
                Vector2D targetvector = defenceSate.Location - posongoal;

                Planner.ChangeDefaulteParams(robotID, false);
                Planner.SetParameter(robotID, 8, 4);
                Position2D normalprep = targetvector.PrependecularPoint(posongoal, currentPos);
                Position2D perp = (normalprep.X > GameParameters.OurGoalCenter.X) ? posongoal + targetvector.GetNormalizeToCopy(.2) : normalprep;
                perp = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - .1, perp.X), perp.Y);

                if (perp.DistanceFrom(currentPos) > .25)
                {
                    gotoperp = true;
                }
                else if (perp.DistanceFrom(currentPos) < .2)
                {
                    gotoperp = false;
                }
                mainTarget = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - .1, mainTarget.X), mainTarget.Y);
                if (gotoperp)
                {
                    Planner.Add(robotID, perp, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
                else
                {
                    Planner.Add(robotID, mainTarget, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }

            }
            #endregion
            #region OpponentIsInPass
            if (CurrentState == (int)GoalieStates.OpponentInPassState)
            {
                if (model.Opponents.ContainsKey(kickerOpponent))
                {
                    Line intersect = new Line(model.Opponents[kickerOpponent].Location, model.Opponents[kickerOpponent].Location + Vector2D.FromAngleSize(model.Opponents[kickerOpponent].Angle.Value * Math.PI / 180, 10));
                    Line goalLine = new Line(GameParameters.OurGoalLeft.Extend(-.1, 0), GameParameters.OurGoalRight.Extend(-.1, 0));
                    Position2D? intersectpos = goalLine.IntersectWithLine(intersect);
                    if (intersectpos.HasValue && intersectpos.Value.Y < .5 && intersectpos.Value.Y > -.5)
                        Planner.Add(robotID, intersectpos.Value, (model.Opponents[kickerOpponent].Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
            }
            #endregion
            #region PreDive
            if (CurrentState == (int)GoalieStates.PreDive)
            {
                Position2D plannerTarget = new Position2D();
                Position2D target = defenceSate.Location;

                Position2D ourGoalLeftWithextend = GameParameters.OurGoalLeft.Extend(0, .04);
                Position2D ourGoalRightWithextend = GameParameters.OurGoalRight.Extend(0, -.04);

                Vector2D leftCornerCenterGoal = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                Vector2D rightCornerCentergoal = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;

                Vector2D targetGoalCenter = target - GameParameters.OurGoalCenter;

                double angleBetweenLeftAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(leftCornerCenterGoal, targetGoalCenter));
                angleBetweenLeftAndtarget = (angleBetweenLeftAndtarget < 180) ? angleBetweenLeftAndtarget : 360 - angleBetweenLeftAndtarget;

                double angleBetweenRightAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(rightCornerCentergoal, targetGoalCenter));
                angleBetweenRightAndtarget = (angleBetweenRightAndtarget < 180) ? angleBetweenRightAndtarget : 360 - angleBetweenRightAndtarget;

                bool Left = (angleBetweenLeftAndtarget < angleBetweenRightAndtarget) ? true : false;
                double mainAngle = (Left) ? angleBetweenLeftAndtarget : angleBetweenRightAndtarget;
                Position2D goalNearCorner = (Left) ? ourGoalLeftWithextend : ourGoalRightWithextend;

                double dist = (.8 * mainAngle) / 90;

                Vector2D LeftwithExtend = (ourGoalLeftWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);
                Vector2D RightWithExtend = (ourGoalRightWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);

                Position2D leftGoal = target + LeftwithExtend;
                Position2D rightGoal = target + RightWithExtend;

                Line MovementLine = new Line(leftGoal, rightGoal);

                int? oppBallOwner = model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
                Vector2D RobotHeadVector = (GameParameters.OurGoalCenter - model.BallState.Location);
                if (oppBallOwner.HasValue)
                    RobotHeadVector = Vector2D.FromAngleSize(((model.Opponents[oppBallOwner.Value].Angle.Value * Math.PI / 180) + (model.BallState.Location - model.Opponents[oppBallOwner.Value].Location).AngleInRadians) / 2, 10);

                Line RobotHeadLine = new Line(model.BallState.Location, model.BallState.Location + RobotHeadVector);

                Position2D? intersection = RobotHeadLine.IntersectWithLine(MovementLine);

                if (intersection.HasValue)
                {
                    Position2D intersect = intersection.Value;
                    if (leftGoal.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter)
                    {
                        Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy(leftGoal.DistanceFrom(rightGoal) / 2);
                        Position2D robottarget = rightGoal + rightToLeft;

                        if (robottarget.DistanceFrom(goalNearCorner) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            robottarget = rightGoal + rightToLeft;
                        }
                        plannerTarget = robottarget;
                    }
                    else
                    {
                        if (intersect.DistanceFrom(leftGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            Position2D robottarget = leftGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (intersect.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            Position2D robottarget = rightGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (!Vector2D.IsBetween(ourGoalLeftWithextend - target, ourGoalRightWithextend - target, intersect - target))
                        {
                            if (Vector2D.IsBetween(ourGoalLeftWithextend - target, GameParameters.OurLeftCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                                Position2D robottarget = leftGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else if (Vector2D.IsBetween(ourGoalRightWithextend - target, GameParameters.OurRightCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                                Position2D robottarget = rightGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else
                            {
                                plannerTarget = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.5);
                            }
                        }
                        else
                        {
                            plannerTarget = intersect;
                        }
                    }
                }
                else
                {
                    plannerTarget = GameParameters.OurGoalCenter - (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.4);
                }
                DrawingObjects.AddObject(new Circle(plannerTarget, .1, new Pen(Brushes.Blue, .01f)), "54564646646546");
                DrawingObjects.AddObject(new Circle(intersection.Value, .1, new Pen(Brushes.HotPink, .01f)), "56456464");

                Planner.Add(robotID, plannerTarget, (target - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);


            }
            #endregion
            #region Piston
            else if (CurrentState == (int)GoalieStates.Piston)
            {

                DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                defenceSate = (id.HasValue) ? model.Opponents[id.Value] : ballState;
                double dist = PistonDistance(defenceSate.Location);
                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-dist);

                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                postoGo = new Position2D(x, postoGo.Y);
                Planner.Add(robotID, postoGo, (defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - model.OurRobots[robotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
            }
            #endregion

            return new SingleWirelessCommand();

        }
        #endregion
        double dist = 0;
        public bool useNewRahmati = true;
        private bool justOnceFlag1 = true;
        double gotoPerpDist = 0.2;
        public SingleWirelessCommand Perform(GameStrategyEngine engine, WorldModel Model, int robotID, SingleObjectState defenceSate)
        {
            int? StaticDefender1ID = null;
            int? StaticDefender2ID = null;
            StaticDefender1IDG = StaticDefender1ID;
            StaticDefender2IDG = StaticDefender2ID;
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                BallState = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                BallState = Model.BallState;
            }
            if (StaticDefender1ID.HasValue)
                StaticDefenderCurrentPos = Model.OurRobots[StaticDefender1ID.Value].Location;
            if (StaticDefender2ID.HasValue)
                StaticDefender2CurrentPos = Model.OurRobots[StaticDefender2ID.Value].Location;
            int? id = StaticRB(engine, Model);
            //DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");
            ///mostafa
            List<Planning.GamePlanner.Types.VisibleGoalInterval> ourGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();
            List<Planning.GamePlanner.Types.VisibleGoalInterval> oppGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();
            List<int> oppRobotIds = new List<int>();
            List<int> ourRobotIds = new List<int>();
            oppRobotIds = Model.Opponents.Keys.Where(a => Model.Opponents.ContainsKey(a)).ToList();
            ourRobotIds = Model.OurRobots.Keys.Where(a => Model.OurRobots.ContainsKey(a)).ToList();
            if (FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value))
            {
                foreach (var item in ourRobotIds)
                {
                    if (item == FreekickDefence.StaticCBID.Value)
                    {
                        ourRobotIds.Remove(FreekickDefence.StaticCBID.Value);
                        break;
                    }
                }
            }
            CalculateGoalIntervals(Model, out ourGoalinterval, out oppGoalinterval, false, true, oppRobotIds, ourRobotIds);
            ///
            double maxdist = 0;
            Line maxline = new Line();
            Planning.GamePlanner.Types.VisibleGoalInterval maxInterval = new Planning.GamePlanner.Types.VisibleGoalInterval();
            Position2D middlepos = new Position2D();
            Vector2D vec = new Vector2D();
            Line middlePosToBallLine = new Line();
            foreach (var item in ourGoalinterval)
            {
                if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                {
                    maxdist = Math.Abs(item.interval.Start - item.interval.End);
                    maxInterval = item;
                    maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                }
            }
            Position2D MyLeftGoal = /*GameParameters.OurGoalLeft*/new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End);
            Position2D MyRightGoal = /*GameParameters.OurGoalRight*/new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);
            if (FreekickDefence.StaticCBID.HasValue)
            {
                double d1, d2;
                d1 = MyLeftGoal.DistanceFrom(GameParameters.OurGoalCenter);
                d2 = MyRightGoal.DistanceFrom(GameParameters.OurGoalCenter);
                if (d1 < d2)
                {
                    Vector2D v1 = MyRightGoal - MyLeftGoal;
                    Vector2D v2 = MyLeftGoal - MyRightGoal;
                    v1 = v1.GetNormalizeToCopy(0.04);
                    v2 = v2.GetNormalizeToCopy(.04);
                    MyLeftGoal = MyLeftGoal + v1;
                    MyRightGoal = MyRightGoal + v2;
                }
                else
                {
                    Vector2D v1 = MyLeftGoal - MyRightGoal;
                    Vector2D v2 = MyRightGoal - MyLeftGoal;
                    v1 = v1.GetNormalizeToCopy(0.04);
                    v2 = v2.GetNormalizeToCopy(0.04);
                    MyRightGoal = MyRightGoal + v1;
                    MyLeftGoal = MyLeftGoal + v2;
                }
            }
            DrawingObjects.AddObject(new Circle(MyLeftGoal, 0.02, new Pen(Brushes.Yellow, 0.02f)), MyLeftGoal.Y.ToString() + "53456");
            DrawingObjects.AddObject(new Circle(MyRightGoal, 0.02, new Pen(Brushes.Yellow, 0.02f)), MyRightGoal.Y.ToString() + "53456");

            defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;
            SingleWirelessCommand SWc = new SingleWirelessCommand();
            #region Normal
            if (CurrentState == (int)GoalieStates.Normal)
            {
                spin = true;
                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-0.4);
                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                postoGo = new Position2D(x, postoGo.Y);
                SWc = GetSkill<GotoPointSkill>().GotoPoint(Model, robotID, postoGo, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, false, false, 3, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
                Planner.Add(robotID, s, false);

            }
            #endregion
            #region InPenaltyArea

            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                spin = true;
                bool useoldInPnaltyArea = true;// false;
                if (Model.BallState.Location.X > Model.OurRobots[robotID].Location.X)
                {
                    useoldInPnaltyArea = true;
                }
                Vector2D ballSpeed = BallState.Speed;
                double g = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[robotID].Location - BallState.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(g) * Math.Cos(g))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(g)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = 0.1;
                Position2D IntersectPoint = new Position2D();
                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[robotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);
                Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                Line goalline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                Position2D? intersect = ballspeed.IntersectWithLine(goalline);
                bool skip = false;
                bool goActive = false;
                bool Gointersect = false;
                bool togoal = true;
                Position2D target = new Position2D();

                if (intersect.HasValue)
                {
                    Position2D intersects = intersect.Value;
                    if (((intersects.Y > GameParameters.OurGoalLeft.Y + .15 && intersects.Y < 1.15) || (intersects.Y < GameParameters.OurGoalRight.Y - .15 && intersects.Y > -1.15)) && Model.BallState.Speed.Size > .3 && Model.BallState.Speed.InnerProduct(GameParameters.OurGoalCenter - Model.BallState.Location) > 0)
                    {
                        skip = true;
                    }
                    else
                    {
                        skip = false;
                    }
                    if (((intersects.Y < GameParameters.OurGoalLeft.Y + .15 && intersects.Y > 0) || (intersects.Y > GameParameters.OurGoalRight.Y - .15 && intersects.Y < 0)))
                    {
                        togoal = true;
                    }
                    else
                    {
                        togoal = false;
                    }
                }
                else
                {
                    skip = false;
                }

                if ((acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                {
                    goActive = true;
                }
                else
                {
                    goActive = false;
                }
                if (acceptable2 * 1.2 < ballSpeed.Size)
                {
                    Gointersect = true;
                }
                else
                {
                    Gointersect = false;
                }
                if (Gointersect)
                {

                }

                if (skip)
                {
                    DrawingObjects.AddObject(new StringDraw("skip", GameParameters.OurGoalCenter.Extend(0.6, 0)), "5645646465564");
                    Position2D targetforSkip = new Position2D();
                    Position2D intersectforCollision = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10)).IntersectWithLine(new Line(GameParameters.OurGoalCenter, Model.BallState.Location)).Value;
                    if ((intersectforCollision - Model.BallState.Location).InnerProduct(Model.BallState.Speed) > 0)
                    {
                        targetforSkip = Model.OurRobots[robotID].Location;
                    }
                    else
                    {
                        targetforSkip = GameParameters.OurGoalCenter.Extend(-.2, 0);
                    }
                    Planner.Add(robotID, targetforSkip, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
                else if (goActive)
                {
                    DrawingObjects.AddObject(new StringDraw("goActive", GameParameters.OurGoalCenter.Extend(0.7, 0)), "654564654565464");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = new Position2D();
                    double s = 0;
                    double s2 = 0;
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                    {
                        tar = TargetToKick(Model, robotID);
                    }
                    else
                    {
                        tar = TargetToKick(Model, robotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, robotID, tar);
                }
                else if (Gointersect)
                {
                    DrawingObjects.AddObject(new StringDraw("gointersect", GameParameters.OurGoalCenter.Extend(0.8, 0)), "987989856654564");
                    if (id == null)
                    {
                        Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                        List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[robotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[robotID].Location).Y);
                                ;
                                if (perp.DistanceFrom(Model.OurRobots[robotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    Planner.Add(robotID, perp, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    Planner.Add(robotID, s, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(Model.BallState.Location, -.1))
                            {
                                pos = Model.BallState.Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, pos, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {

                            Position2D pos = Model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                //GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                                //double s = 0;
                                //double s2 = 0;
                                //Position2D tar = TargetToKick(Model, robotID);
                                //if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                                //{
                                //    tar = Position2D.Zero;
                                //}
                                //else
                                //{
                                //    tar = TargetToKick(Model, robotID);
                                //}
                                //GetSkill<GetBallSkill>().OutGoingSideTrack(Model, robotID, tar);
                                double angle = 180;
                                if (Model.OurRobots[robotID].Angle.HasValue)
                                {
                                    angle = Model.OurRobots[robotID].Angle.Value;
                                }

                                Planner.Add(robotID, new Position2D(Model.OurRobots[robotID].Location.X, Model.BallState.Location.Y), angle, PathType.UnSafe, false, false, false, false);
                            }
                        }
                    }
                    else if (id != null && id.HasValue && Model.Opponents.ContainsKey(id.Value))
                    {
                        Line ballSpeedLine = new Line(Model.BallState.Location, Model.Opponents[id.Value].Location);
                        List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Planner.ChangeDefaulteParams(robotID, false);
                                Planner.SetParameter(robotID, 8, 8);
                                Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[robotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[robotID].Location).Y);
                                ;
                                if (perp.DistanceFrom(Model.OurRobots[robotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    Planner.Add(robotID, perp, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    Planner.Add(robotID, s, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(Model.Opponents[id.Value].Location, -.1))
                            {
                                pos = Model.Opponents[id.Value].Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, pos, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {
                            Position2D pos = Model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Planner.Add(robotID, s, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }


                    }
                }
                else
                {
                    DrawingObjects.AddObject(new StringDraw("Nothing", GameParameters.OurGoalCenter.Extend(0.8, 0)), "213213213213212321");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    double s = 0;
                    double s2 = 0;
                    Position2D tar = TargetToKick(Model, robotID);
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                    {
                        tar = Position2D.Zero;
                    }
                    else
                    {
                        tar = TargetToKick(Model, robotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, robotID, tar);
                }
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { robotID }, null);
                Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 3;

                if (Model.BallState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[robotID].Angle.Value) < 100 || obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(robotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
                spin = false;

            }
            #endregion
            #region KickToGoal
            else if (CurrentState == (int)GoalieStates.KickToGoal)
            {
                spin = true;
                //Position2D intersect = IntersectFind(model, robotID, initialpos, inf.DefenderPosition.Value);// bayad dorost she c bayad ba khate robot intersect gerfte shavad 

                //double velocity = model.BallState.Speed.Size;
                //double ballcoeff = root(-.3, velocity, model.BallState.Location.DistanceFrom(intersect)); // TODO: intersect


                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[robotID].Location - Model.BallState.Location) > 0)
                    SWc = GetSkill<GoaliDiveSkill>().NewDive(engine, Model, robotID, true, 200);//NewDive(engine, Model, robotID, true, 200);
                else
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(Model, robotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, robotID, tar);
                }
            }

            #endregion
            #region KickToRobot
            else if (CurrentState == (int)GoalieStates.KickToRobot)
            {
                spin = true;
                Planner.Add(robotID, Model.OurRobots[robotID].Location, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, false);
            }
            #endregion
            #region EatTheBall
            else if (CurrentState == (int)GoalieStates.EathTheBall)
            {
                spin = true;
                Planner.Add(robotID, GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.2)), (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
            }
            #endregion
            #region Rahmati
            if (CurrentState == (int)GoalieStates.Rahmati)
            {
                spin = true;
                foreach (var item in ourGoalinterval)
                {
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                    {
                        maxdist = Math.Abs(item.interval.Start - item.interval.End);
                        maxInterval = item;
                        maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                    }
                }


                double r = 0.11;//GameDefinitions.RobotParameters.OurRobotParams.Diameter/2);
                Position2D CBcirclecenter = FreekickDefence.CenterCurrentPosition;
                Circle CBcenter = new Circle(CBcirclecenter, r);
                DrawingObjects.AddObject(new Circle(CBcirclecenter, r, new Pen(Brushes.Red, 0.02f)), CBcirclecenter.Y.ToString() + "578302");

                if ((FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value)
                    && !CBcenter.IsInCircle(Model.OurRobots[FreekickDefence.StaticCBID.Value].Location)))
                {
                    useNewRahmati = false;
                }
                if (!(FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value))
                    || Math.Sqrt(maxInterval.interval.End - maxInterval.interval.Start) > OpengapSize)
                {
                    useNewRahmati = false;
                }
                else
                    useNewRahmati = true;

                if ((FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value)
                    && Model.OurRobots[FreekickDefence.StaticCBID.Value].Location.DistanceFrom(CBcirclecenter) < 0.5))
                {
                    useNewRahmati = true;
                }
                else
                {
                    useNewRahmati = false;
                }

                DrawingObjects.AddObject(new StringDraw("use New Rahmati:" + useNewRahmati.ToString(), Position2D.Zero.Extend(-0.6, 0)), "ouidsyfrtoi");
                if (!useNewRahmati)
                {
                    #region use old Rahmati
                    //if (!useNewRahmati)
                    {
                        Position2D posongoal = new Position2D();

                        DefenceInfo inf1 = null;
                        if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                            inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();
                        DefenceInfo inf2 = null;
                        //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                        //    inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();


                        Defender1Delayed = false;
                        if (StaticDefender1ID.HasValue && inf1 != null && inf1.DefenderPosition.HasValue && Model.OurRobots.ContainsKey(StaticDefender1ID.Value))
                        {
                            if (Model.OurRobots[StaticDefender1ID.Value].Location.DistanceFrom(inf1.DefenderPosition.Value) > .1)
                            {
                                Defender1Delayed = true;
                            }
                        }
                        Defender2Delayed = false;
                        if (StaticDefender2ID.HasValue && inf2 != null && inf1.DefenderPosition.HasValue && Model.OurRobots.ContainsKey(StaticDefender2ID.Value))
                        {
                            if (/*model.OurRobots[StaticDefender2ID.Value].Location.DistanceFrom(inf2.DefenderPosition.Value) > .1*/false)
                            {
                                Defender2Delayed = true;
                            }
                        }

                        if (!StaticDefender1ID.HasValue)
                        {
                            Defender1Delayed = true;
                        }
                        if (!StaticDefender2ID.HasValue)
                        {
                            Defender2Delayed = true;
                        }

                        posongoal = GameParameters.OurGoalCenter;
                        if (Defender1Delayed && Defender2Delayed)
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }
                        else if (Defender1Delayed)
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }
                        else if (Defender2Delayed)
                        {
                            Position2D intersect1 = inf1.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - inf1.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                            DrawingObjects.AddObject(intersect1, "9879878979879");
                            Position2D? intersect2 = new Line(defenceSate.Location, intersect1).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));
                            DrawingObjects.AddObject(intersect2, "68796464654");

                            if (intersect2.HasValue)
                            {
                                if (intersect2.Value.Y > GameParameters.OurGoalRight.Y && intersect2.Value.Y < GameParameters.OurGoalLeft.Y)
                                {
                                    posongoal = new Position2D(GameParameters.OurGoalCenter.X, (intersect2.Value.Y + GameParameters.OurGoalRight.Y) / 2);
                                    DrawingObjects.AddObject(posongoal, "654564564");
                                }
                                else
                                {
                                    posongoal = GameParameters.OurGoalCenter;
                                }
                            }
                            else
                            {
                                posongoal = GameParameters.OurGoalCenter;
                            }
                        }
                        else
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }

                        DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                        Position2D currentPos = Model.OurRobots[robotID].Location;

                        DefenceInfo delayedrobot = new DefenceInfo();

                        if (Defender2Delayed)
                        {
                            delayedrobot = inf1;
                        }
                        else if (Defender1Delayed)
                        {
                            delayedrobot = inf2;
                        }
                        else
                        {
                            delayedrobot = inf1;
                        }
                        Position2D tempintersect1 = delayedrobot.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - delayedrobot.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                        Position2D? tempintersect2 = new Line(defenceSate.Location, delayedrobot.DefenderPosition.Value).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));

                        Circle goalcenter = new Circle(GameParameters.OurGoalCenter, GameParameters.SafeRadi(defenceSate, -.20));
                        //Position2D? mainuntersect = goalcenter.Intersect(new Line(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X - .05), defenceSate.Location.Y), posongoal)).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(model.BallState.Location)).FirstOrDefault();
                        //DrawingObjects.AddObject(mainuntersect, "6465464545");
                        Vector2D leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                        Vector2D rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                        Position2D? mainuntersect = new Position2D();

                        if (!GameParameters.IsInField(defenceSate.Location, 0))
                        {
                            if (defenceSate.Location.X > 0)
                            {
                                if (defenceSate.Location.Y > 0)
                                    defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                                else
                                {
                                    defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                                }
                            }
                            else
                            {
                                if (defenceSate.Location.Y > 0)
                                    defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                                else
                                {
                                    defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                                }
                            }

                        }


                        if (Defender1Delayed && tempintersect2.HasValue)
                        {
                            leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                            rightVector = tempintersect2.Value - defenceSate.Location;
                        }
                        if (Defender2Delayed && tempintersect2.HasValue)
                        {
                            leftVector = tempintersect2.Value - defenceSate.Location;
                            rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                        }
                        if (Defender1Delayed && Defender2Delayed)
                        {
                            leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                            rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                        }
                        Line bisector = Vector2D.Bisector(leftVector, rightVector, defenceSate.Location);
                        mainuntersect = goalcenter.Intersect(bisector).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(defenceSate.Location)).FirstOrDefault();

                        #region Normal
                        Position2D go = GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.40));
                        #endregion


                        DrawingObjects.AddObject(mainuntersect, "654564655454564");
                        Position2D mainTarget = (mainuntersect.HasValue && mainuntersect.Value != new Position2D()) ? mainuntersect.Value : go;
                        Vector2D targetvector = defenceSate.Location - posongoal;

                        Planner.ChangeDefaulteParams(robotID, false);
                        Planner.SetParameter(robotID, 8, 4);
                        Position2D normalprep = targetvector.PrependecularPoint(posongoal, currentPos);
                        Position2D perp = (normalprep.X > GameParameters.OurGoalCenter.X) ? posongoal + targetvector.GetNormalizeToCopy(.2) : normalprep;
                        double d1, d2;
                        if (GameParameters.IsInDangerousZone(Model.OurRobots[robotID].Location, false, .05, out d1, out d2))
                        {
                            gotoPerpDist = .05;
                        }
                        else
                        {
                            gotoPerpDist = .2;
                        }

                        perp = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - gotoPerpDist, perp.X), perp.Y);

                        if (perp.DistanceFrom(currentPos) > (gotoPerpDist + .05))
                        {
                            gotoperp = true;
                        }
                        else if (perp.DistanceFrom(currentPos) < gotoPerpDist)
                        {
                            gotoperp = false;
                        }
                        mainTarget = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - .1, mainTarget.X), mainTarget.Y);
                        if (gotoperp)
                        {
                            Planner.Add(robotID, perp, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                        else
                        {
                            Planner.Add(robotID, mainTarget, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region use New Rahmati
                    //if (useNewRahmati)

                    #region got Prep Line

                    Position2D currentPos = Model.OurRobots[robotID].Location;
                    Position2D posongoal = GameParameters.OurGoalCenter;
                    Position2D go = GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.40));
                    Vector2D leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                    Vector2D rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                    Position2D? mainuntersect = new Position2D();
                    Line bisector = Vector2D.Bisector(leftVector, rightVector, defenceSate.Location);
                    Circle goalcenter = new Circle(GameParameters.OurGoalCenter, GameParameters.SafeRadi(defenceSate, -.20));
                    mainuntersect = goalcenter.Intersect(bisector).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(defenceSate.Location)).FirstOrDefault();
                    Position2D mainTarget = (mainuntersect.HasValue && mainuntersect.Value != new Position2D()) ? mainuntersect.Value : go;
                    Vector2D targetvector = defenceSate.Location - posongoal;
                    Planner.ChangeDefaulteParams(robotID, false);
                    Planner.SetParameter(robotID, 8, 4);
                    Position2D normalprep = targetvector.PrependecularPoint(posongoal, currentPos);
                    Position2D perp = (normalprep.X > GameParameters.OurGoalCenter.X) ? posongoal + targetvector.GetNormalizeToCopy(.2) : normalprep;
                    double d1, d2;
                    if (GameParameters.IsInDangerousZone(Model.OurRobots[robotID].Location, false, .05, out d1, out d2))
                    {
                        gotoPerpDist = .05;
                    }
                    else
                    {
                        gotoPerpDist = .2;
                    }
                    perp = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - gotoPerpDist, perp.X), perp.Y);

                    mainTarget = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - gotoPerpDist, mainTarget.X), mainTarget.Y);
                    double dist1, dist2;

                    if (perp.DistanceFrom(currentPos) > (gotoPerpDist + 0.05))
                    {
                        gotoperp = true;
                    }
                    else if (perp.DistanceFrom(currentPos) < gotoPerpDist)
                    {
                        gotoperp = false;
                    }
                    //if (GameParameters.IsInDangerousZone(Model.OurRobots[robotID].Location,false,0.02,out dist1,out dist2))
                    //{
                    //    gotoperp = true;
                    //}
                    //else if (!GameParameters.IsInDangerousZone(Model.OurRobots[robotID].Location, false, 0.02, out dist1, out dist2) || (perp.DistanceFrom(currentPos) < .15))
                    //{
                    //    gotoperp = false;
                    //}
                    //if ((new Circle(GameParameters.OurGoalLeft, 0.1)).IsInCircle(perp) || (new Circle(GameParameters.OurGoalRight, 0.1)).IsInCircle(perp))
                    //{
                    //    gotoperp = true;
                    //}
                    if (gotoperp)
                    {
                        Planner.Add(robotID, perp, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                    }
                    #endregion

                    #region go to intersect Pos
                    if (!gotoperp || GameParameters.IsInDangerousZone(Model.OurRobots[robotID].Location, false, 0.05, out dist1, out dist2))
                    {
                        //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), .02, new Pen(Brushes.Blue, 0.02f)), maxInterval.interval.Start.ToString() + " 5687");
                        //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End), .02, new Pen(Brushes.Blue, 0.02f)), maxInterval.interval.End.ToString() + " 5687");
                        vec = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End) - new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);
                        vec = Vector2D.FromAngleSize(vec.AngleInRadians, vec.Size / 2);
                        middlepos = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start) + vec;
                        //DrawingObjects.AddObject(new Circle(middlepos, .02, new Pen(Brushes.Blue, 0.02f)), middlepos.Y.ToString() + " 5687");
                        Position2D targetBall = (Model.BallState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalCenter.X, Model.BallState.Location.Y)) > GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2) ? Model.BallState.Location : Model.BallState.Location.Extend(-0.07, 0);
                        middlePosToBallLine = new Line(targetBall, middlepos);
                        //DrawingObjects.AddObject(new Line(Model.BallState.Location, middlepos, new Pen(Brushes.Blue, 0.01f)), middlePosToBallLine.Tail.Y.ToString() + " 565436547 ");

                        Line TransverseVerticalLibeWithGoalie = new Line(Model.OurRobots[robotID].Location, Model.OurRobots[robotID].Location.Extend(0, 1));//Transverse Vertical Libe With Goalie
                        Position2D? IntersectPos = null;
                        IntersectPos = TransverseVerticalLibeWithGoalie.IntersectWithLine(middlePosToBallLine);
                        if (IntersectPos.HasValue)
                        {
                            double size = 0.5;
                            Vector2D vvv = Model.BallState.Location - middlepos;
                            Position2D t = middlepos + vvv.GetNormalizeToCopy(size);
                            GoalieCurrentPos = t;
                            Line goalLin = new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft);
                            Line GoalerToGoalLine = new Line(Model.OurRobots[robotID].Location, new Position2D(GameParameters.OurGoalCenter.X, Model.OurRobots[robotID].Location.Y));
                            Position2D? intersectPos = GoalerToGoalLine.IntersectWithLine(goalLin);
                            //if (intersectPos.HasValue)
                            //{
                            //    double tresh = 0.01;
                            //    double distance = Model.OurRobots[robotID].Location.DistanceFrom(intersectPos.Value) + tresh;
                            //    if (distance < GameDefinitions.RobotParameters.OurRobotParams.Diameter / 1.25)
                            //    {
                            //        Position2D ourGoalSide = (Model.BallState.Location.Y > 0 ? GameParameters.OurGoalLeft : GameParameters.OurGoalRight) + ((Model.BallState.Location.Y > 0 ? GameParameters.OurGoalLeft : GameParameters.OurGoalRight) - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.06);
                            //        GoalieCurrentPos = ourGoalSide + (GameParameters.OppGoalCenter - ourGoalSide).GetNormalizeToCopy(GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2);
                            //    }
                            //}
                            if (Math.Sqrt(maxInterval.interval.End - maxInterval.interval.Start) < ClosegapSize)
                            {
                                Position2D ourGoalSide = (Model.BallState.Location.Y > 0 ? GameParameters.OurGoalLeft : GameParameters.OurGoalRight) + ((Model.BallState.Location.Y > 0 ? GameParameters.OurGoalLeft : GameParameters.OurGoalRight) - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.06);
                                GoalieCurrentPos = ourGoalSide + (Position2D.Zero - ourGoalSide).GetNormalizeToCopy(0.09);//GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2);
                            }
                            double BallDistanceFromGoalLine = Model.BallState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalCenter.X, Model.BallState.Location.Y));
                            if (GoalieCurrentPos != GameParameters.OurGoalCenter)
                            {
                                Planner.Add(robotID, GoalieCurrentPos, (Model.BallState.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                    }
                    #endregion


                    //gotoperp
                    string str = "New rahmati";
                    DrawingObjects.AddObject(new StringDraw(str, GameParameters.OurGoalCenter), GameParameters.OurGoalCenter.X.ToString() + "545456");
                    if (gotoperp)
                    {
                        str = "gotoPerp";
                        DrawingObjects.AddObject(new StringDraw(str, GameParameters.OurGoalCenter.Extend(+0.1, 0)), GameParameters.OurGoalCenter.Extend(+0.1, 0).X.ToString() + "545456");
                    }
                    #endregion
                }
            }
            #endregion
            #region OpponentIsInPass
            if (CurrentState == (int)GoalieStates.OpponentInPassState)
            {
                spin = true;
                if (Model.Opponents.ContainsKey(kickerOpponent))
                {
                    Line intersect = new Line(Model.Opponents[kickerOpponent].Location, Model.Opponents[kickerOpponent].Location + Vector2D.FromAngleSize(Model.Opponents[kickerOpponent].Angle.Value * Math.PI / 180, 10));
                    Line goalLine = new Line(GameParameters.OurGoalLeft.Extend(-.1, 0), GameParameters.OurGoalRight.Extend(-.1, 0));
                    Position2D? intersectpos = goalLine.IntersectWithLine(intersect);
                    if (intersectpos.HasValue && intersectpos.Value.Y < .5 && intersectpos.Value.Y > -.5)
                        Planner.Add(robotID, intersectpos.Value, (Model.Opponents[kickerOpponent].Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
            }
            #endregion
            #region PreDive

            if (CurrentState == (int)GoalieStates.PreDive)
            {
                MyLeftGoal = MyLeftGoal.Extend(0, -.2);
                MyRightGoal = MyRightGoal.Extend(0, .2);
                spin = true;
                Position2D plannerTarget = new Position2D();
                Position2D target = defenceSate.Location;

                Position2D ourGoalLeftWithextend = MyLeftGoal.Extend(0, .04);
                Position2D ourGoalRightWithextend = MyRightGoal.Extend(0, -.04);

                Vector2D leftCornerCenterGoal = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                Vector2D rightCornerCentergoal = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;

                Vector2D targetGoalCenter = target - GameParameters.OurGoalCenter;

                double angleBetweenLeftAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(leftCornerCenterGoal, targetGoalCenter));
                angleBetweenLeftAndtarget = (angleBetweenLeftAndtarget < 180) ? angleBetweenLeftAndtarget : 360 - angleBetweenLeftAndtarget;

                double angleBetweenRightAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(rightCornerCentergoal, targetGoalCenter));
                angleBetweenRightAndtarget = (angleBetweenRightAndtarget < 180) ? angleBetweenRightAndtarget : 360 - angleBetweenRightAndtarget;

                bool Left = (angleBetweenLeftAndtarget < angleBetweenRightAndtarget) ? true : false;
                double mainAngle = (Left) ? angleBetweenLeftAndtarget : angleBetweenRightAndtarget;
                Position2D goalNearCorner = (Left) ? ourGoalLeftWithextend : ourGoalRightWithextend;
                if (justOnceFlag1)
                    dist = (.8 * mainAngle) / 90;
                Planner.ChangeDefaulteParams(robotID, false);
                Planner.SetParameter(robotID, 6, 8);
                Line targetCenterGoal = new Line(target, GameParameters.OurGoalCenter);
                Line prep = targetCenterGoal.PerpenducilarLineToPoint(Model.OurRobots[robotID].Location);
                Line targetNearCorner = new Line(goalNearCorner, target);
                Position2D? intersect2 = targetNearCorner.IntersectWithLine(prep);
                //if (intersect2.HasValue && justOnceFlag1)
                //{
                //    justOnceFlag1 = false;
                //    dist = intersect2.Value.DistanceFrom(goalNearCorner);
                //}
                //dist = Math.Min(dist, .8);
                //if (dist < 0.5)
                //{
                //    dist = /*0.8;*/Math.Max(dist, 0.8);
                //}
                DrawingObjects.AddObject(new StringDraw("dist: " + dist.ToString(), Color.Blue, Model.BallState.Location.Extend(0.1, -.75)));
                Vector2D LeftwithExtend = (ourGoalLeftWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);
                Vector2D RightWithExtend = (ourGoalRightWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);

                Position2D leftGoal = target + LeftwithExtend;
                Position2D rightGoal = target + RightWithExtend;

                Line MovementLine = new Line(leftGoal, rightGoal);
                DrawingObjects.AddObject(new Line(MovementLine.Head, MovementLine.Tail, new Pen(Brushes.Aqua, 0.02f)), MovementLine.Head.Y.ToString() + "53246546");
                int? oppBallOwner = Model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(Model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
                Vector2D RobotHeadVector = (GameParameters.OurGoalCenter - Model.BallState.Location);
                if (oppBallOwner.HasValue)
                {
                    Vector2D RobotHeadActual = Vector2D.FromAngleSize((Model.Opponents[oppBallOwner.Value].Angle.Value * Math.PI / 180), 20);
                    bool isBet = Vector2D.IsBetweenWithDirection(GameParameters.OurGoalLeft - Model.BallState.Location, GameParameters.OurGoalRight - Model.BallState.Location, RobotHeadActual);
                    if (isBet)
                    {
                        if (Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < distfromBallThresh)
                        {
                            RobotHeadVector = Vector2D.FromAngleSize((/*(Model.Opponents[oppBallOwner.Value].Angle.Value * Math.PI / 180) +*/(Model.BallState.Location - Model.Opponents[oppBallOwner.Value].Location).AngleInRadians), 10);
                        }
                        else
                        {
                            RobotHeadVector = Vector2D.FromAngleSize(((Model.Opponents[oppBallOwner.Value].Angle.Value * Math.PI / 180) + (Model.BallState.Location - Model.Opponents[oppBallOwner.Value].Location).AngleInRadians) / 2, 10);
                        }
                    }
                    else
                    {
                        double angleBetweenLeftAndtarget2 = Math.Abs(Vector2D.AngleBetweenInDegrees(RobotHeadActual, GameParameters.OurGoalLeft - Model.Opponents[oppBallOwner.Value].Location));
                        angleBetweenLeftAndtarget2 = (angleBetweenLeftAndtarget2 < 180) ? angleBetweenLeftAndtarget2 : 360 - angleBetweenLeftAndtarget2;

                        double angleBetweenRightAndtarget3 = Math.Abs(Vector2D.AngleBetweenInDegrees(RobotHeadActual, GameParameters.OurGoalRight - Model.Opponents[oppBallOwner.Value].Location));
                        angleBetweenRightAndtarget3 = (angleBetweenRightAndtarget3 < 180) ? angleBetweenRightAndtarget3 : 360 - angleBetweenRightAndtarget3;

                        if (angleBetweenLeftAndtarget2 < angleBetweenRightAndtarget3)
                        {
                            RobotHeadVector = GameParameters.OurGoalLeft.Extend(0, .05) - Model.BallState.Location;
                        }
                        else
                        {
                            RobotHeadVector = GameParameters.OurGoalRight.Extend(0, -.05) - Model.BallState.Location;
                        }
                    }
                }
                Line RobotHeadLine = new Line(Model.BallState.Location, Model.BallState.Location + RobotHeadVector);

                Position2D? intersection = RobotHeadLine.IntersectWithLine(MovementLine);

                if (intersection.HasValue)
                {
                    Position2D intersect = intersection.Value;
                    if (leftGoal.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter)
                    {
                        Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy(leftGoal.DistanceFrom(rightGoal) / 2);
                        Position2D robottarget = rightGoal + rightToLeft;

                        if (robottarget.DistanceFrom(goalNearCorner) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) - .02);
                            robottarget = rightGoal + rightToLeft;
                        }
                        plannerTarget = robottarget;
                    }
                    else
                    {
                        if (intersect.DistanceFrom(leftGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) - .02);
                            Position2D robottarget = leftGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (intersect.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) - .02);
                            Position2D robottarget = rightGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (!Vector2D.IsBetween(ourGoalLeftWithextend - target, ourGoalRightWithextend - target, intersect - target))
                        {
                            if (Vector2D.IsBetween(ourGoalLeftWithextend - target, GameParameters.OurLeftCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) - .02);
                                Position2D robottarget = leftGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else if (Vector2D.IsBetween(ourGoalRightWithextend - target, GameParameters.OurRightCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) - .02);
                                Position2D robottarget = rightGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else
                            {
                                plannerTarget = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.5);
                            }
                        }
                        else
                        {
                            plannerTarget = intersect;
                        }
                    }
                }
                else
                {
                    plannerTarget = GameParameters.OurGoalCenter - (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.4);
                }

                foreach (var item in ourGoalinterval)
                {
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                    {
                        maxdist = Math.Abs(item.interval.Start - item.interval.End);
                        maxInterval = item;
                        maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                    }
                }
                if (FreekickDefence.StaticCBID.HasValue)
                {
                    double tresh = 0.02;
                    Circle CBCircle = new Circle(Model.OurRobots[FreekickDefence.StaticCBID.Value].Location, GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2 /*+ tresh*/, new Pen(Brushes.Red, 0.02f));
                    //DrawingObjects.AddObject(new Circle(CBCircle.Center, CBCircle.Radious, new Pen(Brushes.Blue, 0.02f)), CBCircle.Center.X.ToString() + "56465");
                    Position2D pLeft = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End);
                    Position2D pRight = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);
                    Line TargetLine = new Line(plannerTarget, Model.BallState.Location);
                    Position2D posintersect = CBCircle.Intersect(TargetLine).FirstOrDefault();
                    //DrawingObjects.AddObject(new Circle(plannerTarget, .1, new Pen(Brushes.Blue, .01f)), "54564646646546");
                    //DrawingObjects.AddObject(new Circle(intersection.Value, .1, new Pen(Brushes.HotPink, .01f)), "56456464");
                    if (posintersect != Position2D.Zero)
                    {
                        //DrawingObjects.AddObject(new Circle(posintersect, 0.05, new Pen(Brushes.Red, 0.02f)), posintersect.X.ToString() + "65");
                        plannerTarget = GameParameters.OurGoalCenter + (posintersect - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.7);
                        Planner.Add(robotID, plannerTarget, (target - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                    }
                    else
                    {
                        Planner.Add(robotID, plannerTarget, (target - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                    }
                }
                else
                {
                    Planner.Add(robotID, plannerTarget, (target - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }

                //DrawingObjects.AddObject(new StringDraw("distance from GoalCenter: " + (Model.OurRobots[robotID].Location.DistanceFrom(GameParameters.OurGoalCenter)).ToString(), Color.Blue, Model.BallState.Location.Extend(0, -.75)));
            }
            #endregion
            #region Piston
            else if (CurrentState == (int)GoalieStates.Piston)
            {
                spin = true;
                DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;
                double dist = PistonDistance(defenceSate.Location);
                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-dist);

                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                postoGo = new Position2D(x, postoGo.Y);
                Planner.Add(robotID, postoGo, (defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - Model.OurRobots[robotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
            }
            #endregion


            //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[robotID].Location) < 0.5 && spin)
            //{
            //    //Planner.AddKick(robotID, true);
            //}
            //else
            //Planner.AddKick(robotID, false);
            return new SingleWirelessCommand();

        }


        double distfromBallThresh = .18;


        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int? id = StaticRB(engine, Model);
            SingleObjectState defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;

            DefenceInfo inf1 = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();
            DefenceInfo inf2 = null;
            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
            //    inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();

            if (inf1 != null && inf1.DefenderPosition.HasValue && inf2 != null && inf2.DefenderPosition.HasValue)
            {
                if (!eattheball(engine, Model, inf1, inf2, true))
                {
                    FreekickDefence.EaththeBall = false;
                    FreekickDefence.ReadyForEatStatic1 = false;
                    FreekickDefence.ReadyForEatStatic2 = false;
                    FreekickDefence.ReadyForEatStaticCB = false;
                }
            }

            if (CurrentState != (int)GoalieStates.PreDive)
            {
                justOnceFlag1 = true;
                //Planner.ChangeDefaulteParams(RobotID, true);
            }

            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);

            if (!GameParameters.IsInField(ballState.Location, 0.05))
                CurrentState = (int)GoalieStates.Normal;
            else
            {

                Vector2D ballSpeed = BallState.Speed;
                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - BallState.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v)) + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v) + maxVertical;
                double maxSpeedToGet = 0.2;
                double dist, dist2;
                double margin = FreekickDefence.AdditionalSafeRadi + RobotParameters.OurRobotParams.Diameter / 2;

                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

                double innerProduct = Vector2D.InnerProduct(BallState.Speed, (Model.OurRobots[RobotID].Location - BallState.Location));
                double difAngle = Vector2D.AngleBetweenInDegrees(BallState.Speed, (BallState.Location - Model.OurRobots[RobotID].Location));

                Circle c = new Circle(Model.OurRobots[RobotID].Location, 0.12);
                Line l = new Line(BallState.Location, BallState.Location + BallState.Speed);

                List<Position2D> inters = c.Intersect(l);

                double maxdist = 0;
                Line maxline = new Line();
                Planning.GamePlanner.Types.VisibleGoalInterval maxInterval = new Planning.GamePlanner.Types.VisibleGoalInterval();
                Position2D middlepos = new Position2D();
                Vector2D vec = new Vector2D();
                Line middlePosToBallLine = new Line();
                List<Planning.GamePlanner.Types.VisibleGoalInterval> ourGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();
                foreach (var item in ourGoalinterval)
                {
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                    {
                        maxdist = Math.Abs(item.interval.Start - item.interval.End);
                        maxInterval = item;
                        maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                    }
                }

                Position2D MyLeftGoal = /*GameParameters.OurGoalLeft*/new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End);
                Position2D MyRightGoal = /*GameParameters.OurGoalRight*/new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);

                #region Predive Configuration
                int? oppBallOwner = Model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(Model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
                bool ballisinfront = false;
                bool rahmatiMainState = false;
                if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value))
                {
                    int oppballownerID = oppBallOwner.Value;
                    Line robotleftLine = new Line(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11), (Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11)) + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180), 1));
                    Line robotRightLine = new Line(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11), (Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11)) + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180), 1));

                    if (robotleftLine.Distance(Model.BallState.Location) + robotRightLine.Distance(Model.BallState.Location) < .23)
                    {
                        ballisinfront = true;
                    }
                    else
                    {
                        ballisinfront = false;
                    }
                    if (FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value))
                    {
                        StaticDefenderCurrentPos = Model.OurRobots[FreekickDefence.StaticCBID.Value].Location;
                    }
                    if (inf1 != null && inf2 != null)
                    {
                        if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 || StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05)
                        {
                            if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 && StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.StaticCBID.Value/*FreekickDefence.Static1ID.Value*/).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05)
                            {
                                rahmatiMainState = true;
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value && t.Key != FreekickDefence.StaticCBID/*FreekickDefence.Static1ID.Value*/).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                        }
                    }
                }
                if (FreekickDefence.Static2ID == null || FreekickDefence.StaticCBID == null/*FreekickDefence.Static1ID == null*/)
                {
                    rahmatiMainState = true;
                }
                #endregion
                #region Predive
                if (CurrentState == (int)GoalieStates.PreDive)
                {
                    double r = 0.11;//GameDefinitions.RobotParameters.OurRobotParams.Diameter/2);
                    Position2D CBcirclecenter = FreekickDefence.CenterCurrentPosition;
                    Circle CBcenter = new Circle(CBcirclecenter, r);
                    DrawingObjects.AddObject(new Circle(CBcirclecenter, r, new Pen(Brushes.Red, 0.02f)), CBcirclecenter.Y.ToString() + "578302");

                    if ((FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value) && !CBcenter.IsInCircle(Model.OurRobots[FreekickDefence.StaticCBID.Value].Location))
                        || !(FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value)) || Math.Sqrt(maxInterval.interval.End - maxInterval.interval.Start) > OpengapSize)
                    {
                        counter1++;
                        Line ballspeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                        Line golline = new Line(MyLeftGoal, MyRightGoal);
                        if (oppBallOwner.HasValue && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) / Model.BallState.Speed.Size < 1.3 && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18 && golline.IntersectWithLine(ballspeedLine).HasValue && golline.IntersectWithLine(ballspeedLine).Value.Y < MyLeftGoal.Y && golline.IntersectWithLine(ballspeedLine).Value.Y > MyRightGoal.Y)// 1.3 zaribe khoobe az fasele va sorAt baraye raftan be dive
                        {
                            CurrentState = (int)GoalieStates.KickToGoal;
                        }
                        else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && (Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18 && golline.IntersectWithLine(ballspeedLine).HasValue && (golline.IntersectWithLine(ballspeedLine).Value.Y > MyLeftGoal.Y || golline.IntersectWithLine(ballspeedLine).Value.Y < MyRightGoal.Y)) || !rahmatiMainState)
                        {
                            CurrentState = (int)GoalieStates.Normal;
                        }
                        else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18 && (Model.BallState.Speed.Size < .05 || counter1 > 10))// goftim counter bishtar az 10 bezarim ta khialemoon rahat she ke too pre dive nemimoonim toopp biad az kenaremoon rad she
                        {
                            CurrentState = (int)GoalieStates.Normal;
                        }
                        else if (!oppBallOwner.HasValue || !Model.Opponents.ContainsKey(oppBallOwner.Value))
                        {
                            CurrentState = (int)GoalieStates.Normal;
                        }
                    }
                    else
                        CurrentState = (int)GoalieStates.Normal;


                    Line l1 = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                    Position2D? p = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                    if (p.HasValue && p.Value != Position2D.Zero)
                    {
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) < 0.25)
                        {
                            CurrentState = (int)GoalieStates.Rahmati;
                        }
                    }
                }
                #endregion
                #region Normal
                else if (CurrentState == (int)GoalieStates.Normal)
                {
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (!(FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value)) && oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.BallState.Location.X > (Model.Opponents[oppBallOwner.Value].Location.X + 0.05) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < distfromBallThresh && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 && ballisinfront && rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.PreDive;
                    }
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (eattheball(engine, Model, inf1, inf2, false))
                    {
                        FreekickDefence.EaththeBall = true;
                        FreekickDefence.StaticCenterState = CenterDefenderStates.EatTheBall;
                        //FreekickDefence.StaticFirstState = DefenderStates.EatTheBall;
                        //FreekickDefence.StaticSecondState = DefenderStates.EatTheBall;
                        CurrentState = (int)GoalieStates.EathTheBall;
                    }
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                        CurrentState = (int)GoalieStates.Piston;
                    else if ((GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet)) || ((GameParameters.IsInDangerousZone(ballState.Location, false, -.1, out dist, out dist2))))//IO 2015
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (inf1 != null && inf2 != null || inf1.DefenderPosition.HasValue || inf2.DefenderPosition.HasValue || FreekickDefence.StaticCBID == null/*FreekickDefence.Static1ID == null*/ || FreekickDefence.Static2ID == null) // TODO RAHMATI BACK
                    {
                        Position2D goalieTarget = (defenceSate.Location - GoalieTargetPos).PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location);
                        if ((/*StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .10 || */StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .10))
                            CurrentState = (int)GoalieStates.Rahmati;

                    }
                }
                #endregion
                #region piston
                else if (CurrentState == (int)GoalieStates.Piston)
                {
                    Position2D goalieTarget = (defenceSate.Location - GoalieTargetPos).PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location);
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;

                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!(FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value)) && oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.BallState.Location.X > (Model.Opponents[oppBallOwner.Value].Location.X + 0.05) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < distfromBallThresh && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 && ballisinfront && rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.PreDive;
                    }
                    else if ((GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet)) || ((GameParameters.IsInDangerousZone(ballState.Location, false, -.1, out dist, out dist2))))//IO 2015
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if ((inf1.DefenderPosition.HasValue || inf2.DefenderPosition.HasValue))
                    {
                        if (/*StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .10 || */StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .10 || FreekickDefence.StaticCBID == null /*FreekickDefence.Static1ID == null*/ /*|| FreekickDefence.Static2ID == null*/)//Rahmati Back
                            CurrentState = (int)GoalieStates.Rahmati;
                        else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X > 2.7)
                            CurrentState = (int)GoalieStates.Normal;
                    }
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X > 2.7)
                        CurrentState = (int)GoalieStates.Normal;



                }
                #endregion
                #region InPenaltyArea
                else if (CurrentState == (int)GoalieStates.InPenaltyArea)
                {
                    margin = 0.2;
                    if (BallKickedToOurGoal(Model) &&
                        (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2)
                        || acceptableballRobotSpeed * 1.2 < ballSpeed.Size))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                        CurrentState = (int)GoalieStates.Piston;
                    else if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin + .05, out dist, out dist2))//|| acceptable2 * 1.2 < ballSpeed.Size)//IO2015
                        CurrentState = (int)GoalieStates.Normal;
                }
                #endregion
                #region KickToGoal
                else if (CurrentState == (int)GoalieStates.KickToGoal)
                {
                    margin = 0.1;
                    if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!BallKickedToOurGoal(Model) && GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && !BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.Piston;
                    else
                        if (!BallKickedToOurGoal(Model))
                            CurrentState = (int)GoalieStates.Normal;
                }
                #endregion
                #region KickToRobot
                else if (CurrentState == (int)GoalieStates.KickToRobot)
                {
                    if (ballSpeed.Size < 1.5 || BallKickedToOurGoal(Model) || inters.Count == 0 || innerProduct < -0.1)
                    {
                        if (BallKickedToOurGoal(Model))
                            CurrentState = (int)GoalieStates.KickToGoal;
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 < ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))//IranOpen2015
                            CurrentState = (int)GoalieStates.InPenaltyArea;
                        else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                            CurrentState = (int)GoalieStates.Piston;
                        else
                            CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion
                #region Rahmati
                else if (CurrentState == (int)GoalieStates.Rahmati)
                {
                    maxdist = 0;
                    maxline = new Line();
                    maxInterval = new Planning.GamePlanner.Types.VisibleGoalInterval();
                    middlepos = new Position2D();
                    vec = new Vector2D();
                    middlePosToBallLine = new Line();
                    List<Planning.GamePlanner.Types.VisibleGoalInterval> oppGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();
                    CalculateGoalIntervals(Model, out ourGoalinterval, out oppGoalinterval, false, true, new List<int>(), new List<int>() { RobotID });
                    foreach (var item in ourGoalinterval)
                    {
                        if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                        {
                            maxdist = Math.Abs(item.interval.Start - item.interval.End);
                            maxInterval = item;
                            maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                        }
                    }

                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) &&/* Model.BallState.Location.X > (Model.Opponents[oppBallOwner.Value].Location.X + 0.05) &&*/
                             Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < .18 && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 &&
                             ballisinfront && rahmatiMainState)
                    {
                        double dis = Math.Sqrt(maxInterval.interval.End - maxInterval.interval.Start);
                        if (Math.Sqrt(maxInterval.interval.End - maxInterval.interval.Start) > 0.9 && !useNewRahmati)
                        {
                            CurrentState = (int)GoalieStates.PreDive;
                        }
                    }
                    else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))//IranOpen2015
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (/*inf2.DefenderPosition.HasValue && */inf1.DefenderPosition.HasValue && FreekickDefence.Static2ID.HasValue && FreekickDefence.StaticCBID.HasValue /*FreekickDefence.Static1ID.HasValue*/)// && new Line(ballState.Location, ballState.Location + ballState.Speed).IntersectWithLine(new Line(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location), ref IntersectPoint))
                    {
                        if ((/*StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .07 && */StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .07))// && IntersectPoint.DistanceFrom(GameParameters.OurGoalCenter) > Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                            CurrentState = (int)GoalieStates.Normal;

                    }
                    Line l1 = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                    Position2D? p = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                    if (p.HasValue && p.Value != Position2D.Zero)
                    {
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) < 0.25)
                        {
                            CurrentState = (int)GoalieStates.Rahmati;
                        }
                    }
                }
                #endregion
                #region Eat The Ball
                else if (CurrentState == (int)GoalieStates.EathTheBall)
                {
                    if (!eattheball(engine, Model, inf1, inf2, true))
                    {
                        FreekickDefence.EaththeBall = false;
                        FreekickDefence.StaticCenterState = CenterDefenderStates.Normal;
                        //FreekickDefence.StaticFirstState = DefenderStates.Normal;
                        //FreekickDefence.StaticSecondState = DefenderStates.Normal;
                        CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion
            }
            FreekickDefence.CurrentStates[this] = CurrentState;

            int? nearestOpponent = null;
            if (Model.Opponents.Count > 0)
                nearestOpponent = Model.Opponents.OrderBy(r => r.Value.Location.DistanceFrom(Model.BallState.Location)).Select(r => r.Key).First();
            if (nearestOpponent.HasValue && Model.Opponents.ContainsKey(nearestOpponent.Value))
            {
                robotAngle.Enqueue(Model.Opponents[nearestOpponent.Value].Angle.Value);
                robotDistance.Enqueue(Model.Opponents[nearestOpponent.Value].Location.DistanceFrom(Model.BallState.Location));
            }
            ballstates.Enqueue(Model.BallState.Location);
            if (robotAngle.Count > 100)
            {
                robotAngle.Dequeue();
                robotDistance.Dequeue();
                ballstates.Dequeue();
            }
            int kickFrame = 0;
            for (int i = 0; i < robotDistance.Count; i++)
            {
                if (robotDistance.ElementAt(i) < .12)
                {
                    kickFrame = i;
                }
            }
            if (ballstates.Count > kickFrame && robotAngle.Count > kickFrame)
            {
                if (Model.BallState.Speed.Size > 4 && Vector2D.IsBetween(GameParameters.OppGoalLeft - ballstates.ElementAt(kickFrame), GameParameters.OppGoalRight - ballstates.ElementAt(kickFrame), Vector2D.FromAngleSize(robotAngle.ElementAt(kickFrame) * Math.PI / 180, 1)))
                {
                    CurrentState = (int)GoalieStates.KickToGoal;
                }
            }

            DrawingObjects.AddObject(new StringDraw(((GoalieStates)CurrentState).ToString(), GameParameters.OurGoalCenter.Extend(0.3, 0)), "gstate");
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        double PistonDistance(Position2D target)
        {
            Position2D startPiston = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.OurGoalCenter.X - 1.77);
            double dist = target.DistanceFrom(GameParameters.OurGoalCenter);
            return Math.Max(Math.Min((0.2899 * dist) - 0.2594, .9), .4);



        }

        public bool BallKickedToOurGoal(WorldModel Model)
        {

            double tresh = 0.20;
            double tresh2 = 1.3;
            if ((GoalieStates)CurrentState == GoalieStates.KickToGoal)
            {
                tresh = 0.23;
                tresh2 = 1.4;
            }
            Line line = new Line();
            line = new Line(BallState.Location, BallState.Location - BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            DrawingObjects.AddObject(new StringDraw((d / BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
                    if (BallState.Speed.Size > 0.1 && d / BallState.Speed.Size < tresh2)
                        return true;
            return false;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 20 * RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new NewStaticGoalieRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public BallDetails GetBallIntersectWithArea(WorldModel model)
        {
            Circle c = new Circle(GameParameters.OurGoalCenter, 0.95);
            Line l = new Line(model.BallState.Location, ballState.Location + ballState.Speed);

            List<Position2D> intersect = c.Intersect(l);
            double min = double.MaxValue;
            Position2D pos = new Position2D();
            if (intersect.Count > 0)
            {
                min = intersect.Min(m => m.DistanceFrom(model.BallState.Location));
                pos = intersect.First(f => f.DistanceFrom(model.BallState.Location) == min);
            }
            Line l2 = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            Position2D? intersect2 = l.IntersectWithLine(l2);
            BallDetails bd = null;
            if (intersect.Count > 0 && intersect2.HasValue)
            {
                bd = new BallDetails();
                if (intersect.Count == 1)
                {
                    bd.EntryPoint = intersect[0];
                    bd.ExitPoint = intersect[0];
                }
                else if (Vector2D.InnerProduct(model.BallState.Speed, (intersect[0] - ballState.Location)) <
                    Vector2D.InnerProduct(model.BallState.Speed, (intersect[1] - ballState.Location)))
                {
                    bd.EntryPoint = intersect[0];
                    bd.ExitPoint = intersect[1];
                }
                else
                {
                    bd.EntryPoint = intersect[1];
                    bd.ExitPoint = intersect[0];
                }

                bd.Location = ballState.Location;
                bd.OurGoalIntersect = intersect2.Value;
                bd.Speed = ballState.Speed;
            }
            return bd;
        }

        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            //double minopp = Model.Opponents.Min ( m => m.Value.Location.DistanceFrom ( ballState.Location ) );
            //double ourDist = Model.OurRobots [robotID].Location.DistanceFrom ( ballState.Location );
            //if ( minopp > 0.5 && ourDist < 0.2 )
            //    return GameParameters.OppGoalCenter;
            //return ballState.Location + ( ballState.Location - GameParameters.OurGoalCenter ).GetNormalizeToCopy ( 2 );
            Vector2D v = ballState.Location - GameParameters.OurGoalCenter;
            v = Vector2D.FromAngleSize(Math.Sign(v.AngleInRadians) * Math.Max(Math.Abs(v.AngleInRadians), (110.0).ToRadian()), v.Size);
            return ballState.Location + v.GetNormalizeToCopy(2);
            //return ballState.Location + ( -1 * BallState.Speed ).GetNormalizeToCopy ( 2 );
        }

        private bool InconmmingOutgoing(WorldModel Model, int RobotID, ref bool isNear)
        {
            Position2D temprobot = Model.Opponents[RobotID].Location + Model.Opponents[RobotID].Speed * 0.04;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + BallState.Speed * 0.04;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            if (BallState.Speed.Size > 2)
            {
                double coef = 1;
                if (LastRB == RBstate.Robot)
                    coef = 1.2;

                double ballspeedAngle = BallState.Speed.AngleInDegrees;
                double robotballInner = Model.Opponents[RobotID].Speed.InnerProduct((ballState.Location - Model.Opponents[RobotID].Location).GetNormnalizedCopy());
                bool ballinGoal = false;
                Line line = new Line();
                line = new Line(BallState.Location, BallState.Location - BallState.Speed);
                Position2D BallGoal = new Position2D();
                BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
                double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
                if (BallGoal.Y < GameParameters.OurGoalLeft.Y + .65 / coef && BallGoal.Y > GameParameters.OurGoalRight.Y - .65 / coef)
                    if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
                        ballinGoal = true;

                if (ballState.Speed.InnerProduct((temprobot - tempball).GetNormnalizedCopy()) > 1.2 / coef
                    && robotballInner < 2 * coef && robotballInner > -1
                    && !ballinGoal)
                    return true;

            }
            return false;
        }

        private int? StaticRB(GameStrategyEngine engine, WorldModel Model)
        {
            if (CurrentState == (int)GoalieStates.InPenaltyArea)
                return null;

            Position2D? g;
            var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
            double d1, d2;
            //if ( opps.Count > 0 && GameParameters.IsInDangerousZone ( ballState.Location , false , 0.2 , out d1 , out d2 ) )
            //{
            //    if ( !GameParameters.IsInDangerousZone ( Model.Opponents [opps.First ()].Location , false , 0.03 , out d1 , out d2 ) )
            //    {
            //        LastRB = RBstate.Robot;
            //        return opps.First ();

            //    }
            //    else if ( opps.Count > 1 )
            //    {
            //        LastRB = RBstate.Robot;
            //        return opps.Skip ( 1 ).First ();
            //    }
            //    else
            //        return null;
            //}
            //else if ( opps.Count > 0 && GameParameters.IsInDangerousZone ( Model.Opponents [opps.First ()].Location , false , 0.03 , out d1 , out d2 ) )
            //{
            //    return null;
            //}
            if (opps.Count == 0 || BallState.Speed.Size < 1)
                return null;

            SingleObjectState oppstate = Model.Opponents[opps.First()];


            Position2D temprobot = oppstate.Location + oppstate.Speed * 0.2;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballState.Speed * 0.2;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            SingleObjectState ball = ballState;
            Vector2D ballRobot = temprobot - tempball;
            Vector2D robotTarget = GameParameters.OurGoalCenter - temprobot;
            double ballAngle = Vector2D.AngleBetweenInDegrees(ballRobot, robotTarget);
            bool incomningNear = false;
            if (InconmmingOutgoing(Model, opps.First(), ref incomningNear))
            {
                LastRB = RBstate.Robot;
                return opps.First();
            }
            return null;
        }

        private bool eattheball(GameStrategyEngine engine, WorldModel model, DefenceInfo inf1, DefenceInfo inf2, bool fornormal)
        {
            int? id = StaticRB(engine, model);
            SingleObjectState defenceSate = (id.HasValue) ? model.Opponents[id.Value] : ballState;

            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();

            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
            //    inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();


            Defender1Delayed = false;

            if (StaticDefender1IDG.HasValue && inf1 != null && inf1.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender1IDG.Value))
            {
                if (model.OurRobots[StaticDefender1IDG.Value].Location.DistanceFrom(inf1.DefenderPosition.Value) > .02)
                {
                    Defender1Delayed = true;
                }
            }
            Defender2Delayed = false;
            if (StaticDefender2IDG.HasValue && inf2 != null && inf2.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender2IDG.Value))
            {
                if (/*model.OurRobots[StaticDefender2IDG.Value].Location.DistanceFrom(inf2.DefenderPosition.Value) > .02*/false)
                {
                    Defender2Delayed = true;
                }
            }
            if (StaticDefender1IDG != null && StaticDefender2IDG != null && StaticDefender1IDG.HasValue && StaticDefender2IDG.HasValue && model.OurRobots.ContainsKey(StaticDefender1IDG.Value) && model.OurRobots.ContainsKey(StaticDefender2IDG.Value))
            {
                double histersis = (fornormal) ? .02 : 0;
                Line leftRight = new Line(model.OurRobots[StaticDefender1IDG.Value].Location, model.OurRobots[StaticDefender2IDG.Value].Location);
                if (leftRight.Tail.DistanceFrom(leftRight.Head) < RobotParameters.OurRobotParams.Diameter + .04)
                {
                    Vector2D leftrightvector = leftRight.Tail - leftRight.Head;

                    Position2D middle = new Position2D((leftRight.Head.X + leftRight.Tail.X) / 2, (leftRight.Head.Y + leftRight.Tail.Y) / 2);

                    Position2D preppos = leftrightvector.PrependecularPoint(model.OurRobots[StaticDefender1IDG.Value].Location, model.BallState.Location);


                    double distballfromline = preppos.DistanceFrom(model.BallState.Location);

                    if (distballfromline < .09 && model.BallState.Location.DistanceFrom(middle) < .13)
                    {
                        if (model.BallState.Speed.Size < .2)
                            return true;
                    }
                }
            }
            return false;
        }

        // time from current pos
        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = Model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - Model.OurRobots[RobotID].Location.DistanceFrom(initialstate));
            double tc = (Model.OurRobots[RobotID].Location.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }
        // total time of path
        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos, bool inittarget)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = initialpos;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - currentPos.DistanceFrom(initialstate));
            double tc = (currentPos.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }
        // time of arriving to intersect when we want to stop in the intersect pos
        double predicttime(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            Position2D currentPos = model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * init.DistanceFrom(target));
            double accelDx = Math.Min(0.942, .46 * init.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * accelDx);
            double adeccel = 3.04;
            double aaccel = 3.14;

            double deltaXIntersectTarget = intersect.DistanceFrom(target);

            double coeff1 = deltaXIntersectTarget / deccelDX;
            double v0deccel = vmax * coeff1;
            double tTemp = v0deccel / adeccel;
            double tdeccel = predicttime(model, RobotID, init, target, true) - tTemp;

            double deltaxInitialIntersect = init.DistanceFrom(intersect);
            double coeff2 = deltaxInitialIntersect / accelDx;

            double V0accel = coeff2 * vmax;
            double taccel = V0accel / aaccel;

            double tcruise = ((deltaxInitialIntersect - accelDx) / vmax) + (vmax / accelDx);

            double deltaXInitialTarget = init.DistanceFrom(target);
            double ttotal = 0;
            if (deltaXIntersectTarget < accelDx + deccelDX) // Accel - Deccel
            {
                if (deltaXIntersectTarget > deccelDX)
                {
                    ttotal = taccel;
                }
                else
                {
                    ttotal = tdeccel;
                }
            }
            else // Accel - Cruise - Deccel
            {
                if (deltaxInitialIntersect < accelDx)
                {
                    ttotal = taccel;
                }
                else if (deltaXIntersectTarget < deccelDX)
                {
                    ttotal = tdeccel;
                }
                else
                {
                    ttotal = tcruise;
                }
            }
            return ttotal;
        }
        // time of arriving to intersect when we dont to stop in the intersect pos
        double timeRobotToTargetInIntersect(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            double timeInittarget = predicttime(model, RobotID, init, target, true);
            double timeInitIntersect = predicttime(model, RobotID, init, target, intersect);
            double timeIntersecttarget = timeInittarget - timeInitIntersect;
            double timeRobotTarget = predicttime(model, RobotID, init, target);
            double timeRobotIntesect = timeRobotTarget - timeIntersecttarget;
            return timeRobotIntesect;
        }

        private Position2D IntersectFind(WorldModel model, int RobotID, Position2D initpoint, Position2D target)
        {
            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

            double x4 = target.X;

            double x3 = initpoint.X;
            double y4 = target.Y;
            double y3 = initpoint.Y;
            double x2 = ballspeedpos.X;
            double y2 = ballspeedpos.Y;
            double x1 = model.BallState.Location.X;
            double y1 = model.BallState.Location.Y;
            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
            Position2D intersect = new Position2D();
            if (first.IntersectWithLine(second).HasValue)
                intersect = first.IntersectWithLine(second).Value;
            else
            {
                intersect = new Position2D(100, 100);
            }
            return intersect;
        }

        static double root(double a, double initialV, double deltaX)
        {
            double t = 0;
            double delta = (initialV * initialV) - (2 * a * -deltaX);
            if (delta == 0)
            {
                t = -initialV / (.5 * a);
            }
            if (delta > 0)
            {

                double t1 = (-initialV - Math.Sqrt(delta)) / a;
                double t2 = (-initialV + Math.Sqrt(delta)) / a;
                if (t2 > 0 && t1 < 0)
                    t = t2;
                else if (t1 > 0 && t2 < 0)
                    t = t1;
                else if (t1 > 0 && t2 > 0)
                    if (t1 < t2)
                        t = t1;
                    else
                        t = t2;
            }
            if (delta < 0)
                return -1000;
            return t;
        }

        public enum GoalieStates
        {
            Normal = 0,
            InPenaltyArea = 1,
            KickToGoal = 2,
            KickToRobot = 3,
            Rahmati = 4,
            Piston = 5,
            EathTheBall = 6,
            OpponentInPassState = 7,
            PreDive = 8
        }

        public class BallDetails
        {
            public Vector2D Speed = new Vector2D();
            public Position2D? OurGoalIntersect = new Position2D();
            public Position2D EntryPoint = new Position2D();
            public Position2D ExitPoint = new Position2D();
            public Position2D Location = new Position2D();
        }
#else
        public override RoleCategory QueryCategory()
        {
            throw new NotImplementedException();
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
#endif
    }

}
