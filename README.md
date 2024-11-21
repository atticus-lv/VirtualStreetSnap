## VirtualStreetSnap

VirtualStreetSnap is a screenshot tool for save virtual street view.

Compare to other screenshot tool, VirtualStreetSnap provide a more camera-like experience.

![VirtualStreetSnap](docs/images/shot.png)

![ImageGallery](docs/images/gallery.png)

### Features

> v0.0.2

+ Radio button: select different size of screenshot.
    + 4:3 16:9 3:2 9:16 3:4 1:1

+ Overlays
    + red focus border for indicate the screenshot area.
    + guidelines for help align the screenshot area.
        + grid
        + center
        + ratio

+ Settings
  + on top: always on top.
  + file prefix: custom file prefix for saving screenshot.
  + save directory: custom save directory for saving screenshots.

+ Image gallery: review the screenshot.
    + right click to flip the image.
    + scroll to zoom the image.
    + middle/left click to drag the image.
    + right click thumbnail to delete/open in explorer.

### Download

[Releases](https://github.com/atticus-lv/VirtualStreetSnap/releases)

### Development

Environment

+ .net 8.0
+ C# 12.0
+ win10 x64


Build (default with aot)

```
dotnet publish
```