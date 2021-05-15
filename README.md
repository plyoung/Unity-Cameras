# Unity Cameras

A repo where I'll be placing various camera controller scripts. These will probably all use the new input system but it should be easy enough to change the code to work with whatever you use for input.

## FreeMoveCamera.cs 

A Unity scene editor like camera. 

* WASD movement (holding the RightMouseButton).
* MiddelMouse or Ctrl+LeftMouse to pan.
* Alt+LeftMouse to rotate around pivot point. This pivot could be the position of a focused transform.
* ScrollWheel to zoom in/out.
* Method to focus on a transform (like the F key in Unity).

![Image of FreeMoveCamera](/Images/FreeMoveCam.webp)

## TopDownCamera.cs

View world/action from top at some angle.

* Frteely move camera (with or without holding a button)
* Pan up/down/left/right (can disable by not binding related input)
* Rotate and Tilt (can disable these and limit tilt's min/max)
* Zoom
* Focus on object

TODO: Follow a transform.

![Image of FreeMoveCamera](/Images/TopDownCam.webp)


