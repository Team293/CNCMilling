# CNCMilling

## Introduction

This repository contains some scripts and associated documents
for creating G-code to automate machining using Team 293's CNC
machines.

Whenever you make changes that might affect the G-code, be sure to
always run the output through a simulator and carefully check each
step. Make sure the tool length gets set and the spindle is turning
when (and only when) it should be.

Also, try not to have too many different versions floating around.

## Files

### 8020CodeGen.html

This script is intended to make generation of common 8020 extrusion
parts simple and quick using a common fixture. There are three
operations currently supported by this script ("Bore and Score",
"Cut-off Lengths", and "Daily Check-up"). These operations are
combined into the same script to ensure that important parameters
(such as the work origin) are always in sync.

### Tormach.md

To help understand what the G-code is doing, I've compiled information
on the codes we use (and some associated ones) with the Tormach. Other
machines may need speparate documentation, since they may behave
somewhat differently.

This file also has basic information on using the PathPilot simulator.
