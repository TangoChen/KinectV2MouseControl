## Kinect v2 Mouse Control

##### Updates:
	Sept 13, 2022
	Modified for usability in seated position for flight simulators
	
	Added 2 additional ergonomic modes for cursor movement in seated position:
		wrist relative to shoulder to reduce the number of joints involved
		hand tip relative to shoulder which is more intuitive
	Integrated unscented kalman filter from https://github.com/prozoroff/UKFSharp to smooth cursor movement and reduce jitter when still
	
	Click via pinching thumb and pointer finger (to reduce cusror jitter when clicking) where:
		left mouse click is palm facing down (thumb is horizontal to hand)
		right click is palm facing sideways (thumb is above hand)
		middle click is palm facing diagonal
		
	Separate mouse down/mouse up via grabbing with thumb pointed outwards:
		left mouse down is palm facing down (thumb is horizontal to hand)
		right mouse down is palm facing sideways (thumb is above hand)
		middle mouse down is palm facing diagonal
		
	Dead zone is measured from base of neck to be visible in a seated position.
	The dead zone units are in terms of forearm length (elbow to wrist) which normally will not require calibration	
	The default is set to allow a joystick and throttle to be placed on the armrests
	
	A universal stop gesture (double chop with both arms crossed at the wrist in an X) will stop the hand tracking
	
	It will also preserve its minimized stated between shutdown and startup

  * ***v1.2.1***  
  	May 4th, 2018: Lock control with first tracked person so as to avoid cursor being influenced by other people.
  * ***v1.2***  
  	Apr 10th, 2018: Added two-hand controls:
	 * Move + grip pressing.
	 * Move + lift clicking.

	I personlly don't think these control functions are intuitive or good-looking gestures. But they kind of solve cursor jittering when doing a grip gesture, in a quick doable way. Still fine if you are okay with them for your scenarios.
  * ***v1.1***  
  	Feb 21th, 2018: An update after 3 years since first created. On usages and the looking, the app doesn't seem very different. Features are nearly just the same, but with a big structure change on the code side using MVVM pattern.  
    A small improvement on feature is, the cursor will keep following first tracked (lifting-forward) hand regardless of another hand lifting afterwards. Previously, it was set to only follow right hand when both hand are tracked.
---
### Features

#### Control mode options
* Disabled
* Move only
* Grip to press
* Hover to click
* *Move + grip pressing* \*
* *Move + lift clicking* \*

*\* P.s. 2-hand control (One hand for moving cursor, another for mouse button control)*


#### Adjustable parameters
* Movement scale
* Cursor smoothing
* Hover-to-click range
* Hover-to-click duration
---
Some more descriptions what some classes do:
#### KinectReader.cs
* Turns on/off Kinect sensor.
* Reads data from sensor.
* Raises OnTrackedBody, OnLostTracking events.
#### CursorMapper.cs
* Maps positions from input rect to output rect. Specifically in this case:
  * Input rect: Hand gesture moving area.
  * Output rect: Screen area.
* Gets position for smoothed cursor movement.
* Scale alignment available for deciding how movements are scaled from input to output rect with an adjusted proportion. So as to make desired and suitable cursor movements on specific area sizes. There's no visual settings on the current app. And it's set to LongerRange by default in code.

	e.g. If hand gesture moving area is (0, 0, 100, 100), screen area is (0, 0, 800, 600).
    
  * **None.** Scale: (1, 1)
  * **Both.** Scale: (8, 6)
  * **Horizontal.** Scale: (8, 8)
  * **Vertical.** Scale: (6, 6)
  * **ShorterRange.** Scale: (6, 6)
  * **LongerRange.** Scale: (8, 8)


#### MapperStructs.cs:
***MRect*** and ***MVector2*** included in **MapperStructs.cs** are custom Rect and Vector2 struct with an 'M' in the front just to make it more distinguishable from possibly existing namesake structs.

***MRect*** has DeltaX, DeltaY, Width, Height, Center properties that can be used for calculating scale and offset for mapping.
