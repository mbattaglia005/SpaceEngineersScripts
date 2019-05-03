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
        ExpectedBlocks blocks;

        public Program()
        {
            blocks = new ExpectedBlocks(GridTerminalSystem);
            blocks.FindBlocks<IMyLightingBlock>("Interior Light", -1);
            blocks.FindBlocks<IMyLightingBlock>("Corner Light", -1);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            blocks.GetBlocks<IMyLightingBlock>("Interior Light").ForEach(light =>
            {
                Echo(light.DisplayNameText);
                if (!light.DisplayNameText.Contains("["))
                {
                    light.CustomName = "Interior Light";
                    light.Radius = 10;
                    light.Falloff = .5f;
                    light.Intensity = 4;
                }
            });
            blocks.GetBlocks<IMyLightingBlock>("Corner Light").ForEach(light =>
            {
                Echo(light.DisplayNameText);
                if (!light.DisplayNameText.Contains("["))
                {
                    if (light.DisplayNameText.Contains("Double"))
                        light.CustomName = "Corner Light - Double";
                    else
                        light.CustomName = "Corner Light";
                    light.Radius = 10;
                    light.Falloff = .5f;
                    light.Intensity = 3;
                }
            });
        }
    }
}