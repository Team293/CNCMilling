# Words of Warning

Observe good practices when using or producing G-code.

Always run the simulator on the G-code whenever you make changes to
it. While running the simulator, watch for these things in particular:

1) The spindle is on when it should be (and only when it should be).

2) Watch the spindle speed and make sure it is on whenever the tool is
  close to the stock. It should almost always be spinning near the stock.

3) Watch the tool length and make sure it is set as early as
  possible. Positioning the spindle anywhere near the stock is a no-no
  without an active tool length set (unless you are absolutely sure
  there will never be a tool in the spindle).

4) Step through and make sure the tool is never going to collide with
  the fixture. Or the stock (unless cutting, of course). Watch your Z
  values and make sure it never goes too far down.

When possible, insert a move to a particular position near the start
of the cut. For example, 1 inch above the cut. This way, when you step
through the code on the real machine, you will be able to visually
verify the tool position before the cutting starts.

Always keep track of where the spindle might be at any point. A change
within the G-code might change that position and affect any following
commands. It's easy to introduce a sequence that causes a collision,
when you are not accounting for all possibilities.

Whenever you start a move near the stock, and you are not cutting, you
almost always should move Z first, away from the stock, before X
and/or Y. This can even be the case if you re-reference the axes while
the spindle is near the stock or fixture. Do the Z first. Conversely,
you will almost always want to move Z last when approaching the
stock. Do your X and Y moves when you are sure the tool is away from
the stock, then approach along Z.

There will almost always be a tool offset set. You can think of the
tool offset as the distance from the bottom of the spindle to the
bottom of the tool, and this is how it is usually set. If there is no
tool and no tool offset set, then (typically) Z0 will put the bottom
of the spindle right on top of your stock. With a tool loaded and a
tool offset set, then Z0 will position the tip of the tool right on
top of your stock.

It's very important to have the correct tool offset set whenever there
is a tool in the spindle. If there is a tool and no offset is set,
then moving to Z0 will force your tool into your stock, usually
causing a disaster.

Never do a rapid move to engage the tool with the stock. Always set
the correct feed rate for your tool and your stock.

# Tormach PathPilot Simulator

PathPilot is the controller software. This is what you are using on
the console next to the machine. There is an online simulator that
runs PathPilot that you can use to test your G-code. The simulator is
part of PathPilot Hub, at https://hub.pathpilot.com.

Once you create an account and log in, you will be on the file
management screen. This is where you can upload your G-code files.
It also has a button to "Create Virtual PathPilot Controller".
Click that and choose the CNC one.

One thing to keep in mind is that you may have to occasionally jiggle
the mouse to get the screen to refresh. This can happen while
connecting or during a simulation. If it seems to be stuck, just
jiggle your mouse.

You should now have the option to "Enter PathPilot". Do that and click
"Connect". Agree to sell your soul, then select a machine. I don't
think our Tormach model is here, so I use the 1100M.

From here, we need to set up the machine.

1) Reset.

2) Reference all axes. Do Z first just to build up the habit.
  
3) Click the "Offsets" tab and scroll down to 293 and 294.
  
4) Now we enter tool information.
  
    a) Click next to the 293 a couple times, and you should be able to enter a name. It doesn't really matter. I use "cb" since this is the counterboring tool. After you press Enter, you will get to the diameter field.
  
    b) Enter 0.375 as the diameter.
  
    c) Enter a length. It doesn't matter much, just make it
  reasonable. I choose something in the 3-5 inch range.
  
   d) Now do the same process for 294. The diameter should be smaller, say 0.25.
  
5) If you do not set up the automatic tool changer (ATC), the
   simulator will prompt for tool changes, just like on the machine. If
   you want to use the tool changer, follow these steps:
  
   a) Click the "Settings" tab.
  
   b) Click next to "ATC".
  
   c) Now go to the "ATC" tab that just sprang into being.
  
   d) Enter "293" in the top left box and click "Insert". A "293"
  should appear on the right of the tool changer diagram. If it
  happens to be an asterisk, that just means that the tool is
  already loaded on the spindle.
  
   e) Click "Tray Fwd".
  
   f) Now insert tool 294.
  
   g) Go to the the main screen and enter "G53 G0 Z-2" in the MDI
  box. Wait for it to position.
  
   h) Go back to the ATC tab and click "Set TC Pos".
  
