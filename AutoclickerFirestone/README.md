--- Requirements ---

0. Must set the display resolution to 2560x1440 !!!

1. Download the training data for Tesseract-OCR here:
https://github.com/tesseract-ocr/tessdata

2. Unzip and place training data in the folder C:/OCR

folder structure should look like: (you need all files, not only the ones listed here)
	C:/OCR/tessdata/script
	C:/OCR/tessdata/tessconfigs
	C:/OCR/tessdata/eng.traineddata
	...

3. Add environment variable to windows named 'TESSDATA_PREFIX', set the value to 'C:\OCR'

4. Start the application

5. Set your 'Define offset-x' parameter. 0 = main screen, -2560 = left screen, 2560 = right screen.

--- Info ---

How this application works:
It's a mix of OCR (Optical character recognition -> text) and image comparison (stock screenshots vs realtime screenshots).

When the program clicks somewhere in firestone, it's using the 'pixel' to get an X/Y coordinate and it will click there.
When the program needs to compare images or retrieve text, it will use an 'image'. Based on the image list two points (X,Y) are given.
The top left point of the rectangle and the bottom right point of the rectangle. 

All of these can be easily changed/created from within the application if needed.

Normally seen everything should be good to go without any changes. 
The only params that the user should tweak are the 'Pixels' starting with 'A_'.
The other ones should be fine as they are.

In order to update a pixel, select the pixel from the list, click the 'Update pixel' button, now hover your mouse to the desires location, press 'space'.
The pixel will now have the updated coordinates.

Settings are saved automatically when exiting the app. 

--- Known bugs ---
The 'Stop' button works, but when pressing 'Start' after having pressed 'Stop' issues occur.
For this reason when pressing stop the application is closed.