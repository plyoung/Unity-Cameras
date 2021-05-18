# Unity Cameras

A repo where I'll be placing various camera controller scripts. These will probably all use the new input system but it should be easy enough to change the code to work with whatever you use for input.

## FreeMoveCamera.cs 

A Unity scene editor like camera. 

* WASD movement (holding the RightMouseButton).
* MiddelMouse or Ctrl+LeftMouse to pan.
* Alt+LeftMouse to rotate around pivot point. This pivot could be the position of a focused transform.
* ScrollWheel to zoom in/out.
* Method to focus on a transform (like the F key in Unity).
* Fast move (holding Shift button for example)

![Image of FreeMoveCamera](/Images/FreeMoveCam.webp)

## TopDownCamera.cs (Simpler)

View world/action from top at some angle. This is a simpler version of the TopDownCamera. See the non "_simpler" one for more.

* Freely move camera (with or without holding a button)
* Pan up/down/left/right (can disable by not binding related input)
* Rotate and Tilt (can disable these and limit tilt's min/max)
* Zoom
* Focus on object
* fast move (holding Shift button for example)

![Image of TopDownCamera](/Images/TopDownCam.webp)

## TopDownCamera.cs

View world/action from top at some angle. This one has smoothing for when you focus on an object or adjust the zoom level.

* Freely move camera (with or without holding a button)
* Pan up/down/left/right (can disable by not binding related input)
* Rotate and Tilt (can disable these and limit tilt's min/max)
* Zoom
* Focus on object
* Fast move or zoom (holding Shift button for example)
* Follow an object

![Image of TopDownCamera](/Images/TopDownCam2.webp)


