using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const string PISTON_DOOR = "Piston Door";
        const string ROTOR_DOOR_POWER = "Rotor Door [POWER]";
        const float ROTOR_VELOCITY = 2;
        const float PISTON_VELOCITY = .2f;
        const float SAFE_ROTOR_ANGLE = 30;

        ExpectedBlocks blocks;
        bool open = false;

        public Program()
        {
            blocks = new ExpectedBlocks(GridTerminalSystem);
            blocks.FindBlocks<IMyPistonBase>(PISTON_DOOR, 2);
            blocks.FindBlocks<IMyMotorStator>(ROTOR_DOOR_POWER, 2);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Trigger) == UpdateType.Trigger)
            {
                if (open)
                {
                    blocks.GetBlocks<IMyMotorStator>(ROTOR_DOOR_POWER).ForEach(r => r.TargetVelocityRPM = -ROTOR_VELOCITY);
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                }
                else
                {
                    blocks.GetBlocks<IMyMotorStator>(ROTOR_DOOR_POWER).ForEach(r => r.TargetVelocityRPM = ROTOR_VELOCITY);
                    blocks.GetBlocks<IMyPistonBase>(PISTON_DOOR).ForEach(p => p.Velocity = PISTON_VELOCITY);
                }

                open = !open;
            }

            if ((updateSource & UpdateType.Update10) == UpdateType.Update10)
            {
                if (open)
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                else if (blocks.GetBlocks<IMyMotorStator>(ROTOR_DOOR_POWER).All(r => r.Angle * 180 / Math.PI <= SAFE_ROTOR_ANGLE))
                {
                    blocks.GetBlocks<IMyPistonBase>(PISTON_DOOR).ForEach(p => p.Velocity = -PISTON_VELOCITY);
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                }
            }
        }
    }
}