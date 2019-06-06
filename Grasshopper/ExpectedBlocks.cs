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

            Dictionary<string, IMyTerminalBlock> singleBlocks;
            Dictionary<string, List<IMyTerminalBlock>> multipleBlocks;

            /// <summary>
            /// Creates an ExpectedBlock instance to find all blocks that you need for your script.
            /// </summary>
            /// <param name="grid">The grid system to use to find the blocks.  Typically 'GridTerminalSystem'.</param>
            public ExpectedBlocks(IMyGridTerminalSystem grid)
            {
                this.grid = grid;
                this.singleBlocks = new Dictionary<string, IMyTerminalBlock>();
                this.multipleBlocks = new Dictionary<string, List<IMyTerminalBlock>>();
            }

            /// <summary>
            /// Get a single block with the exact name provided.
            /// </summary>
            /// <typeparam name="T">The type of the block you are looking for.</typeparam>
            /// <param name="name">The exact name of the block you are looking for.</param>
            /// <returns></returns>
            public void FindBlock<T>(string name) where T : class, IMyTerminalBlock
            {
                List<T> foundBlocks = new List<T>();
                grid.GetBlocksOfType(foundBlocks, block => block.DisplayNameText.Equals(name));
                if (foundBlocks.Count == 0)
                    throw new Exception("Block not found: " + name);
                else if (foundBlocks.Count > 1)
                    throw new Exception("More than one block named " + name);
                else
                    singleBlocks.Add(name, foundBlocks[0]);
            }

            /// <summary>
            /// Get a collection of blocks that contain the name provided.
            /// </summary>
            /// <typeparam name="T">The type of the blocks you are looking for.</typeparam>
            /// <param name="nameSubstring">The text that is included in the name of the blocks you are looking for.</param>
            /// <param name="expectedCount">The number of blocks you are looking for.</param>
            /// <returns></returns>
            public void FindBlocks<T>(string nameSubstring, int expectedCount) where T : class, IMyTerminalBlock
            {
                List<IMyTerminalBlock> foundBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<T>(foundBlocks, block => block.DisplayNameText.Contains(nameSubstring));
                if (foundBlocks.Count != expectedCount)
                    throw new Exception("Incorrect number of blocks found containing name: " + nameSubstring + "\n" +
                        "Expected " + expectedCount.ToString() + " but found " + foundBlocks.Count.ToString());
                else
                    multipleBlocks.Add(nameSubstring, foundBlocks);
            }

            /// <summary>
            /// Get a single block that have been added with 'FindBlock'.
            /// </summary>
            /// <typeparam name="T">The type of the block you are trying to get.</typeparam>
            /// <param name="name">The text that was used to initially find the block.</param>
            /// <returns>The requested block by 'name' as type 'T'</returns>
            public T GetBlock<T>(string name) where T : class, IMyTerminalBlock
            {
                try
                {
                    return (T)singleBlocks[name];
                }
                catch (KeyNotFoundException)
                {
                    throw new Exception("Could not find block: " + name + "\n" +
                        "Did you forget to add this block, or is it possibly a list?");
                }
            }

            /// <summary>
            /// Get a collection of blocks that have been added with 'FindBlocks'.
            /// </summary>
            /// <typeparam name="T">The type of the blocks that you are trying to get.</typeparam>
            /// <param name="nameSubstring">The text that was used to initially find the blocks.</param>
            /// <returns></returns>
            public List<T> GetBlocks<T>(string nameSubstring) where T : class, IMyTerminalBlock
            {
                try
                {
                    return multipleBlocks[nameSubstring].Cast<T>().ToList();
                }
                catch (KeyNotFoundException)
                {
                    throw new Exception("Could not find blocks: " + nameSubstring + "\n" +
                        "Did you forget to add this collection of blocks, or is it possibly a single block?");
                }
            }
        }
    }
}
