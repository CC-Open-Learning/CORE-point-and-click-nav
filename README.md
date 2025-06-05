# Point & Click Navigation
![](https://img.shields.io/badge/2025--01--20-1.4.3-green)

This repository contains the source code and Unity package for the **Point & Click Navigation** system.
See developer [Confluence](https://github.com/CC-Open-Learning/CORE-confluence) documentation

## Point & Click Navigation Functionality

* Provides the ability to navigate between custom waypoints/point of interests (POIs) with the help of Unity's NavMesh and Cinemachine packages.


## Installation

### Package Manager
**Point & Click Navigation** can be found in the [CORE UPM Registry](http://upm.core.varlab.org:4873/) as `com.varlab.navigation.pointclick`.

Navigate to the **Package Manager** window in the Unity Editor and install the package under the "My Registries" sub-menu.


### Legacy Installation
In the `Packages/manifest.json` file of the Unity project project, add the following line to dependencies:

`"com.varlab.navigation.pointclick": "ssh://git@bitbucket.org/VARLab/point-and-click-navigation.git#upm"`

Optionally, replace `upm` with a branch name or label to track a specific package version.