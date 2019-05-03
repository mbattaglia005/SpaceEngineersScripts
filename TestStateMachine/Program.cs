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
        IMyInteriorLight _panelLight;
        IMyTextPanel _textPanel;
        IEnumerator<bool> _stateMachine;

        ExpectedBlocks blocks;

        string argument;

        public Program()
        {
            // Retrieve the blocks we're going to use.
            blocks = new ExpectedBlocks(GridTerminalSystem);
            _panelLight = blocks.GetBlock<IMyInteriorLight>("Interior Light");
            _textPanel = blocks.GetBlock<IMyTextPanel>("LCD Panel");
        }

        public void Main(string argument, UpdateType updateType)
        {
            if (argument.Length > 0)
            {
                this.argument = argument;
                Echo(this.argument);

                // Initialize our state machine
                _stateMachine = RunStuffOverTime();

                // Signal the programmable block to run again in the next tick. Be careful on how much you
                // do within a single tick, you can easily bog down your game. The more ticks you do your
                // operation over, the better.
                //
                // What is actually happening here is that we are _adding_ the Once flag to the frequencies.
                // By doing this we can have multiple frequencies going at any time.
                Runtime.UpdateFrequency |= UpdateFrequency.Once;
            }
            // We only want to run the state machine(s) when the update type includes the
            // "Once" flag, to avoid running it more often than it should. It shouldn't run
            // on any other trigger. This way we can combine state machine running with
            // other kinds of execution, like tool bar commands, sensors or what have you.
            if ((updateType & UpdateType.Once) == UpdateType.Once)
            {
                RunStateMachine();
            }
        }

        // ***MARKER: Coroutine Execution
        public void RunStateMachine()
        {
            // If there is an active state machine, run its next instruction set.
            if (_stateMachine != null)
            {
                // The MoveNext method is the most important part of this system. When you call
                // MoveNext, your method is invoked until it hits a `yield return` statement.
                // Once that happens, your method is halted and flow control returns _here_.
                // At this point, MoveNext will return `true` since there's more code in your
                // method to execute. Once your method reaches its end and there are no more
                // yields, MoveNext will return false to signal that the method has completed.
                // The actual return value of your yields are unimportant to the actual state
                // machine.

                // If there are no more instructions, we stop and release the state machine.
                if (!_stateMachine.MoveNext())
                {
                    _stateMachine.Dispose();

                    // In our case we just want to run this once, so we set the state machine
                    // variable to null. But if we wanted to continously run the same method, we
                    // could as well do
                    // _stateMachine = RunStuffOverTime();
                    // instead.
                    _stateMachine = null;
                }
                else
                {
                    // The state machine still has more work to do, so signal another run again, 
                    // just like at the beginning.
                    Runtime.UpdateFrequency |= UpdateFrequency.Once;
                }
            }
        }

        // ***MARKER: Coroutine Example
        // The return value (bool in this case) is not important for this example. It is not
        // actually in use.
        public IEnumerator<bool> RunStuffOverTime()
        {
            // For the very first instruction set, we will just switch on the light.
            _panelLight.Enabled = true;

            // Then we will tell the script to stop execution here and let the game do it's
            // thing. The time until the code continues on the next line after this yield return
            // depends  on your State Machine Execution and the timer setup.
            // The `true` portion is there simply because an enumerator needs to return a value
            // per item, in our case the value simply has no meaning at all. You _could_ utilize
            // it for a more advanced scheduler if you want, but that is beyond the scope of this
            // tutorial.
            yield return true;

            int i = 0;
            // The following would seemingly be an illegal operation, because the script would
            // keep running until the instruction count overflows. However, using yield return,
            // you can get around this limitation - without breaking the rules and while remaining
            // performance friendly.
            while (i < 100)
            {
                i++;
                _textPanel.WritePublicText(argument + " " + i.ToString());
                // Like before, when this statement is executed, control is returned to the game.
                // This way you can have a continuously polling script with complete state
                // management, with very little effort.
                yield return true;
            }
        }
    }
}