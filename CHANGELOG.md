# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]


## [0.16.1] - 2024-02-22
Playtest & Pancakes 2024 build.

### Added

- Added cutscenes.
- Added pages.


## [0.16.0] - 2024-01-19
Deutscher Computerspielpreis 2024 build.

### Added

- Added Robbyn.
- Added Printer.
- Added cutscenes.


## [0.15.0] - 2023-10-27
GameCamp Munich 2023 build.

### Added

- Added City.


## [0.14.1] - 2023-06-26
Dokomi 2023 build.

### Added

- Added camera movement via right stick.
- Added maps to the sewers.
- Added FFF credits.

### Changed

- Changed menus to use pixel art.


## [0.14.0] - 2023-05-30
Milestone 14 build.

### Added

- Added birds that can give you a boost to your flight.
- Added lights to various Sewer objects.
- Added a dialog system.

### Fixed

- Fixed pause menu sometimes not disabling/enabling avatar input properly.
- Fixed Maya's color palette.

### Changed
- Rewrote codebase and divided it into modules.
- Changed the end of the Sewers to lead straight to the credits.


## [0.13.0] - 2022-10-22
We welcomed our new audio director, Nic, into our project and had him redo most audio assets.

### Changed
- Replaced the background music.

## [0.12.2] - 2022-09-29

### Added
- Added screenshake effect.

### Fixed
- Fixed Maya sometimes clipping through ledges.

## [0.12.1] - 2022-09-28

### Changed
- Tweaked the sewers level.

### Fixed
- Fixed a bug preventing loading after dying.

## [0.12.0] - 2022-09-24
We mainly reworked the main menu, replacing the Mario Bros. level pan with a static image of Maya.

### Changed
- Replaced the main menu background.

## [0.11.0] - 2022-05-07
We polished the sewers a lot, adding lights and decor elements.

We also said goodbye to our composer, Max, and wish him well on his journey to Cologne.

## [0.10.0] - 2021-11-24
We overhauled the game world, replacing the cave with sewers and the meadows with a city.

## [0.9.0] - 2021-10-23
This milestone focused on lots of changes to the backend that will make further development easier.

First was, of course, a fix to the WebGL version of the game which broke v0.8.0.

We  also rewrote the character controller from scratch to get a more granular control of when all the good game juice stuff happens: Animations, sounds, rumble, particles, camera movement, and of course the movement of the avatar herself.

![v090](~/docs/devlog/v090.png)
*The new character controller uses Unity's Timeline to allow for frame-perfect tuning of effects.*

## [0.8.0] - 2021-03-27
Our composer asked for a high-performance build of the game to use in an arcade he was working on. So, we added a graphics quality menu and used nifty multithreading to make the game run much faster.

Sadly, this broke the browser build.

![v080](~/docs/devlog/v080.png)
*If you're ever in Bayreuth, come check out Cursed Broom on this here arcade machine!*

## [0.7.0] - 2020-10-31
For the final milestone, lots of polish was applied to the level design, the sound effects and the character controller.

![v070](~/docs/devlog/v070.png)
*Now with a proper main menu!*

Play [Cursed Broom v0.7.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.7.0/).

## [0.6.0] - 2020-10-03
This milestone focused on improving the tilemap performance, polishing the particle systems, and adding the start of the 2nd level.

![v060](~/docs/devlog/v060.png)
*Completing the tutorial rewards you with some pretty flowers.*

Play [Cursed Broom v0.6.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.6.0/).

## [0.5.0] - 2020-07-23
Mechanically, we added a wall-jump and a dash, both available only while flying.

Level design wise, we completed a draft of the tutorial level, and decided for the game structure to be about a vertical climb out of a cave and into the clouds.
					
![v050](~/docs/devlog/v050.png)
*The tutorial is all about emerging from darkness.*

Play [Cursed Broom v0.5.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.5.0/).

## [0.4.0] - 2020-06-18
This milestone focused on making the ground/jump movement snappy, intuitive and fun.

For this, the character controllers of Celeste's Madeline and Super Mario World's Mario were studied and approximated. Turns are now instant, even in mid-air, and there's some coyote time when walking off of platforms.

There's also many more visuals and, finally, sound effects. And collectibles!
					
![v040](~/docs/devlog/v040.png)
*Brace for impact!*

Play [Cursed Broom v0.4.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.4.0/).

## [0.3.0] - 2020-06-04
This milestone iterated the flight controls and tried to make the current avatar momentum matter a lot while mid-air.

Sadly, the reliance on momentum made Maya very floaty.

On the aesthetic side we added a lot of animations and visual effects, which improved game feel a lot!
					
![v030](~/docs/devlog/v030.png)
*Ground movement aside, you can fly some nice sinusoidal curves.*

Play [Cursed Broom v0.3.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.3.0/).

## [0.2.0] - 2020-05-28
At this point Tina joined the project and brought a wonderful narration of a witch with a slightly malfunctioning broom.

Gameplay introduced rotation during gliding, and a kinetic energy/potential energy translation similar to Mario's wing cap flight in Super Mario 64.
					
![v020](~/docs/devlog/v020.png)
*Also, particle systems.*

Play [Cursed Broom v0.2.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.2.0/).

## [0.1.0] - 2020-05-14
The first prototype is the barest-bone implementation of a gliding ability. It's inspired by Knuckles' glide in Sonic 3 and Mario's dive jump in Super Mario 64.
					
![v010](~/docs/devlog/v010.png)
*In retrospect, binding both jump and glide to "any key" might have been a mistake (works well with a gamepad though).*

Play [Cursed Broom v0.1.0](http://daniel-schulz.slothsoft.net/Builds/TheCursedBroom/0.1.0/).