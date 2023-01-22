## Introduction

This script is deliberately kept simple. It is as self-contained
as possible, using no external javascript libraries, and the one
html file contains everything needed.

Keeping it simple makes it easy to learn, easy to maintain, easy to
copy, and easy to use in a variety of environments. I think it's also
useful to show that you can make an interactive form pretty easily
with just a little html and js.

This is of course also very limiting. There is no fancy page layout,
no images, no persistent data (no saving and loading of parameters or
preferences, no databases of common parts, and so on). One could
certainly dedicate a web server to provide much, much more complex
services and better looking pages, if one were to choose that route.

One goal here was to keep all the essential functions in one file that
can be easily loaded into a browser from a web server, personal
desktop, USB drive, or whatever.

## Operations

### Bore and Score

In OnShape, custom featurescript creates the holes in the extrusions
and exports a custom table. The holes can be simple drills or
counterbore and drill. This table can then be copied and pasted into
the Bore and Score form. From this information, the script generates
G-code to create the holes using the shop Tormach and the fixture
designed for this purpose.

In addition, two score marks can be machined at X = 0 and X = stock length
(minus or plus the tool radius). The part can then be cut at these
marks (along the appropriate edge depending on which end) in the chop saw.

### Cut-off Lengths

This operation does not require CAD input. The form requests a number
of lengths to cut, and the lengths of those lengths, and generates
G-code to create scoring marks which can then be used at the chop
saw. For example, one could enter a length of 3 and a number of 4, and
it would make scoring marks for cutting out four 3-inch lengths of
extrusion.

### Daily Calibration

Since the mill is in a shared shop environment, it's probably
wise to have some safe way of verifying the setup at the start of each
shop session. This would ideally check the WCS origin and tool lengths.

We now have a simple procedure for doing this though one of the forms.
The G-code that is produced loads each tool, in turn, positioning the
business end at Z3 (3 inches above the stock). With 1x1 stock loaded,
a 1-2-3 block can be used to verify the tool is at Z3 (or, ideally,
just very slightly above). If the block's edges are flush with the
stock, then the tool should also be centered on the middle hole.

## The Fixture

A jig in the Tormach allows for different offset stops in the -X direction.
This allows for longer extrusions to be machined. The operator is
prompted to move the stock between stops. The stops are used in order
of increasing distance from the vice, but of course some stops may
not be used. If there is no tool changer, or if the tool is not in the
changer, the Tormach will also prompt the operator to change the tool.

A brief overview of the machine table configuration:
<pre>
                                        (v vice v)
                                        ----------
 (X0 is 0.5 inch to right of stop 1 v)  |        |
                       #           #    ---------- (<- Y0 on inside of fixed jaw)
                       #           #(---workpiece--------------------)
                       #           #    ----------
               (stop 2 ^)  (stop 1 ^)   |        |
                                        ----------
</pre>
The X0 and Y0 refer to the work coordinate system origin (not the
machine coordinate system). Z0 will be the top of the stock in the
WCS.

NOTE: X0 is 1/2" to the right (+X direction) of the first stop. This
is important, since scoring operations will be offset by the tool
radius. For example, if the tool radius is 0.375, the first score will
be at X-0.1875, and the left side of the score will be at
X-0.375. This 1/2" offset applies to the other stops as well.

BEWARE: Keep the tool numbers consistent with the machine.
The machine MUST account for tool lengths.

BEWARE: Keep the WCS X, Y, and Z offsets consistent with the fixture.
Keep the X stop offset values consistent with the fixture.

BEWARE: Program in appropriate speeds and feeds, which depend on the
tools used and the stock.

## Details

### Bore and Score

In OnShape, custom featurescript creates the holes in the extrusions
and exports a custom table with output like this:
<pre>
  (Part1)
  (Face1)
  (Counterbore)
  0
  0
  20
  1
  X0.5Y-0.5
  X13Y-0.5
  ...etc...
