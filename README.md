# AsciiFx
[![Twitter](https://img.shields.io/badge/Follow-Twitter?logo=twitter&color=white)](https://twitter.com/NullTale)
[![Discord](https://img.shields.io/badge/Discord-Discord?logo=discord&color=white)](https://discord.gg/CkdQvtA5un)
[![Boosty](https://img.shields.io/badge/Support-Boosty?logo=boosty&color=white)](https://boosty.to/nulltale)

Variable Ascii effect for Unity Urp, controlled via volume profile </br>
Works as render feature or a pass for selective post processing [VolFx](https://github.com/NullTale/VolFx)

![_cover](https://github.com/NullTale/AsciiFx/assets/1497430/6f75fa3a-1772-47c0-a12a-ca2cf6fba22c)


## Features
* Animated signs
* Smooth interpolation
* Variable background and signs colorization
* Custom palettes

![_cover](https://github.com/NullTale/AsciiFx/assets/1497430/78fb6997-3ff1-40c0-bc32-866280090e61)

## Part of Artwork Project

* [Vhs](https://github.com/NullTale/VhsFx)
* [OldMovie](https://github.com/NullTale/OldMovieFx)
* [GradientMap](https://github.com/NullTale/GradientMapFilter)
* [ScreenOutline](https://github.com/NullTale/OutlineFilter)
* [ImageFlow](https://github.com/NullTale/FlowFx)
* [Pixelation](https://github.com/NullTale/PixelationFx)
* [Ascii]
* [Dither](https://github.com/NullTale/DitherFx)
* ...

## Usage
Install via Unity [PackageManager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)<br>
Add `AsciiFx` feature to the UrpRenderer, control via volume profile

```
https://github.com/NullTale/AsciiFx.git
```

Render feature contains a few global parameters</br>
to control default settings and animation noise resolution</br>

<img src="https://github.com/NullTale/AsciiFx/assets/1497430/d4b5d687-0664-449d-bd55-1fb83c1c2025" width="450"><br>

## Volume settings:
* Ascii color - signs color multiplyer interpolated by alpha beetween originial image color and defined in settings
* Image color - cell color multiplyer
* Gradient - custom signs gradient
* Depth - gradient height in case if used atimated signs atlas or for glitch effect
* Fps - animation frame rate, applied via screen space noise defined in render feature
* Platte - custom pallet for color replacement
* Impact - impact of the custom palette, interpolation between original image color and graded via palette

> One of the volumes from PackageManager Samples

<img src="https://github.com/NullTale/AsciiFx/assets/1497430/ab1c0fad-065c-4936-bbcc-aea7ff9122ea" width="770"><br>


