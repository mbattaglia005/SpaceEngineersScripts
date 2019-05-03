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
    public enum State
    {
        WaitingForPayload,
        AcceptingPayload,
        WaitingForTransport,
        TransportingPayload,
        WaitingForDropoff,
        WaitingForReset,
        Reseting
    }

    partial class Program : MyGridProgram
    {
        static State state;

        // Acceptable arguments
        const string PICKUP = "PICKUP";
        const string DUMP = "DUMP";
        const string TRANSPORT = "TRANSPORT";
        const string DROPOFF = "DROPOFF";
        const string RESET = "RESET";
        // TEMP
        const string LOW = "LOW";
        const string HIGH = "HIGH";

        // Block names
        const string CRANE_ARM_STATUS_PANEL = "Crane Status Panel";
        const string CRANE_ARM_PISTON = "Crane Arm Piston";
        const string CRANE_ARM_ROTOR = "Crane Arm Rotor";
        const string CRANE_ARM_CONNECTOR = "Crane Arm Connector";
        const string CRANE_ARM_THRUSTER = "Crane Arm Thruster";
        const string CRANE_LIFT_PISTON = "Crane Lift Piston";

        IEnumerator<bool> stateMachine;
        ExpectedBlocks blocks;
        ThrustControl thrustControl;
        PayloadPickupStateMachine payloadPickupStateMachine;
        PayloadTransportStateMachine payloadTransportStateMachine;
        CraneArmResetStateMachine craneArmResetStateMachine;

        public Program()
        {
            // Load state if state was saved previously.
            int savedState;
            if (int.TryParse(Storage, out savedState))
                state = (State)savedState;
            else
                state = State.WaitingForPayload;

            // Find all needed blocks.
            blocks = new ExpectedBlocks(GridTerminalSystem);
            blocks.FindBlock<IMyTextPanel>(CRANE_ARM_STATUS_PANEL);
            blocks.FindBlock<IMyPistonBase>(CRANE_ARM_PISTON);
            blocks.FindBlock<IMyMotorStator>(CRANE_ARM_ROTOR);
            blocks.FindBlocks<IMyThrust>(CRANE_ARM_THRUSTER, 6);
            blocks.FindBlocks<IMyShipConnector>(CRANE_ARM_CONNECTOR, 3);
            blocks.FindBlocks<IMyPistonBase>(CRANE_LIFT_PISTON, 2);

            // Initialize other utility classes.
            thrustControl = new ThrustControl(blocks);
            payloadPickupStateMachine = new PayloadPickupStateMachine(blocks);
            payloadTransportStateMachine = new PayloadTransportStateMachine(blocks, thrustControl);
            craneArmResetStateMachine = new CraneArmResetStateMachine(blocks, thrustControl);

            // Start update loop.
            Runtime.UpdateFrequency |= UpdateFrequency.Update1;
        }

        public void Save()
        {
            Storage = state.ToString("d");
        }

        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & UpdateType.Update1) == UpdateType.Update1)
            {
                thrustControl.Update();
                blocks.GetBlock<IMyTextPanel>(CRANE_ARM_STATUS_PANEL).WritePublicText("Crane Arm Status\n\n" +
                    "Program\n" +
                    "  State: " + state.ToString() + "\n\n" +
                    thrustControl.GetDebugText());
            }

            if ((updateType & UpdateType.Terminal) == UpdateType.Terminal ||
                (updateType & UpdateType.Trigger) == UpdateType.Trigger)
            {
                switch (argument)
                {
                    case LOW:
                        thrustControl.Adjust(ThrustControl.BOTTOM_ANGLE);
                        break;
                    case HIGH:
                        thrustControl.Adjust(ThrustControl.TOP_ANGLE);
                        break;
                    case PICKUP:
                        if (state == State.WaitingForPayload)
                        {
                            state = State.AcceptingPayload;
                            stateMachine = payloadPickupStateMachine.GenerateStateMachine();
                            Runtime.UpdateFrequency |= UpdateFrequency.Once;
                        }
                        break;
                    case DUMP:
                        if (state == State.WaitingForTransport || state == State.WaitingForPayload)
                        {
                            blocks.GetBlocks<IMyShipConnector>(CRANE_ARM_CONNECTOR).ForEach(connector =>
                            {
                                connector.Disconnect();
                                connector.Enabled = false;
                            });
                            thrustControl.OverrideThrust(50);
                            state = State.WaitingForPayload;
                        }
                        break;
                    case TRANSPORT:
                        if (state == State.WaitingForTransport)
                        {
                            state = State.TransportingPayload;
                            stateMachine = payloadTransportStateMachine.GenerateStateMachine();
                            Runtime.UpdateFrequency |= UpdateFrequency.Once;
                        }
                        break;
                    case DROPOFF:
                        if (state == State.WaitingForDropoff)
                        {
                            blocks.GetBlocks<IMyShipConnector>(CRANE_ARM_CONNECTOR).ForEach(connector => {
                                connector.Disconnect();
                                connector.Enabled = false;
                            });
                            state = State.WaitingForReset;
                        }
                        break;
                    case RESET:
                        state = State.Reseting;
                        stateMachine = craneArmResetStateMachine.GenerateStateMachine();
                        Runtime.UpdateFrequency |= UpdateFrequency.Once;
                        break;
                    default:
                        if (argument.Length == 0)
                            Echo("ERROR Missing argument");
                        else
                            Echo("ERROR Unrecognized argument: " + argument);
                        break;
                }
            }

            if ((updateType & UpdateType.Once) == UpdateType.Once)
            {
                RunStateMachine();
            }
        }

        void RunStateMachine()
        {
            if (stateMachine != null)
            {
                if (stateMachine.MoveNext())
                {
                    Runtime.UpdateFrequency |= UpdateFrequency.Once;
                }
                else
                {
                    stateMachine.Dispose();
                    stateMachine = null;
                }
            }
        }
    }
}