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
        public class PayloadTransportStateMachine
        {
            IMyMotorStator craneArmRotor;
            List<IMyPistonBase> craneLiftPistons;
            ThrustControl thrustControl;

            public PayloadTransportStateMachine(ExpectedBlocks blocks, ThrustControl thrustControl)
            {
                craneArmRotor = blocks.GetBlock<IMyMotorStator>(CRANE_ARM_ROTOR);
                craneLiftPistons = blocks.GetBlocks<IMyPistonBase>(CRANE_LIFT_PISTON);
                this.thrustControl = thrustControl;
            }

            public IEnumerator<bool> GenerateStateMachine()
            {
                if (state == State.TransportingPayload)
                {
                    thrustControl.Adjust(ThrustControl.TOP_ANGLE);
                    while (!craneAtTop)
                        yield return true;
                    LowerLift();
                    while (!craneLiftAtBottom)
                        yield return true;
                    StopLift();
                    state = State.WaitingForDropoff;
                }
            }

            // Block status:
            bool craneAtTop => thrustControl.GetCraneArmAngle() > ThrustControl.TOP_ANGLE - .5f;
            bool craneLiftAtBottom => craneLiftPistons.All(piston => piston.CurrentPosition <= 2);

            // Crane lift operations:
            void LowerLift() => craneLiftPistons.ForEach(piston => piston.Velocity = -.5f);
            void StopLift() => craneLiftPistons.ForEach(piston => piston.Velocity = 0);
        }
    }
}