6. At this point you should be ready to load the file you
   uploaded. (You did, right?) This is a slightly different process than
   on the actual machine.
  
   a) Go to the "File" tab.
  
   b) If you don't see your file in the middle file browser, you may
  need to click the circular arrow to refresh the list. You will need to do this whenever you upload a new file as well.
  
   c) Now copy the file to the left.
  
   d) Select it and click "Load G-code" or just double click on the
  file name.
  
7. You should now be in the main tab with your file loaded and ready
   to go.

The reason for entering "G53 G0 Z-2" is that you need to set the
position of the ATC. It uses the current spindle position, and it only
allows a Z of -1.5 to -4 or so.

I don't know of any way to save the controller configuration. (If you
figure out a way, please share it!) This means that you will need to
go through this process each time you start a new controller. The
controller should last 6 hours.

If you get an error like "Tool setter input changed during motion":
This is a weird simulator issue. It will probably go away if you do a
move during initialation, say after setting your WCS. I have this sequence:
```
  (( Initialize Tormach ))
  G17 G20 G90 G40 G49 G64 P0.001 Q0.0000 G80 S0 M5 (boilerplate)
  G10 L2 P7 X0.5 Y-1.385 Z-12.2861 (set 59.1 offsets)
  G59.1 (switch to WCS)
  G0 Z10
  G0 X0 Y0 (safe starting position, also simulator requires a move here)
```

# G-Code

Here's a list of G-codes that we use (and some others included for
clarity) with examples and explanations as I currently understand
them.

Despite the name, there are also M-codes, S-codes, and T-codes.

It's important to note that different machines may support different
codes. Every machine will support G0, G1, F, and other really common
codes, but some machines do not support canned cycles or G30, for
example. Different machines will also interpret some commands somewhat
differently. This document is written with regards to the Tormach and
PathPilot.

## Definitions, Syntax

### Comments

Comments are within parentheses. Usually they are ignored. However, if
comments are in the same line with M0, those comments are displayed on
the console.

### Block

A block is a line in your G-code. Some commands can be combined in a
single block to do something slightly special. Some only apply to the
block they are in. A good example of this is G53, which only applies
to the line it is on.

For movement, any coordinates given on the same line indicate a single
move to those coordinates.

Examples:
```
G53 G0 X5 Y6
G0 X4 Y4
```
This does a rapid move in the machine coordinate system, then does a
rapid move in the work coordinate system, because the G53 only holds
for the one block. (G53 instructs the controller to use the machine
coordinate system for the move that is in the same block.)
```
G0 X3 Y-1
```
This moves to X3 Y-1 in a single, straight line (one move).
```
G0 X3
Y-1
```
This moves to X3 and then moves to Y-1 (two different moves).

## Coordinate Systems

There is a machine coordinate system, which is always the same. For
the Tormach, X0 Y0 Z0 is up, left, and back.

On the Tormach, positive X direction is to the right, positive Y
direction is to the back, positive Z direction is up. That means that,
**in machine coordinates**, all X will be positive, all Y negative, and
all Z negative. X1 Y-1 Z-1 will move to the right, to the front, and
down from X0 Y0 Z0.

Then there is the work coordinate system (WCS). There may be many work
coordinate systems saved, and they are invoked with other
commands. Once a WCS is invoked, all new coordinates are relative to
the WCS origin that was loaded.

If the WCS origin is set to X5 Y-1 Z-7 (which is in machine
coordinates), moving to X0 Y0 Z0 after invoking that coordinate system
will move to the machine coordinates X5 Y-1 Z-7. In other words, X5
Y-1 Z-7 in machine coordinates becomes the new X0 Y0 Z0 in the work
coordinates.

To convert from WCS to machine coordinates, add the WCS values to the
work coordinates. Maybe it's better to explain this way:

Xm = Xwo + Xw

where Xm is the machine coordinate we are converting to, Xwo is the
work origin position (stored in the active WCS register), and Xw is the
coordinate within the WCS. So if the WCS origin is set at X5, the work
coordinate Xw will be machine coordinate Xw + 5. It's easier than it
sounds.

