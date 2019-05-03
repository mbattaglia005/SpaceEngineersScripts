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
        public class PayloadPickupStateMachine
        {
            List<IMyPistonBase> craneLiftPistons;
            List<IMyShipConnector> craneConnectors;

            int timer;

            public PayloadPickupStateMachine(ExpectedBlocks blocks)
            {
                craneLiftPistons = blocks.GetBlocks<IMyPistonBase>(CRANE_LIFT_PISTON);
                craneConnectors = blocks.GetBlocks<IMyShipConnector>(CRANE_ARM_CONNECTOR);
            }

            public IEnumerator<bool> GenerateStateMachine()
            {
                if (state == State.AcceptingPayload)
                {
                    timer = 0;
                    EnableConnectors();
                    LowerLift();
                    while (!CraneConnected && !CraneRejectedPayload)
                        yield return true;
                    StopLift();
                    if (CraneConnected)
                    {
                        while (timer < 180)
                        {
                            timer++;
                            yield return true;
                        }
                    }
                    if (CraneConnected)
                        LockWithPayload();
                    else
                        DisableConnectors();
                    RaiseLift();
                    while (!LiftFullyExtended)
                        yield return true;
                    StopLift();
                    if (CraneLocked)
                        state = State.WaitingForTransport;
                    else
                        state = State.WaitingForPayload;
                }
            }

            // Block status:
            bool CraneRejectedPayload => craneLiftPistons.Any(piston => piston.CurrentPosition < 8.5f);
            bool LiftFullyExtended => craneLiftPistons.All(piston => piston.CurrentPosition == 10);
            bool CraneConnected => craneConnectors.All(connector => connector.Status == MyShipConnectorStatus.Connectable);
            bool CraneLocked => craneConnectors.All(connector => connector.Status == MyShipConnectorStatus.Connected);

            // Crane Arm operations:
            void EnableConnectors() => craneConnectors.ForEach(connector => connector.Enabled = true);
            void DisableConnectors() => craneConnectors.ForEach(connector => connector.Enabled = false);
            void RaiseLift() => craneLiftPistons.ForEach(piston => piston.Velocity = .1f);
            void LowerLift() => craneLiftPistons.ForEach(piston => piston.Velocity = -.1f);
            void StopLift() => craneLiftPistons.ForEach(piston => piston.Velocity = 0);
            void LockWithPayload() => craneConnectors.ForEach(connector => connector.Connect());
        }
    }
}
