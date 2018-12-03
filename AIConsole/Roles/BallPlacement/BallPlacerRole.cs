﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.AIConsole.Roles
{
    class BallPlacerRole : RoleBase
    {
        int counter = 0;
        modes currentMode = modes.Pass;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (currentMode == modes.OneRobot)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.1)
                        CurrentState = (int)state.GoPlace;
                }
                else if (CurrentState == (int)state.GoPlace)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.1)
                        CurrentState = (int)state.Place;
                }
            }
            else if (currentMode == modes.Pass)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.1)
                        CurrentState = (int)state.Pass;
                }
                if (Model.BallState.Speed.Size > 0.6)
                {
                    CurrentState = (int)state.Halt;
                }
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public void Perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID)
        {
            if (currentMode == modes.OneRobot)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    GetSkill<GetBallSkill>().PerformForStrategy(engine, Model, RobotID, StaticVariables.ballPlacementPos);
                    Planner.AddKick(RobotID, true);
                }
                else if (CurrentState == (int)state.GoPlace)
                {
                    Planner.ChangeDefaulteParams(RobotID, false);
                    Planner.SetParameter(RobotID, 1.5);
                    Planner.Add(RobotID, StaticVariables.ballPlacementPos, (StaticVariables.ballPlacementPos - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    Planner.AddKick(RobotID, true);
                }
                else
                {
                    Planner.Add(RobotID, Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Angle.Value, PathType.UnSafe, false, false, false, false);
                    if (counter++ > 30)
                        Planner.AddKick(RobotID, false);
                    else
                        Planner.AddKick(RobotID, true);
                } 
            }
            else if (currentMode == modes.Pass)
            {
                if (CurrentState == (int)state.GoBehind)
                {
                    GetSkill<GetBallSkill>().PerformForStrategy(engine, Model, RobotID, StaticVariables.ballPlacementPos);
                    Planner.AddKick(RobotID, true);
                }
                else if (CurrentState == (int)state.Pass )
                {
                    Planner.AddRotate(Model,RobotID,StaticVariables.ballPlacementPos,0,kickPowerType.Speed,4,false);
                }
                else if (CurrentState == (int)state.Halt)
                {
                    Planner.Add(RobotID,Model.OurRobots[RobotID]);
                }

            }
        }

        public void Reset()
        {
            CurrentState = 0;
            counter = 0;
        }

        enum state
        {
            GoBehind,
            GoPlace,
            Place,
            Pass,
            Halt
        }
        enum modes
        { 
            OneRobot,
            Pass
        }
    }
}
