## Requirements

**Important:** Must set the display resolution to **2560x1440**!

1. **Download the training data for Tesseract-OCR:**

   Download it [here](https://github.com/tesseract-ocr/tessdata).

2. **Unzip and place the training data:**

   Extract the files and place them in the folder `C:/OCR`. The folder structure should look like this (you need all files, not just the ones listed):

C:/OCR/tessdata/script
C:/OCR/tessdata/tessconfigs
C:/OCR/tessdata/eng.traineddata
...

3. **Add environment variable:**

Add an environment variable to Windows named `TESSDATA_PREFIX`, and set the value to `C:\OCR`.

4. **Start the application.**

5. **Set the 'Define offset-x' parameter:**

- `0` = main screen
- `-2560` = left screen
- `2560` = right screen

## Info

### How this application works:

This application is a mix of **OCR (Optical Character Recognition)** and **image comparison** (comparing stock screenshots with real-time screenshots).

- **Clicking in Firestone:** The program uses a 'pixel' to get an X/Y coordinate and clicks there.
- **Image comparison and text retrieval:** The program uses an 'image' to determine two points (X,Y), which define a rectangle (top-left and bottom-right corners).

All of these settings can be easily changed or created from within the application if needed.

### Default Setup:

Normally, everything should be good to go without any changes. The only parameters that the user should tweak are the 'Pixels' starting with `A_`. The other ones should be fine as they are.

### Updating a Pixel:

1. Select the pixel from the list.
2. Click the 'Update pixel' button.
3. Hover your mouse to the desired location and press `space`.
4. The pixel will now have the updated coordinates.

**Note:** Settings are saved automatically when exiting the app.

## Known Bugs

- The 'Stop' button works, but pressing 'Start' after pressing 'Stop' causes issues. To avoid this, the application will close when 'Stop' is pressed.

![screenshot](https://github.com/user-attachments/assets/3881b291-94a6-4cfa-a526-5dd4be38a904)

