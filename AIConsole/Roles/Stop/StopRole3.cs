﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Roles
{
    class StopRole3 : RoleBase
    {
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        public SingleWirelessCommand RunRole(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 2.5, true);
        }
        public SingleWirelessCommand RunRoleStop(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            if (Model.Status == GameStatus.BallPlace_Opponent)
            {
                ballState = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
                ballStateFast = new SingleObjectState(StaticVariables.ballPlacementPos, new Vector2D(), 0);
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 2);
            return GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GetTarget(Model, RobotID), (ballState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, true, 1.9, false);
        }


        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        private Position2D GetTarget(WorldModel Model, int RobotID)
        {
            //			return ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.35, 0.6);
            Position2D target = new Position2D();

            double StopDistFromBall = .65;



            Position2D ball = ballState.Location;
            //////if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.3)
            //////{
            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 1 * Math.Sign(ball.Y) * 1.6 * Math.PI / 3, .6);
            //////    if (ballState.Location.X > 2.4)
            //////    {
            //////        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1 * Math.Sign(ball.Y) * 1.6 * Math.PI / 3, .6);
            //////    }
            //////}
            //////else if (ballState.Location.X > 2.4)
            //////{

            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1 * Math.Sign(ball.Y) * 1.4 * Math.PI / 3, .6);
            //////    target = new Position2D(1.5, .5);
            //////}
            //////else if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.3)
            //////{
            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1 * Math.Sign(ball.Y) * 1.5 * Math.PI / 3, .6);
            //////}
            //////else if (Math.Abs(ball.Y) > 1.5 && ball.X > 2.45)//Our Corner
            //////{
            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * Math.PI / 6, 3);
            //////}
            //////else if (Math.Abs(ball.Y) > 1.5 && ball.X < 2.45 && ball.X > -2.45)//side
            //////{
            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + Math.Sign(ball.Y) * -1 * 0.6, 2.5);
            //////}
            //////else if (Math.Abs(ball.Y) > 1.5 && ball.X < -2.45)//opp corner
            //////{
            //////    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + Math.Sign(ball.Y) * -1, 2.5);
            //////}


            //////else//other
            //////{
            //////    if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < GameParameters.SafeRadi(ballState, 1.4)) // reverse
            //////    {
            //////        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1.3, 0.6);
            //////    }
            //////    else
            //////    {
            //////        target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 0.33, 0.6);
            //////    }

            //////}
            //////if (target.DistanceFrom(GameParameters.OppGoalCenter) < 1.4)
            //////    target = GameParameters.OppGoalCenter + (target - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.4);
            if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 1.9)
            {
                target = new Position2D(2.08, -1);
            }
            else
            {
                if (ball.X > GameParameters.OurGoalCenter.X - .5 && Math.Abs(ball.Y) > 2.5)//corner
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + -1 * Math.Sign(ball.Y) * Math.PI / 6.5, 4.2);
                }
                else if (ball.X > 1.5 && Math.Abs(ball.Y) > 2.5)
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + (Math.Sign(ball.Y) * -.75), 4.2);
                }
                else if (Math.Abs(ball.X) <= 1.5 && Math.Abs(ball.Y) > 2.5)
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + (Math.Sign(ball.Y) * -.5), 4.2);
                }

                else if (ball.X < -1.5 && Math.Abs(ball.Y) > 2.5)
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - .38, StopDistFromBall);
                }
                else if (Math.Abs(ball.Y) <= 1.15 && ball.X < 1.5)
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - .38, StopDistFromBall);
                }
                else if (Math.Abs(ball.Y) <= 1.15 && ball.X < 1.5 && ball.X >= 1.5)
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - .7, StopDistFromBall);

                }
                else
                {
                    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 1, StopDistFromBall);
                }
            }
            //if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 1.3)
            //{
            //    if (Math.Abs(ballState.Location.X) - Math.Abs(GameParameters.OurLeftCorner.X) < 0.15)
            //    {
            //        //if (ballState.Location.DistanceFrom(GameParameters.OurLeftCorner) < 0.3)
            //        //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 0.7, 0.6);
            //        //else if (ballState.Location.DistanceFrom(GameParameters.OurRightCorner) < 0.3)
            //        //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.7, 0.6);
            //        //else
            //        //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.35, 0.6);
            //        if (ballState.Location.DistanceFrom(GameParameters.OurLeftCorner) < 0.3)
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians - 0.15, 2.64);
            //        else if (ballState.Location.DistanceFrom(GameParameters.OurRightCorner) < 0.3)
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.15, 2.64);
            //        else if (ballState.Location.DistanceFrom(GameParameters.OppLeftCorner) < 0.3)
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OppGoalCenter - ballState.Location).AngleInRadians + 0.15, 2.85);
            //        else if (ballState.Location.DistanceFrom(GameParameters.OppRightCorner) < 0.3)
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OppGoalCenter - ballState.Location).AngleInRadians - 0.15, 2.85);
            //        else if (Math.Abs(ballState.Location.Y) > 1.6 && ballState.Location.X >= 0)
            //        {
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.35, 0.6);
            //            target.Y *= -1;

            //            target.Y += (ballState.Location.Y >= 0) ? 1 : -1;
            //            if (Math.Abs(target.X) > Math.Abs(GameParameters.OurLeftCorner.X) - 0.8)
            //                target.X = Math.Sign(target.X) * (Math.Abs(GameParameters.OurLeftCorner.X) - 0.8);
            //        }
            //        else if (Math.Abs(ballState.Location.Y) > 1.6 && ballState.Location.X < 0)
            //        {
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.35, 0.6);
            //            target.Y *= -1;
            //            target.Y += (ballState.Location.Y >= 0) ? 0.5 : -0.5;
            //            if (Math.Abs(target.X) > Math.Abs(GameParameters.OurLeftCorner.X) - 0.8)
            //                target.X = Math.Sign(target.X) * (Math.Abs(GameParameters.OurLeftCorner.X) - 0.8);
            //        }
            //        else
            //            target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - ballState.Location).AngleInRadians + 0.35, 0.6);


            //    }
            //    DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f)));
            //    VisualizerConsole.WriteLine(target.ToString());
            //target = OverlapSolving(Model, RobotID, target);
            return target;
            //}
            //if ((ballState.Location.Y <= -0.175 && ballState.Location.Y >= -0.676 && ballState.Location.X < GameParameters.OurGoalCenter.X && ballState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalRight.X, -0.175)) <= GameParameters.DefenceareaRadii) || (ballState.Location.Y >= 0.175 && ballState.Location.Y <= 0.676 && ballState.Location.X < GameParameters.OurGoalCenter.X && ballState.Location.DistanceFrom(new Position2D(GameParameters.OurGoalLeft.X, 0.175)) <= GameParameters.DefenceareaRadii) || (ballState.Location.Y <= 0.175 && ballState.Location.Y >= -0.175 && (GameParameters.OurGoalCenter.X - ballState.Location.X) <= 0.5 && (GameParameters.OurGoalCenter.X - ballState.Location.X) >= 0))
            //{
            //    if (Math.Abs(ballState.Location.Y) < 0.5)
            //        target = new Position2D(ballState.Location.X, 0.5 * Math.Sign(ballState.Location.Y)) + Vector2D.FromAngleSize((new Position2D(ballState.Location.X, 0.5 * Math.Sign(ballState.Location.Y)) - GameParameters.OurGoalCenter).AngleInRadians + 0.35, 0.6);
            //    else
            //        target = ballState.Location + Vector2D.FromAngleSize((ballState.Location - GameParameters.OurGoalCenter).AngleInRadians + 0.35, 0.6);
            //    DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f)), "tar3");
            //    return target;
            //}
            //if (ballState.Location.Y >= 0.76)
            //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalLeft - ballState.Location).AngleInRadians - 1.65, 0.6);
            //else if (ballState.Location.Y < 0.76 && ballState.Location.Y > 0)
            //    target = ballState.Location + Vector2D.FromAngleSize((new Position2D(GameParameters.OurLeftCorner.X, .076) - ballState.Location).AngleInRadians - 2.1, 0.6);
            //else if (ballState.Location.Y < 0 && ballState.Location.Y > -.76)
            //    target = ballState.Location + Vector2D.FromAngleSize((new Position2D(GameParameters.OurRightCorner.X, .076) - ballState.Location).AngleInRadians + 1.75, 0.6);
            //else
            //    target = ballState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalRight - ballState.Location).AngleInRadians + 1.85, 0.6);

            //DrawingObjects.AddObject(new Circle(target, 0.02, new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f)), "tar3");
            //return target;
        }
        Position2D OverlapSolving(WorldModel Model, int RobotID, Position2D Target)
        {
            Dictionary<int, SingleObjectState> ourRobots = Model.OurRobots.Where(u => u.Key != RobotID).ToDictionary(t => t.Key, y => y.Value);
            if (Model.GoalieID.HasValue)
            {
                ourRobots = Model.OurRobots.Where(t => t.Key != Model.GoalieID.Value && t.Key != RobotID).ToDictionary(y => y.Key, h => h.Value);
            }
            Position2D extendpoint = Target;
            foreach (var item in ourRobots)
            {
                if (item.Value.Location.DistanceFrom(Target) < .22)
                {
                    Vector2D OGCandRobot = GameParameters.OurGoalCenter - item.Value.Location;
                    Vector2D extendtempvec = Target - item.Value.Location;
                    extendpoint = item.Value.Location + extendtempvec.GetNormalizeToCopy(.22);
                    if (OGCandRobot.InnerProduct(extendtempvec) > 0)
                        extendpoint = Target - extendtempvec;
                    if (extendpoint.DistanceFrom(GameParameters.OppGoalCenter) < 1.1)
                        extendpoint = GameParameters.OppGoalCenter + (extendpoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.1);
                    DrawingObjects.AddObject(extendpoint);
                    Vector2D ExtendPoint2 = extendpoint - ballState.Location;
                    ExtendPoint2.NormalizeTo(.6);
                    extendpoint = ballState.Location + ExtendPoint2;
                }
            }

            return extendpoint;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GetTarget(Model, RobotID).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new StopRole3(), new ActiveRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
