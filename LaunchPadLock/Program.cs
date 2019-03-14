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
        const float OPEN_ANGLE = 20f;
        const float CLOSE_ANGLE = 0f;
        const float VELOCITY = .5f;

        bool open;

        List<IMyLandingGear> locks = new List<IMyLandingGear>();
        List<IMyMotorStator> rotorsReg = new List<IMyMotorStator>();
        List<IMyMotorStator> rotorsInv = new List<IMyMotorStator>();

        public Program()
        {
            GridTerminalSystem.GetBlocksOfType(locks, lg => lg.DisplayNameText.Contains("Lock"));
            GridTerminalSystem.GetBlocksOfType(rotorsReg, rotor => rotor.DisplayNameText.Contains("[REGULAR]"));
            GridTerminalSystem.GetBlocksOfType(rotorsInv, rotor => rotor.DisplayNameText.Contains("[INVERTED]"));
            
            foreach(IMyMotorStator rotor in rotorsReg)
            {
                rotor.SetValue<float>("UpperLimit", OPEN_ANGLE);
                rotor.SetValue<float>("LowerLimit", CLOSE_ANGLE);
            }
            foreach (IMyMotorStator rotor in rotorsInv)
            {
                rotor.SetValue<float>("UpperLimit", CLOSE_ANGLE);
                rotor.SetValue<float>("LowerLimit", -OPEN_ANGLE);
            }

            open = false;
            CloseRotors();
            Lock();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (open)
            {
                Lock();
                CloseRotors();
            }
            else
            {
                Unlock();
                OpenRotors();
            }
            open = !open;
        }

        public void Lock()
        {
            foreach (IMyLandingGear lg in locks)
            {
                lg.AutoLock = true;
            }
        }

        public void Unlock()
        {
            foreach (IMyLandingGear lg in locks)
            {
                lg.AutoLock = false;
                lg.Unlock();
            }
        }

        public void OpenRotors()
        {
            foreach(IMyMotorStator rotor in rotorsReg)
            {
                rotor.SetValue<float>("Velocity", VELOCITY);
            }
            foreach (IMyMotorStator rotor in rotorsInv)
            {
                rotor.SetValue<float>("Velocity", -VELOCITY);
            }
        }

        public void CloseRotors()
        {
            foreach (IMyMotorStator rotor in rotorsReg)
            {
                rotor.SetValue<float>("Velocity", -VELOCITY);
            }
            foreach (IMyMotorStator rotor in rotorsInv)
            {
                rotor.SetValue<float>("Velocity", VELOCITY);
            }
        }
    }
}