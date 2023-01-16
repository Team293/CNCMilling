// M6 Macro for use with STEPCRAFT Q-Series sliding rear tool changer
// Simple version (V1.0) 2/7/20

// Hopewell Valley High School, Erick Tornegard
// Q.408 No.627 (ZWC)
// 1/10/2022
// Sliding tool rack

// Modifications: Trevor Daly, Elliot Scully
// 1/14/2023
// Manual tool change

// Coordinate measurements are in millimeters

// Port definitions
const int ChuckOpenPort = 1;
const int ChuckOpenPin = 14;
const int RackOpenPort = 1;
const int RackOpenPin = 16;

// Basic parameters for tool change
const double SafeZ = -3; // absolute position relative to Z Home for all XY motions
const int WaitForSpindle = 9000; // msec
const int WaitForRack = 3750; // msec

// *** Note: Feed and offset numbers are tied to this system configuration, air pressure etc.
const double FeedRateZ = 2000;

// *** Note: set offset values to control smooth motion while changing tools
const double ZToolEjectOffset = 7.0; // when ejecting tool, release stud while going up eject height
const double ZToolEngageOffset = 3.5; // when engaging tool, go to engage offset then grab stud while lowering

// *** Note: this value MUST be set to match the tool rack height
const double ZToolRelease = -79.9; // tool slide into clamp height when releasing

// *** Note: increase this NEGATIVE value to stay closer to top of pull studs between change
const double ZToolOffset = 0; // added to SafeZ for XY motion between tools

// *** Note: set the first tool XY positions (base) and relative offsets (pitch) for others
// Pitch values may be negative to change the direction of tool sequencing
const double PitchX = 130.0;
const double PitchY = 0.0;

const double BaseX = 23.925; // Tool1 X base position
const double BaseY = 2409.3; // Tool1 Y base position

const double ParkX = 597.115; // Park X position
const double ParkY = -2279.7625; // Park Y position
const double ParkZ = SafeZ; // Park Z position


// Tool index 0 is NOT used and the array is declared one larger than required
const int MaxTool = 10;
const int MaxTotalTool = 48;
double[] ToolX = new double[MaxTool + 1]; // MUST be equal to MaxTool + 1
double[] ToolY = new double[MaxTool + 1]; // MUST be equal to MaxTool + 1
ToolX[0] = 0.0; // NOT USED
ToolY[0] = 0.0; // NOT USED

ToolX[1] = BaseX + 0 * PitchX + 0.0; // Tool1 X position
ToolY[1] = BaseY + 0 * PitchY + 0.0; // Tool1 Y position
ToolX[2] = BaseX + 1 * PitchX + 0.0; // Tool2 X position
ToolY[2] = BaseY + 1 * PitchY + 0.0; // Tool2 Y position
ToolX[3] = BaseX + 2 * PitchX + 0.0; // Tool3 X position
ToolY[3] = BaseY + 2 * PitchY + 0.0; // Tool3 Y position
ToolX[4] = BaseX + 3 * PitchX + 0.0; // Tool4 X position
ToolY[4] = BaseY + 3 * PitchY + 0.0; // Tool4 Y position
ToolX[5] = BaseX + 4 * PitchX + 0.0; // Tool5 X position
ToolY[5] = BaseY + 4 * PitchY + 0.0; // Tool5 Y position
ToolX[6] = BaseX + 5 * PitchX + 0.0; // Tool6 X position
ToolY[6] = BaseY + 5 * PitchY + 0.0; // Tool6 Y position
ToolX[7] = BaseX + 6 * PitchX + 0.0; // Tool7 X position
ToolY[7] = BaseY + 6 * PitchY + 0.0; // Tool7 Y position
ToolX[8] = BaseX + 7 * PitchX + 0.0; // Tool8 X position
ToolY[8] = BaseY + 7 * PitchY + 0.0; // Tool8 Y position
ToolX[9] = BaseX + 8 * PitchX + 0.0; // Tool9 X position
ToolY[9] = BaseY + 8 * PitchY + 0.0; // Tool9 Y position
ToolX[10] = BaseX + 9 * PitchX + 0.0; // Tool10 X position
ToolY[10] = BaseY + 9 * PitchY + 0.0; // Tool10 Y position

// Get tool change parameters
const int NewTool = exec.Getnewtool();
const int CurrentTool = exec.Getcurrenttool();


int TimeToWait = 0;