</pre>
These are, in order: Part name, face name, hole type, offset X, offset Y,
extrusion length, extrusion height, then a list of hole centers.

Currently offset X and offset Y are not used.

Different stock heights are supported for Bore and Score (only). The
CAD must report the correct height. The WCSOffsetZ is adjusted by the
stock height. Note that the height will be different for (for example)
1020 extrusion for the sides vs the top. The CAD must report this
correctly.

Drills go all the way through the stock. Counterbores are via plunge
with a square endmill (which may also be used for the scoring), cut to
a fixed depth which is defined in the "params" dictionary
below. Drills are preceded by a centering tool to prevent bit wandering.

In addition, two score marks can be machined at X = 0 and X = stock length
(minus or plus the tool radius). The part can then be cut at these
marks (along the appropriate edge depending on which end) in the chop saw.

Limitations:

  1) The max width supported for the stock is 3 inches. There is a
  hardcoded value for the end Y in ScoreGC of -3.5. This could safely be
  increased some, but, since it's a hardcoded value (and we are not
  given the stock width), the scoring cut will always be that value,
  which wastes time for thinner stock.

  2) Any number of holes can be specified, but only one *hole type* is possible
  for each of these tables. Currently only "Drill" and "Counterbore" are
  supported.

  3) Scoring depth is hardcoded.

  4) Counterbore depth is also hardcoded. That is, the counterbore will
  always be cut to the same depth below the top of the stock. This
  could be more flexible if we get the counterbore depth from CAD.
  
  5) Tool paths are not very well optimized.

The general order in the G-code goes as follows for Bore and Score:

For each offset, start with scoring, then counterbores, then holes,
then move stock. Repeat for new offset. Usually this will result in:
left score, counterbores for first offset, drills for first offset,
move stock, counterbores for second offset, drills for second offset,
move stock, etc. Then for the last offset: right score, counterbores,
then drills.
<pre>
  Job description comments
  HeaderGC
  SetWCSGC (and adjust WCSOffsetZ by StockHeight)
  PostInitGC (may include moving spindle to neutral location)
  Pause and move stock to 0 offset (this is always done for clarity)
  For each offset:
    Do scoring:
      Change to scoring tool if needed
      Set spindle speed (if needed) and turn on spindle
      SafeRetractZ (may move down to SafeRetractZ from G30 height)
      ScoreStartGC (with X adjusted by tool radius)
      ScoreGC
      ScoreEndGC
      SafeRetractZ
    Do counterbores:
      Change to cb tool if needed
      Set spindle speed and turn on spindle if needed
      SafeRetractZ (may move down to SafeRetractZ from G30 height)
      CounterboreCannedStartGC
      For each hole:
        X and Y for canned operation (X adjusted for stock offset)
        CounterboreCannedOpGC
      CannedEndGC
      SafeRetractZ
    Do centering of counterbored holes:
      Change to centering tool if needed
      Set spindle speed (if needed) and turn on spindle
      SafeRetractZ
      CounterboredCenterdrillCannedStartGC
      For each hole:
        X and Y for canned operation (X adjusted for stock offset)
	CounterboredCenterdrillCannedOpGC
      CannedEndGC
      SafeRetractZ
    Do drills of counterbored holes:
      Change to drill tool if needed
      Set spindle speed and turn on spindle if needed
      SafeRetractZ (may move down to SafeRetractZ from G30 height)
      CounterboredDrillCannedStartGC
      For each hole:
        X and Y for canned operation (X adjusted for stock offset)
        CounterboredDrillCannedOpGC
      CannedEndGC
      SafeRetractZ
  FooterGC
</pre>
### Cut-off Lengths

This operation does not require CAD input. The form requests a number
of lengths to cut, and the lengths of those lengths, and generates
G-code to create scoring marks which can then be used at the chop
saw. For example, one could enter a length of 3 and a number of 4, and
it would make scoring marks for cutting out four 3-inch lengths of
extrusion.

