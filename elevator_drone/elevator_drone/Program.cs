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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

        public Program()
        {

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }



        public void Save()
        {


        }


        string savedArg;

        IMyProgrammableBlock pb;
        IMyRadioAntenna antenna;
        bool setupComplete = false;
        bool messageRecieved = false;


        public void Main(string argument, UpdateType updateSource)
        {

            // Un-Comment these and manually run with no arguement to get the IGC.Me ID for unicast messaging
            //pb = GridTerminalSystem.GetBlockWithName("Auto-Pilot Script") as IMyProgrammableBlock;
            //pb.CustomData = IGC.Me.ToString();

            // Running Setup and checking Messages
            Setup();
            RecieveMessage();

            //Fucked shit
            if (argument == "earth")
            {
                savedArg = argument;
                Echo("savedArg: " + savedArg.ToString());
            }
            if (argument == "space")
            {
                savedArg = argument;
                Echo("savedArg: " + savedArg.ToString());
            }
            Echo("messageRecieved: " + messageRecieved.ToString());
            if (messageRecieved)
            {
                savedArg = pb.CustomData;
                pb.CustomData = "";
                Echo("savedArg: " + savedArg.ToString());
                messageRecieved = false;
            }


            var rc = GridTerminalSystem.GetBlockWithName("MainRemoteControl") as IMyRemoteControl;
            var textPanel = GridTerminalSystem.GetBlockWithName("Display Panel") as IMyTextPanel;
            var ctrl = GridTerminalSystem.GetBlockWithName("MainRemoteControl") as IMyShipController;

            double shutoffDistance = 750;
            Vector3D currentPosition = Me.GetPosition();

            if (savedArg == "earth")
            {
                Vector3D destination = new Vector3D(53828.94, -26593.54, 11940.2);
                rc.ClearWaypoints();
                rc.AddWaypoint(destination, "Destination");

                rc.ApplyAction("AutoPilot_On");

                var distance = Vector3D.Distance(destination, currentPosition);
                double altitude = 0;
                ctrl.TryGetPlanetElevation(MyPlanetElevation.Surface, out altitude).ToString();
                double speed = ctrl.GetShipSpeed();
                double gravStrength = ctrl.GetNaturalGravity().Length() / 9.81;

                if (distance > shutoffDistance)
                {
                    textPanel.WritePublicText("Destination: Earth Station");
                    textPanel.WritePublicText("\n \n    VELOCITY: " + Math.Round(speed, 2).ToString() + "m/s", true);
                    textPanel.WritePublicText("\n    ALTITUDE: " + Math.Round(altitude, 2).ToString() + " m", true);
                    textPanel.WritePublicText("\n    GRAVITY: " + Math.Round(gravStrength, 2).ToString() + " g", true);
                    textPanel.WritePublicText("\n    DISTANCE: " + Math.Round(distance, 2).ToString() + " m", true);
                    textPanel.WritePublicText("\n    ETA: " + Math.Round((distance / speed), 2).ToString() + " s", true);
                }

                if (distance <= shutoffDistance)
                {
                    rc.ApplyAction("AutoPilot_Off");
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                    textPanel.WritePublicText("Now arriving at destination.");
                }
            }
            if (savedArg == "space")
            {
                Vector3D destination = new Vector3D(96957.82, -50522.49, 19134.15);
                rc.ClearWaypoints();
                rc.AddWaypoint(destination, "Destination");

                rc.ApplyAction("AutoPilot_On");

                var distance = Vector3D.Distance(destination, currentPosition);
                double altitude = 0;
                ctrl.TryGetPlanetElevation(MyPlanetElevation.Surface, out altitude).ToString();
                double speed = ctrl.GetShipSpeed();
                double gravStrength = ctrl.GetNaturalGravity().Length() / 9.81;

                if (distance > shutoffDistance)
                {
                    textPanel.WritePublicText("Destination: Space Station 1");
                    textPanel.WritePublicText("\n \n    VELOCITY: " + Math.Round(speed, 2).ToString() + "m/s", true);
                    textPanel.WritePublicText("\n    ALTITUDE: " + Math.Round(altitude, 2).ToString() + " m", true);
                    textPanel.WritePublicText("\n    GRAVITY: " + Math.Round(gravStrength, 2).ToString() + " g", true);
                    textPanel.WritePublicText("\n    DISTANCE: " + Math.Round(distance, 2).ToString() + " m", true);
                    textPanel.WritePublicText("\n    ETA: " + Math.Round((distance / speed), 2).ToString() + " s", true);
                }

                if (distance <= shutoffDistance)
                {
                    rc.ApplyAction("AutoPilot_Off");
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                    textPanel.WritePublicText("Now arriving at destination.");
                }
            }
        }


        public void RecieveMessage()
        {
            for (int i = 0; i < 2; i++)
            {
                if (!setupComplete)
                {
                    Echo("Running setup.");
                    Setup();
                }
                if (setupComplete)
                {
                    // Declaring unicast listener
                    IMyUnicastListener uniSource = IGC.UnicastListener;
                    Echo("Pending Msg: " + uniSource.HasPendingMessage.ToString());
                    //pb.CustomData = IGC.Me.ToString();

                    // Accepting messages if there are any
                    if (uniSource.HasPendingMessage)
                    {
                        // Accepting messages
                        MyIGCMessage message = new MyIGCMessage();
                        message = uniSource.AcceptMessage();
                        Echo("Msg Tag: " + message.Tag.ToString());
                        pb.CustomData = message.Data.ToString();
                        messageRecieved = true;
                    }
                }
            }
        }

        public void Setup()
        {
            // Setup script for assigning pb and antenna

            pb = GridTerminalSystem.GetBlockWithName("Auto-Pilot Script") as IMyProgrammableBlock;
            antenna = GridTerminalSystem.GetBlockWithName("Unicast Antenna") as IMyRadioAntenna;

            if (antenna != null)
            {
                Echo("Setup Complete");
                setupComplete = true;
            }
            else
            {
                Echo("Setup failed: No antenna found with name 'Unicast Antenna'");
            }
        }

    }
}