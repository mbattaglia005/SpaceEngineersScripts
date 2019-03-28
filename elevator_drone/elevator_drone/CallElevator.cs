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

        public Program()
        {

            //Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }


        bool setupComplete = false;
        IMyRadioAntenna antenna;
        IMyProgrammableBlock pb;
        string savedArg;

        public void Main(string arg, UpdateType updateSource)
        {
            for (int i = 0; i < 2; i++)
            {
                // If setupComplete is false, run Setup method
                if (!setupComplete)
                {
                    Echo("Running setup.");
                    Setup();
                }
                else
                {
                    // Setting pb.CustomData based on arg
                    pb.CustomData = "";
                    if (arg == "earth")
                    {
                        pb.CustomData = "earth";
                    }
                    if (arg == "space")
                    {
                        pb.CustomData = "space";
                    }

                    // Setting arguement via pb.CustomData
                    savedArg = pb.CustomData;

                    // Declaring unicast sender
                    IMyUnicastListener uniSource = IGC.UnicastListener;

                    // Declaring target ID, tag and data
                    long targetId = 76289045578856837;
                    string tag1 = "Elevator Call";
                    string data = savedArg;

                    // Sending message
                    IGC.SendUnicastMessage(targetId, tag1, data);
                }
            }
        }

        public void Setup()
        {
            antenna = GridTerminalSystem.GetBlockWithName("Drone Testing Site") as IMyRadioAntenna;
            pb = GridTerminalSystem.GetBlockWithName("Programmable block") as IMyProgrammableBlock;

            // Connect the PB to the antenna. This can also be done from the grid terminal.
            antenna.AttachedProgrammableBlock = pb.EntityId;

            if (antenna != null)
            {
                Echo("Setup complete.");
                setupComplete = true;
            }
            else
            {
                Echo("Setup failed. No antenna found.");
            }
        }

    }
}
