# ExpectedBlocks

Provides a more managed means of finding and using blocks within your grid.

## Usage

1. Copy ExpectedBlocks.cs to your project
2. Create an instance of `ExpectedBlocks` in `Program.cs`
```
partial class Program : MyGridProgram
{
    ExpectedBlocks blocks;

	public Program()
	{
		blocks = new ExpectedBlocks(GridTerminalSystem);
		...
	}
```
3. Find all blocks you need with `FindBlock` and `FindBlocks`
```
	public Program()
	{
		blocks = new ExpectedBlocks(GridTerminalSystem);
		blocks.FindBlock<IMyTextPanel>("My LCD Panel");
		blocks.FindBlocks<IMyThrust>("My Thruster", 2);
	}
```
4. Get the blocks you want to use with `GetBlock` and `GetBlocks`
```
	public void DisplayThrustersOnPanel()
	{
		string message = "";
		blocks.GetBlocks<IMyThrust>("My Thruster").ForEach(thruster => message += thruster.DisplayNameText + "\n");
		blocks.GetBlock<IMyTextPanel>("My LCD Panel").WritePublicText(message);
	}
```
5. Pass `blocks` to the constructors of your other Utility classes so all blocks are available to them.
```
public class SomeUtilityClass
{
	ExpectedBlocks blocks;

	public SomeUtilityClass(ExpectedBlocks blocks)
	{
		this.blocks = blocks;
	}

	public void DoSomethingWithBlock()
	{
		blocks.GetBlock<BlockType>("Block Name").BlockAction(...);
	}
}
```