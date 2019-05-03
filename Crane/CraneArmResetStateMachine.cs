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
    partial class Program
    {
        public class CraneArmResetStateMachine
        {
            List<IMyPistonBase> craneLiftPistons;
            ThrustControl thrustControl;

            public CraneArmResetStateMachine(ExpectedBlocks blocks, ThrustControl thrustControl)
            {
                craneLiftPistons = blocks.GetBlocks<IMyPistonBase>(CRANE_LIFT_PISTON);
                this.thrustControl = thrustControl;
            }

            public IEnumerator<bool> GenerateStateMachine()
            {
                if (state == State.Reseting)
                {
                    if (!liftAtTop)
                    {
                        thrustControl.Adjust(ThrustControl.TOP_ANGLE);
                        RaiseCraneLift();
                    }
                    while (!liftAtTop)
                        yield return true;
                    StopCraneLift();
                    thrustControl.Adjust(ThrustControl.BOTTOM_ANGLE);
                    while (!craneArmAtBottom)
                        yield return true;
                    state = State.WaitingForPayload;
                }
            }

            // Block status:
            bool liftAtTop => craneLiftPistons.All(piston => piston.CurrentPosition == 10);
            bool craneArmAtBottom => thrustControl.GetCraneArmAngle() < ThrustControl.BOTTOM_ANGLE + .5f;

            // Crane operations:
            void RaiseCraneLift() => craneLiftPistons.ForEach(piston => piston.Velocity = .5f);
            void StopCraneLift() => craneLiftPistons.ForEach(piston => piston.Velocity = 0);
        }
    }
}