Some waste is generated. In order to facilitate cutting, there is a
separation of 1/2 inch between lengths. So, if cutting 2 inch lenghts,
the first score will be at 0 - tool radius (so the right edge is the
edge to chop at), the second at 2 + tool radius, the third at 2.5 -
tool radius, and the forth at 4.5 + tool radius. 

Limitations:

  1) Only 1 inch high stock is supported. Stock of greater height could be
  supported. It would require adjusting the work coordinate system
  Z. This is just a little dangerous to leave as a form input.
	   
  2) The max width supported for the stock is 2 inches. There is a
  hardcoded value for the end Y in ScoreGC of 2.5. This could safely be
  increased some, but, since it's a hardcoded value (and we are not
  given the stock width), the scoring cut will always be that value,
  which wastes time for thinner stock.

  3) Scoring depth is hardcoded.

  4) Distance between the end score of one piece and the begin score
  of the next piece is hardcoded at 0.5 inches.

There are no stock offsets (yet) for cut-offs. I think this was intentional.

The general order in the G-code goes as follows for Cut-off:
<pre>
  Job description comments
  HeaderGC
  SetWCSGC (and adjust WCSOffsetZ by CutoffStockHeight)
  PostInitGC (may include moving spindle to neutral location)
  For each cut-off length:
    Do left scoring:
      Change to scoring tool if needed
      Set spindle speed and turn on spindle if needed
      SafeRetractZ (may move down to SafeRetractZ from G30 height)
      ScoreStartGC (X = n * (length + separation) - tool radius)
      ScoreGC
      ScoreEndGC
    Do right scoring:
      Change to scoring tool if needed (will not change)
      Set spindle speed and turn on spindle if needed (will not change)
      SafeRetractZ
      ScoreStartGC (X = left X + length + tool radius)
      ScoreGC
      ScoreEndGC
  FooterGC
</pre>
### Daily Calibration

  This does not require any input. Clicking "Generate" will return
  a simple script to perform the "calibration" steps. This process
  is already described pretty well above.
 
## Design Principles

There are a few design principles that you should be aware of. Please
note that these plans will result in some extra, seemingly unnecessary
G-code and machine movements. But see #8.

1) The operator is prompted on the screen to move the stock or change
   tools (as needed).

2) Whenever the operator is to interact with the machine or setup
   (change tool, move stock, perform tool length checks, etc.), the
   spindle will retract to G30 position and be turned off.

3) Before and after each cutting operation, the Z moves to the
   SafeRetractZ position. It is harmless to do it multiple times in a row.
   Especially with canned cycles, tool changes, G30s, and other things
   going on, it might just be wise to do this and do it often.

4) Whenever the tool is in proximity to the stock (within a few
   inches), the spindle should be on (except if this is necessary for
   preflight checks). The purpose of this is to allow the operator a
   chance to hit the E-stop if the spindle is not turning when it
   prepares for a cut. (This has been observed on our mill. Even if we
   work out the cause, it's still probably good practice.) To
   accomplish this, a G30 is performed whenever the spindle is turned
   on or the speed changes. Yes this makes for some extra movement.

5) The spindle speed is checked at the start of each cutting
   operation. This way we can safely turn off the spindle for various
   functions preceding the cut and easily manage multiple possible
   states without burdening the code.

6) At the start of each shop session, and as often as one whilt, the
   Daily Check-up operation should be done to ensure WCS offsets are
   correct (and possibly other things). In a shared shop environment,
   the saved coordinates could be easily changed. Part of this process
   could be to reset tool lengths using a tool setter.

7) If tool diameters, spindle speeds, or feed rates are changed, this
   code must be updated within the "Tools", "*SpindleSpeed",
   "*FeedRate" parameters in the machine dictionary. Tool diameter is
   used for position checks and offsets for scoring. Speed and feed
   must be appropriate for the operation and the tool (and the
   material, though we only expect one aluminum alloy here).