#### G10 L2 P7 X? Y? Z?

Activates work coordinate system (WCS) stored in G59.1. Fun fact: G59.1
refers to WCS register 7, which is why we have P7. P1 goes with G54,
and P0 is the current coordinate system. The L values are strange and
confusing, so just use the right one!
```
P0 <-> active coordinate system
P1 <-> G54
P2 <-> G55
P3 <-> G56
P4 <-> G57
P5 <-> G58
P6 <-> G59
P7 <-> G59.1
P8 <-> G59.2
etc.
```
Example:
```
G10 L2 P1 X0.5 Y-1 Z-13 (store these coordinates in register 1 to be used with G54)
```
```
G10 L2 P7 X0.5 Y-1 Z-13 (store these coordinates in register 7 to be used with G59.1)
```
The first one means that if you say G54, the new X origin will be
machine X position 0.5, the Y origin will be machine Y position -1,
and the Z origin will be machine Z position -13. The last one is the
same, except that it takes effect when you use G59.1 instead.

#### G53

The movement on this line is to be interpreted in machine coordinates,
not work coordinates. This only applies to the line it is on.

Example:
```
G53 G0 X5 Y-1
```

#### G59.1

Switch to the work coordinate system (WCS) stored with G10 P7.

## Movement

#### F?

Set feed rate (inches per minute in imperial units, mm/min in metric
units) for all following commands that use it. Often appears in the
same block with G1.

Examples:
```
F6.5
G1 X1 Y-0.5 Z-10
```
```
G1 X1 Y-0.5 Z-10 F6.5
```
Move to the new position at 6.5 in/min. These two sequences are
equivalent.

#### G0

Rapid move. Ignores F setting. Note that G0 is modal ("sticky"). Any
lines specifying a coordinate after G0 will result in a G0 move even
if G0 is not explicitly stated.

Examples:
```
G0 X1 Y-0.5 Z-20
```
Rapid move to new position.
```
G0
X1 Y-0.5 Z-20
```
Set G0 mode, then rapid move to new coordinates with the already active G0.

#### G1

Move at feed rate set with F. Otherwise it's the same as G0. G1 is also modal.

Examples:
```
F6.5
G1 X1 Y-0.5 Z-10
```
Move to new position at the set feed rate.
```
G1 X2 Y-0.5 Z-13 F6.5
```
Move to new position at new feed rate.
```
G1 F6.5
X1 Y-0.5 Z-10
X5 F10
```
Set G1 mode and feed rate but don't move. Then move to the first
position at that feed rate. Then move to a new position at a new feed
rate.

#### G28

Home. If no axes are given, then all axes are homed at the same
time. (Some controllers may automatically home Z first.) If axes are
given, the given axes are homed at the same time. Any axes that are
specified must also have a number. When axes are specified with a
number, the position is changed to the coordinates specified, just as
with G0, then the specified axes are homed.

Usually G28 is used with G91 (relative positioning) if there are any
axis values included. It's much safer to use relative positioning here
than absolute positioning. However you will likely need to follow this
up with a G90 to go back to absolute positioning.

Home position is often the same as the reference position, and both
are often X0 Y0 Z0. This isn't always the case, though. The home
position may be an entirely different position on some machines.

Examples:
```
G28
```
Home all axes at once.
```
G28 X0 Y0
```
Home X and Y axes after moving to X0 Y0. Z isn't changed or homed.
```
G28 X0 Y0 Z5
```
First move to X0 Y0 Z5, then home X, Y, and Z.
```
G28 G91 X0 Y0 Z5
G90
```
First move 5 in Z from current position, then home all axes. THIS IS
THE PREFERRED SYNTAX. Just remember you may need to issue a G90
immediately afterward.

#### G30

Move to a predefined position. This might only move in the Z direction,
depending on machine settings.

#### G90

Set absolute positioning mode. The X, Y, and Z values that come after
this will be the actual position within the current coordinate
system. G90 is usually always used except in very specific
circumstances (like homing, see G28). In absolute positioning mode, G0
Z5 will move to the actual position Z5 (usually in work coordinates).

#### G91

Set relative positioning mode. The X, Y, and Z values will be
interpreted as relative to the current position instead of the actual
coordinate position. (G0 Z5 will move up 5 from current position.) Note
that it doesn't really matter which coordinate system is in use.