// Make sure the new tool is valid
if (NewTool == CurrentTool) // Same tool was selected, so do nothing
{
  MessageBox.Show("Tool change failed, same tool was selected!");
  return;
}
if (NewTool == -1) // If new tool number is -1, it means a missing T code
{
  MessageBox.Show("Tool change failed, T code is missing!");
  return;
}
else if((NewTool < 1) || (NewTool > MaxTotalTool)) // Tool number is out of range
{
  MessageBox.Show("Tool change to " + NewTool + " failed, new tool number is out of range! (Min: 1, Max: " + MaxTotalTool + ")");
  return;
}

// If machine X, Y, Z were not homed, unsafe to move in machine coordinates, stop here
if (!exec.GetLED(56) || !exec.GetLED(57) || !exec.GetLED(58))
{
  MessageBox.Show("The machine was not yet homed, do homing before executing a tool change!");
  exec.Stop();
  return;
}

// Save the current machine position
double Xoriginalpos = exec.GetXmachpos();
double Yoriginalpos = exec.GetYmachpos();

// Stop spindle if running CW or CCW
if (exec.GetLED(50) || exec.GetLED(51))
{
  exec.Stopspin(); // M5
  if (TimeToWait < WaitForSpindle)
    TimeToWait = WaitForSpindle;
}

// Verify tool rack position
if (exec.GetLED(RackOpenPin))
{
  MessageBox.Show("Tool rack extended, retracting."); // Safety message
  if (!exec.Ismacrostopped())
  {
    exec.Wait(100); // Wait in msec
    TimeToWait -= 100;
    exec.AddStatusmessage("Rack retracting");
    exec.Clroutpin(RackOpenPort, RackOpenPin); // Retract tool rack
    if (TimeToWait < WaitForRack)
      TimeToWait = WaitForRack;
  }
}

// Verify chuck status
if ((CurrentTool != 0) && (exec.GetLED(ChuckOpenPin)))
{
  MessageBox.Show("Chuck open (no tool), tool set to " + CurrentTool + ". Click OK to reset the ATC or STOP to ignore."); // Safety message
  if (!exec.Ismacrostopped())
  {
    exec.Wait(100); // Wait in msec
    exec.Clroutpin(ChuckOpenPort, ChuckOpenPin); // Close the chuck with pneumatic valve
    exec.Wait(500); // Wait in msec
    exec.Setcurrenttool(0);
    MessageBox.Show("ATC is reset and tool set to 0. Please run the macro again.");
    exec.Stop();
    return;
  }
}

while(exec.IsMoving()){} // Be extra safe, all motion done

// Move Z up
exec.Code("G0 G53 Z" + SafeZ); // Move Z up close to Home
if (TimeToWait > 0)
  exec.Wait(TimeToWait); // Wait in msec

while(exec.IsMoving()){}

// If the current tool is 0 (no tool in chuck), then open the chuck
if (CurrentTool == 0)
{  // Open empty chuck
  MessageBox.Show("No tool in chuck. Click OK to continue."); // Safety message
  exec.Setoutpin(ChuckOpenPort, ChuckOpenPin); // Open the chuck with pneumatic valve
  exec.Wait(300); // Wait in msec

  while(exec.IsMoving()){}
}
// If the current tool is saved in the ATC, then eject it to the tool holder
if (CurrentTool <= 10)
{  // Eject old tool
  // Move to old tool position on XY plane
  exec.Code("G0 G53 X" + ToolX[CurrentTool] + " Y" + ToolY[CurrentTool]);

  while(exec.IsMoving()){}

  // Lower current tool to drop off height
  exec.Code("G0 G53 Z" + ZToolRelease); // Move Z axis down to tool holder level

  while(exec.IsMoving()){}

  // Slide rack forward to engage tool
  exec.Setoutpin(RackOpenPort, RackOpenPin); // Activate tool rack
  exec.Wait(WaitForRack); // Wait in msec

  // Raise spindle while ejecting
  exec.Setoutpin(ChuckOpenPort, ChuckOpenPin); // Open the chuck with pneumatic valve
  exec.Wait(50); // Wait in msec, TOOL EJECT STRAIN RELIEF DELAY
  exec.Code("G1 F" + FeedRateZ + " G53 Z" + (ZToolRelease + ZToolEjectOffset)); // Move Z axis up to eject height
  exec.Wait(200); // Wait in msec

  while(exec.IsMoving()){}

  // Clear pull stud to move to new tool
  exec.Code("G0 G53 Z" + (SafeZ + ZToolOffset)); // Move Z up to clear pull stud

  while(exec.IsMoving()){}
}
// If the current tool is not in the ATC, then remove it manually
else if (CurrentTool > 10)
{
  // Make the user remove the tool manually
  // If the user says no to the above message, the macro will stop here
  DialogResult dialogResult = MessageBox.Show("Tool " + CurrentTool + " is not in the ATC rack. Click OK to begin the manual removal process.", "Tool Change", MessageBoxButtons.OKCancel);
  if (dialogResult == DialogResult.Cancel)
  {
    // Stop the macro
    return;
  }

  // Move the spindle to the tool change position (ParkX, ParkY)
  exec.Code("G0 G53 X" + ParkX + " Y" + ParkY);
  // Move the spindle to the tool change position (ParkZ)
  exec.Code("G0 G53 Z" + ParkZ);

  // Await user confirmation that the tool is ready to be removed
  dialogResult = MessageBox.Show("When you are ready to release the tool, click OK. MAKE SURE YOU ARE HOLDING THE TOOL THAT IS IN THE CHUCK.", "Tool Change", MessageBoxButtons.OKCancel);
  if (dialogResult == DialogResult.Cancel)
  {
    // Stop the macro
    return;
  }

  // Release the tool
  exec.Setoutpin(ChuckOpenPort, ChuckOpenPin); // Open the chuck with pneumatic valve

  // Await user confirmation that the tool is released
  dialogResult = MessageBox.Show("Verify that tool is released from the spindle. Select OK to continue.", "Tool Change", MessageBoxButtons.OKCancel);
  if (dialogResult == DialogResult.Cancel)
  {
    // Stop the macro
    return;
  }
}