8) Operator management is every bit as important as machine
   management. Though it may result in some inefficiency in machine
   operation, some things are done with operator management in
   mind. For example:

   a) Giving the operator time to detect and react to unfavorable
   machine states, such as the spindle moving into a cutting position
   without the spindle running (#4).

   b) Making prompts for different interactions (tool changes vs
   moving the stock) easily distinguishable.

9) Prioritize safety over efficiency. Perform sanity checks, bounds
   checks, and whatever other checks are useful, as often as they are
   useful.

## Note for developers new to javascript

I'm including this because it's a common pitfall, even for developers
already familiar with javascript, and something that can drastically
affect the output (and break your machine).

Be sure your expressions are in the right context. Javascript has implicit
type conversion. For example, expressions involving '+', in particular,
can be interpreted either as string concatenation or numeric addition. You may
need to explictly convert to a number (usually you don't need to explicitly
convert to a string, except maybe for dictionary keys). For example,
<pre>
   var r = getEl("SomeFormInputID").value;
   var x = 12 + r;
   gc = "G0 X" + x;
</pre>
With the value of the form input SomeFormInputID being 13, for
example, this will result in gc = "G0 X1213", which is likely not
correct. This happens because the form input value is typically
(always?) a string, so x is assigned "12" + "13" which is "1213".
If there is any string in your expression, javascript will convert all
numbers into strings, then do string concatenation. Even though I'm
aware of this, I still get foiled by it occasionally.

This will produce the intended result:
<pre>
   var r = Number(getEl("SomeFormInputID").value);
   var x = 12 + r;
   gc = "G0 X" + x;
</pre>
Then gc = "G0 X25". I believe it is good practice to do the type
conversion when first assigning to a variable. In this example, we all
understand r as a radius, which is a number. There is no reason for r
to ever be defined as a string (though it may need to be converted to
one in some contexts).

Also, just in case you are expecting javascript to be like java:
"Java is to javascript as car is to carpet."

## Specific areas for improvement

Should anyone feel up to it, I have some ideas for improving the code
and (in some cases) operation. None of these are particularly important.
Though #2 might be. Feel free to add other things to this list.

1. In the machine definition, for tools, have a reference to a defined
tool type. For example: "4 flute HSS 3/8" end mill". Then have feeds
and speeds for operations under that tool definition, including
slotting and plunging at a minimum. For boring, scoring, etc., have
tool number and type of operation. Then look up speeds and feeds by
tool number, tool type, and operation type. Other variations on this
could be good, too. It would just be a little slicker, allow
different tools to be swapped in easily (swap out the aforementioned end mill
for a 2 flute carbide 3/8" end mill, for example), and also serve
as a database for the all-important speeds and feeds for different tools,
which could save considerable time. Those numbers could also just be
stored in comments, but that's not quite as slick.

2. Optimize stop usage. In particular, avoid operations on long parts
in areas that stick out without any support. The best example is a piece
that is not quite long enough to reach the right support. The current
version may start this piece at the first left stop and have it stick out
quite a lot from the vice. It would flex and vibrate more than it needs to
when machining that far end. This part could instead be moved to a different
stop to the left and held in the vice without much stickout, for those operations.
It shouldn't be too hard to add, since the code simply creates jobs and
assigns each of them an offset. So one could detect these jobs and add them
to the most appropriate offset number.

3. Optimize movement. There are some extra moves. Some of them are
artifacts of algorithmic generation: "I always want to be sure to end
every operation at a safe Z."; then, "I always want to be sure we are at
a safe Z before rapid moves in the X-Y plane." Just to illustrate what
I mean by that. There might be ways to reduce those. Some extra movements
(via G30 mainly) were introduced when we had problems with the spindle sometimes not
spinning. Because of that, it is designed to start the spindle
spinning while away from the stock (potentially far above it)
before moving towards the stock. We don't seem to have that problem now.
The more conventional approach would be to move the tool to a good
clearance Z, say 1" above the stock, then start it spinning. It would
also not move far above the stock just to turn the spindle
on.