## Spindle

#### M3

Turn on the spindle in the "forward" direction.

#### M5

Turn off the spindle.

#### S?

Set spindle RPM.

Example:
```
S1000
```
## Tools

#### G43 H?

Set the tool length to the value stored in the given offset table
position. By convention, the table position number is the same as the
tool number. Usually this command is in or at least near the block
containing the tool change command.

Example:
```
G43 H293
```
Set the tool offset to the offset stored in table position 293 (by
convention this corresponds to tool 293).

You could do "T293 M6 G43 H7" if the tool length for tool 293 is
stored in table position 7, but don't do that. Just use H293 for T293.

#### M6

Change to the tool stored with T. With an autochanger, automatically
load the tool. Otherwise prompt the operator to change the tool.

#### T?

Set the tool number. This doesn't actually do anything except store
the number to be later used by M6.

## Program flow

#### M0

Pause execution and wait for the operator. If there is a comment on
this line, it is displayed on the console.

Example:
```
M00 (Hit "cycle start" when ready)
```
#### M1

Same as M0, but M1 pauses can be disabled on the console.

## Canned cycles

#### G80

End any current canned cycle.

#### G81 Z? R?

Simple drill (no pecking). Do the first drill at the current position.
Any coordinates on later blocks are new holes. The last specified feed
rate is used. Z sets the final Z position. Before the actual drilling
starts, there is a rapid move to the Z position specified by R.

Usually you will have the tool pretty far above where the hole will
be, because it just moved there from a different X and Y with good
clearance. The R allows a rapid move down towards the stock, so that
drilling (at a lower feed rate) can start much closer to the stock.

Example:
```
G98 F6.5
G0 Z1
G0 X1 Y-0.5
G81 Z-1 R0.2
G80
```
Set the option to return to original Z (see G98) and set feed rate.
Move above the stock 1 inch for clearance. Move to X1 Y-0.5. Do the
canned cycle: rapid move to Z0.2, move at 6.5 in/min to Z-1, return
to Z1 (via G98). End the canned cycles.

This is roughly equivalent to the above, without using a canned cycle:
```
F6.5
G0 Z1
G0 X1 Y-0.5
G0 Z0.2
G1 Z-1
G0 Z1
```
#### G83 Z? R? Q?

Peck drill. Very similar to G81. Q gives the depth of each peck.

Example:
```
G98 F6.5
G0 Z1
G0 X1 Y-0.5
G83 Z-1 R0.2 Q0.25
X3 Y-0.5
G80
```
Set the option to return to original Z (see G98) and set feed
rate. Move above the stock 1 inch for clearance. Move to X1 Y-0.5. Do
first canned cycle: rapid move to Z0.2, then peck to Z-1 by 0.25
increments, at feed rate 6.5, then return to Z1 (via G98). Then move
to X3 Y-0.5, do another peck drill cycle, and return again to
Z1. Finally end the canned cycles.

#### G98

Used before the canned cycles, this tells the machine to return the
tool, at the end of each cycle, to the Z position at the start of
the cycle. In the preceding G83 example, if Z is 1 at the start of the
canned cycle, the tool is returned to Z1 at the end of the cycle.

#### G99

Like G98, only return to the rapid Z position (specified by R). In the
preceding G83 example, it would return to Z0.2 at the end of each
cycle. This can reduce the amount of movement between cycles, as long
as this low Z still provides enough clearance for the X Y moves.

## Misc

#### G17

This sets the plane to use for G2 and G3 commands, which are arc moves,
to the XY plane. We don't actully need this if we don't use arcs. It's
in the boilerplate just to set the state.

#### G20

Use imperial units. Positions are in inches, feed rates are in inches
per minute.

#### G21

Use metric units. Positions are in mm, feed rates are in mm/min.

#### G40

Cancel any cutter radius compensation that was set with G41 or G42. We
don't actually use G41 or G42. This is in the boilerplate just to
clear the state.

#### G49

Cancel any tool length compensation. This is just in the boilerplate
to clear the state.

#### G64 P? Q?

This fine tunes some motion parameters. Basically it tells the system
how accurate moves need to be.
