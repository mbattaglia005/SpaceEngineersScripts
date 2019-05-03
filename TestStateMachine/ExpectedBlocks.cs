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
        public class ExpectedBlocks
        {
            private IMyGridTerminalSystem grid;

            /// <summary>
            /// Creates an ExpectedBlock instance.
            /// </summary>
            /// <param name="grid">The grid system to use to find the blocks.  Typically 'GridTerminalSystem'.</param>
            public ExpectedBlocks(IMyGridTerminalSystem grid)
            {
                this.grid = grid;
            }

            /// <summary>
            /// Get a single block with the exact name provided.
            /// </summary>
            /// <typeparam name="T">The type of the block you are looking for.</typeparam>
            /// <param name="name">The exact name of the block you are looking for.</param>
            /// <returns></returns>
            public T GetBlock<T>(string name) where T : class, IMyTerminalBlock
            {
                List<T> foundBlocks = new List<T>();
                grid.GetBlocksOfType(foundBlocks, block => block.DisplayNameText.Equals(name));
                if (foundBlocks.Count == 0)
                    throw new Exception("Block not found: " + name);
                if (foundBlocks.Count > 1)
                    throw new Exception("More than one block named " + name);
                return foundBlocks[0];
            }

            /// <summary>
            /// Get a collection of blocks that contain the name provided.
            /// </summary>
            /// <typeparam name="T">The type of the blocks you are looking for.</typeparam>
            /// <param name="nameSubstring">The text that is included in the name of the blocks you are looking for.</param>
            /// <param name="expectedCount">The number of blocks you are looking for.</param>
            /// <returns></returns>
            public List<T> GetBlocks<T>(string nameSubstring, int expectedCount) where T : class, IMyTerminalBlock
            {
                List<T> foundBlocks = new List<T>();
                grid.GetBlocksOfType(foundBlocks, block => block.DisplayNameText.Contains(nameSubstring));
                if (foundBlocks.Count != expectedCount)
                    throw new Exception("Incorrect number of blocks found containing name: " + nameSubstring + "\n" +
                        "Expected " + expectedCount.ToString() + " but found " + foundBlocks.Count.ToString());
                return foundBlocks;
            }
        }
    }
}
