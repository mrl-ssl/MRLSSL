﻿using MRL.SSL.AIConsole.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.GamePlanner.Types;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class FreeKickAttackerRole : RoleBase
    {

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(calculateTarget(engine,Model,RobotID));
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            ;
        }
        public void Perform(GameStrategyEngine engine, WorldModel model, int robotID)
        {
            Position2D targetToGo = calculateTarget(engine, model, robotID);
            Planner.Add(robotID,targetToGo,( model.BallState.Location - targetToGo).AngleInDegrees,PathType.UnSafe,true,true,true,true,false);
        }
        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        public Position2D calculateTarget(GameStrategyEngine engine, WorldModel Model,int RobotID)
        {
            Position2D target = new Position2D();
            List<Position2D> ballHistory = new List<Position2D>();

            #region Attack
            if (ballHistory.Count > 1 && ballHistory.First().DistanceFrom(ballHistory.Last()) > 1)
                ballHistory.Clear();
            if (ballHistory.Count == 0)
            {

                List<PassPointData> poses = new List<PassPointData>();
                double regionX = 0;
                //if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H / 2)
                //{
                //    regionX = Model.BallState.Location.X;
                //}
                //else if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H)
                //{
                regionX = -.5;
                //}
                Position2D topLeft = new Position2D(regionX, GameParameters.OurRightCorner.Y);
                double passSpeed = 4, shootSpeed = Program.MaxKickSpeed;
                int Rows =10, column = 15;

                int sgn;
                sgn = Math.Sign(NormalSharedState.CommonInfo.PassTarget.Y);


                topLeft = new Position2D(regionX, sgn < 0 ? 0 : GameParameters.OurRightCorner.Y);
                double width = (GameParameters.OurGoalCenter.X - 0.5 - 0.25);
                double heigth = GameParameters.OurLeftCorner.Y + 4;
                if (!Model.GoalieID.HasValue)
                    return Position2D.Zero;
                poses = engine.GameInfo.CalculatePassScore(Model, Model.GoalieID.Value, RobotID/*, attackerPos*/, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);

                //double maxSc = double.MinValue;
                //foreach (var item in poses)
                //{
                //    if (item.score > maxSc)
                //    {
                //        maxSc = item.score;
                //        target = item.pos;
                //    }
                //}
                target = poses[1].pos;
            }
            ballHistory.Add(Model.BallState.Location);
            target.DrawColor = Color.DarkGreen;
            DrawingObjects.AddObject(target);
            #endregion
            return target;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new FreeKickAttackerRole() };
        }
    }
}
