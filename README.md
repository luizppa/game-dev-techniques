# Game Development Techniques

Hi! My name is Luiz, I am an aspring game developer and this is a repository where I will be putting the projects I will create throughout my journey. I will be trying to roughly follow [this roadmap](https://github.com/utilForever/game-developer-roadmap) by [Chris Ohk](https://github.com/utilForever). Many of the projects you will see here are inspired by a few of my favorite content creators, I absolutely recommend you to check them out:

* [Freya Holmér](https://www.youtube.com/c/Acegikmo)
* [Sebastian Lague](https://www.youtube.com/c/SebastianLague)
* [Stylized Station](https://www.youtube.com/c/StylizedStation)

I would also recommend you to check out this other creators who does not (necessarily) inspired the content of this repository, but are a solid option to learn about game art, design, optimization, marketing and development in general:

* [Game Maker's Toolkit](https://www.youtube.com/c/MarkBrownGMT)
* [Ask Gamedev](https://www.youtube.com/c/AskGamedev)
* [Acerola](https://www.youtube.com/c/Acerola_t)
* [Daryl Talks Games](https://www.youtube.com/c/DarylTalksGames)
* [Dani](https://www.youtube.com/c/DaniDev)

Whithout further ado, let's get to the projects!

## Marching Cubes

<p align="center">
  <img src="Docs\marching-cubes-screen-title.png"/>
</p>

[Marching Cubes](https://en.wikipedia.org/wiki/Marching_cubes) is a procedural polygonization algorithm that will generate natural looking meshes based on a grid of points. It is a very popular technique used in many applications, such as terrain generation, fluid simulation, voxel rendering and many others. The technique was first described by William E. Lorensen and H. E. Cline in 1987.

The algorithm works by creating a tridimensional grid of points, where each point has a value (often reffered at as "density") that indicates wheter the points is located at the interior or the exterior of the mesh. The code slides (marches) a cube through the grid and creates polygons by interpolition of the position of adjacent points. The polygons are then connected to form a mesh.

<p align="center">
  <img src="Docs\marching_cube.png"/>
  <br>
  Image from <a href="http://shamshad-npti.github.io/implicit/curve/2016/01/10/Marching-Cube/">Shamshad Alam's blog</a>
</p>

There are 256 possible formations for a cube (however, some of them are symmetrical, therefore are redundant and can be reduced to 15 unique formations). Each formation is represented by a 16-bit integer, where each bit indicates wheter a vertex is inside or outside the mesh.

### Implementation

My implementation of this algorithm is based on [this video by Sebastian League](https://youtu.be/M3iI2l0ltbE). His implementation used compute shaders to make the mesh generation parallel, thus, giving him the possibilty to have more polygons beeing generated without heavy impact on performance. For now, my implementation is written in C# and runs on the CPU, so it is not parallelized, but I plan to do so in the future. I tried not to look at his code, which is also available on Github in order not to be biased in any way during may implementation.

### Result

You can check out a video of the result [here](https://www.youtube.com/watch?v=SCsOzZVZ7ic)

<p align="center">
  <img width="45%" src="Docs\marching-cubes-screen-capture-2.png"/>
  <img width="45%" src="Docs\marching-cubes-screen-capture-3.png"/>
  <img width="45%" src="Docs\marching-cubes-screen-capture-4.png"/>
  <img width="45%" src="Docs\marching-cubes-screen-capture-5.png"/>
</p>

### Credits

Learining resources:
* [Polygonising a scalar field](http://paulbourke.net/geometry/polygonise/)
* [Generating Complex Procedural Terrains Using the GPU](https://developer.nvidia.com/gpugems/gpugems3/part-i-geometry/chapter-1-generating-complex-procedural-terrains-using-gpu)
* [Marching Cubes: a High Resolution 3D Surface Costruction Algorithm](https://people.eecs.berkeley.edu/~jrs/meshpapers/LorensenCline.pdf) (the original papper for the algorithm)

Assets:
* Music
  * DSTechnician - [Gateway](https://pixabay.com/music/ambient-gateway-110018/)
* Sound effects
  * Fission9 - [Underwater Ambience](https://pixabay.com/sound-effects/underwater-ambience-6201/)