// If the new tool is saved in the ATC, then pick it up
if (NewTool <= 10)
{
  // Pick up new tool
  // Move to new tool position on XY plane
  exec.Code("G0 G53 X" + ToolX[NewTool] + " Y" + ToolY[NewTool]);
  while(exec.IsMoving()){}
  // Move Z axis down near tool holder level
  exec.Code("G0 G53 Z" + (ZToolRelease + ZToolEngageOffset));

  while(exec.IsMoving()){}

  // Lower Z while engaging tool stud
  exec.Clroutpin(ChuckOpenPort, ChuckOpenPin); // Close the chuck with pneumatic valve
  exec.Wait(5); // Wait in msec
  exec.Code("G1 F" + FeedRateZ + " G53 Z" + ZToolRelease); // Move Z axis down to engage height
  exec.Wait(200); // Wait in msec

  while(exec.IsMoving()){}

  if (!exec.Ismacrostopped()) // If tool change was not interrupted with a STOP only then validate new tool number
    exec.Setcurrenttool(NewTool); // Set the current tool to the new tool

  // Retract tool rack
  exec.Clroutpin(RackOpenPort, RackOpenPin); // Retract tool rack
  exec.Wait(WaitForRack); // Wait in msec
  exec.Code("G0 G53 Z" + SafeZ); // Move Z up close to Home
}
// If the new tool is not in the ATC, then place it manually
else if(NewTool > 10)
{
  // Retract tool rack
  exec.Clroutpin(RackOpenPort, RackOpenPin); // Retract tool rack
  exec.Wait(WaitForRack); // Wait in msec
  exec.Code("G0 G53 Z" + SafeZ); // Move Z up close to Home

  // Move the spindle to the tool change position (ParkX, ParkY)
  exec.Code("G0 G53 X" + ParkX + " Y" + ParkY);
  // Move the spindle to the tool change position (ParkZ)
  exec.Code("G0 G53 Z" + ParkZ);

  // Await user confirmation that the tool is ready to be placed
  DialogResult dialogResult = MessageBox.Show("Place tool into chuck. Click OK when you are ready to continue.", "Tool Change", MessageBoxButtons.OKCancel);
  if (dialogResult == DialogResult.Cancel)
  {
    // Stop the macro
    return;
  }

  // Close the chuck
  exec.Clroutpin(ChuckOpenPort, ChuckOpenPin); // Close the chuck with pneumatic valve
  exec.Wait(5); // Wait in msec

  // Await user confirmation that the tool is in the spindle
  dialogResult = MessageBox.Show("Verify that tool is in the spindle. Select OK to continue.", "Tool Change", MessageBoxButtons.OKCancel);
  if (dialogResult == DialogResult.Cancel)
  {
    // Stop the macro
    return;
  }

  if (!exec.Ismacrostopped()) // If tool change was not interrupted with a STOP only then validate new tool number
    exec.Setcurrenttool(NewTool); // Set the current tool to the new tool
}

while(exec.IsMoving()){}

// Move back to start point
exec.Code("G00 G53 X" + Xoriginalpos + " Y" + Yoriginalpos);

while(exec.IsMoving()){}

// Measure new tool will go here
exec.Code("G43 H" + NewTool); // Load new tool offset

while(exec.IsMoving()){}

if (exec.Ismacrostopped()) // STOP interruption
{
  exec.StopWithDeccel();
  MessageBox.Show("Tool change was interrupted by user!");
}
