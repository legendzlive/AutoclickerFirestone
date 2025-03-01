using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;
using Microsoft.VisualBasic;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using System.Collections.Generic;
using OpenCvSharp;
using static System.Net.Mime.MediaTypeNames;

namespace AutoclickerFirestone
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        AutoClicker myAutoClicker = new AutoClicker();

        private readonly System.Windows.Forms.Timer clickTimer = new System.Windows.Forms.Timer();

        static int offsetX = 0;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            new ToolTip().SetToolTip(labelOffsetX, "When your game does not run on your main (center) screen, use this value to offset all the click actions. The application only supports resolution 2560 x 1440. Which means possible X-offset values are -2560 (left screen), 0 (center screen) and 2560 (right screen).");
            new ToolTip().SetToolTip(LabelTotalSquads, "You can view your squad count from the map screen. Press 'M' when ingame to open the map. Squad count is on top of the screen near the center. OCR is unreliable for getting the total squad count which is why this is a manual setting.");
            new ToolTip().SetToolTip(LabelUpgradeMode, "'Upgrade Max' upgrades your heroes to the maximum amount possible. 'Next milestone' upgrades to the next tier. This can be usefull in combination with the setting 'Switch to primary hero after stage'.");
            new ToolTip().SetToolTip(LabelSwitchToPrimaryHeroAfterStage, "After the game has reached the specified stage only your primary hero will be upgraded. If the current stage is unreadable by OCR all heroes will be upgraded.");
            new ToolTip().SetToolTip(LabelTrainGuardian, "Select which guardian will be trained.");
            new ToolTip().SetToolTip(CheckboxUseStrangeDust, "Specify if it's allowed to use 'Strange dust' to level your guardian.");
            new ToolTip().SetToolTip(LabelAutoclickDuration, "Specify in seconds how long the autoclicker task should run. Note: all selected tasks will be looped infinitely.");

            new ToolTip().SetToolTip(cbEnableResearch1, "Specify if your first firestone research slot is enabled. When enabled the selected tree upgrade will be leveled.");
            new ToolTip().SetToolTip(cbEnableResearch2, "Specify if your second firestone research slot is enabled. When enabled the selected tree upgrade will be leveled.");

            new ToolTip().SetToolTip(LabelResearchScroll1, "Specify the scroll amount for scroll action 1 when opening the firestone research screen. 70 is a decent base value to experiment with. Use the 'Test research scroll' button to try it out. Use '0' to not scroll at all.");
            new ToolTip().SetToolTip(LabelResearchScroll2, "Specify the scroll amount for scroll action 2 when opening the firestone research screen. 70 is a decent base value to experiment with. Use the 'Test research scroll' button to try it out. Use '0' to not scroll at all.");

            new ToolTip().SetToolTip(cbScrollLeft1, "Select this checkbox if you want to scroll to the left side instead of right. This is applicable to scroll action 1.");
            new ToolTip().SetToolTip(cbScrollLeft2, "Select this checkbox if you want to scroll to the left side instead of right. This is applicable to scroll action 2.");

            new ToolTip().SetToolTip(ButtonTestResearchScroll, "Make sure you are inside the firestone research screen before pressing the button.");
            new ToolTip().SetToolTip(ButtonTestMapScroll, "Make sure you are inside the map mission screen before pressing the button.");

            new ToolTip().SetToolTip(LabelCurrentStage, "Displays the current stage and the maximum stage. (refreshes regularly)");

            new ToolTip().SetToolTip(LabelAlchemist, "Specify which resources are allowed for alchemist upgrades.");

            ComboboxGuardian.Items.Add("Grace (Fairy)");
            ComboboxGuardian.Items.Add("Vermillion (Dragon)");
            ComboboxGuardian.Items.Add("Ankaa (Phoenix)");
            ComboboxGuardian.Items.Add("Azhar (Jinn)");

            ComboboxUpgradeMode.Items.Add("Upgrade Max");
            ComboboxUpgradeMode.Items.Add("Next Milestone");

            LoadUserSettings();

            FillMonitorCombobox();

            Screen screen = GetScreenByAppName("Firestone");
            if (screen is null)
            {
                MessageBox.Show("The game is not running");
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                SelectMonitorByDeviceName(screen.DeviceName);

                ClearPixelLabels();
                ClearImageLabels();
                ClearGameInformationLabels();

                RefreshImageListBox();
                RefreshPixelListBox();
            }
        }

        private void LoadUserSettings()
        {
            cbEnableResearch1.Checked = Properties.Settings.Default.DoResearch1;
            cbEnableResearch2.Checked = Properties.Settings.Default.DoResearch2;

            TextResearchScroll1.Text = Properties.Settings.Default.ResearchScrollAmount1;
            TextResearchScroll2.Text = Properties.Settings.Default.ResearchScrollAmount2;

            cbScrollLeft1.Checked = Properties.Settings.Default.ResearchScrollLeft1;
            cbScrollLeft2.Checked = Properties.Settings.Default.ResearchScrollLeft2;

            CheckboxUseStrangeDust.Checked = Properties.Settings.Default.UseDustForGuardian;

            ComboboxGuardian.SelectedIndex = Properties.Settings.Default.TrainedGuardian;
            ComboboxUpgradeMode.SelectedIndex = Properties.Settings.Default.UpgradeMode;
            TextSwitchToPrimaryHeroAfterStage.Text = Properties.Settings.Default.StopSubUpgradesAfterStage;

            TextTotalSquads.Text = Properties.Settings.Default.TotalSquads;
            TextOffsetX.Text = Properties.Settings.Default.offsetX;

            TextAutoclickDuration.Text = Properties.Settings.Default.cycleDurationSeconds;

            cbTaskUpgradeHeroes.Checked = Properties.Settings.Default.TaskUpgradeHeroes;
            cbTaskOracleRituals.Checked = Properties.Settings.Default.TaskOracleRituals;
            cbTaskGuildExpeditions.Checked = Properties.Settings.Default.TaskGuildExpeditions;
            cbTaskAlchemist.Checked = Properties.Settings.Default.TaskAlchemist;
            cbTaskFirestoneResearch.Checked = Properties.Settings.Default.TaskFirestoneResearch;
            cbTaskMeteoriteResearch.Checked = Properties.Settings.Default.TaskMeteoriteResearch;
            cbTaskTrainGuardian.Checked = Properties.Settings.Default.TaskTrainGuardian;
            cbTaskMapMissions.Checked = Properties.Settings.Default.TaskMapMissions;
            cbTaskDailyMissionsLiberations.Checked = Properties.Settings.Default.TaskDailyMissionsLiberations;
            cbTaskDailyMissionsDungeons.Checked = Properties.Settings.Default.TaskDailyMissionsDungeons;
            cbTaskCampaignLoot.Checked = Properties.Settings.Default.TaskCampaignLoot;
            cbTaskEngineerReward.Checked = Properties.Settings.Default.TaskEngineerReward;
            cbTaskPickaxes.Checked = Properties.Settings.Default.TaskPickaxes;
            cbTaskQuests.Checked = Properties.Settings.Default.TaskQuests;
            cbTaskDailyReward.Checked = Properties.Settings.Default.TaskDailyReward;
            cbTaskAutoclick.Checked = Properties.Settings.Default.TaskAutoclick;

            cbAlchemistDragonBlood.Checked = Properties.Settings.Default.AlchemistDragonBlood;
            cbAlchemistStrangeDust.Checked = Properties.Settings.Default.AlchemistStrangeDust;
            cbAlchemistExoticCoin.Checked = Properties.Settings.Default.AlchemistExoticCoin;
        }

        private void ClearPixelLabels()
        {
            LabelPixelName.Text = "";
            LabelPixelX.Text = "";
            LabelPixelY.Text = "";
            LabelPixelHex.Text = "";
            pbPixelActual.Image = null;
            pbPixelStored.Image = null;
            LabelPixelMatch.Text = "";
        }

        private void ClearGameInformationLabels()
        {
            TextCurrentStage.Text = "";
            TextMaxStage.Text = "";
        }

        public void FillMonitorCombobox()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                string position;

                if (bounds.Left < 0)
                    position = "Left of Primary";
                else if (bounds.Left > 0 && bounds.Top == 0)
                    position = "Right of Primary";
                else if (bounds.Top < 0)
                    position = "Above Primary";
                else if (bounds.Top > 0)
                    position = "Below Primary";
                else
                    position = "Primary";

                Monitor m = new Monitor(screen.DeviceName, bounds.Width, bounds.Height, position);
                ComboboxMonitor.Items.Add(m);

                Console.WriteLine($"{screen.DeviceName} - {position}, Resolution: {bounds.Width}x{bounds.Height}");
            }
        }

        public void RefreshImageListBox()
        {
            ListBoxImages.ClearSelected();
            ListBoxImages.Items.Clear();

            string imageFolder = AppDomain.CurrentDomain.BaseDirectory + "images\\";

            string searchPattern = "FSB*";

            // Get all files that match the search pattern in the Debug folder
            string[] files = Directory.GetFiles(imageFolder, searchPattern);

            // Display the list of files
            foreach (var filePath in files)
            {
                string input = filePath.Replace(imageFolder, "");

                string pattern = @"^FSB_(.*?)_X(-?\d+)Y(-?\d+)_X(-?\d+)Y(-?\d+)\.png$";

                Regex regex = new Regex(pattern);
                Match match = regex.Match(input);

                if (match.Success)
                {
                    string imageName = match.Groups[1].Value;
                    int firstPointX = int.Parse(match.Groups[2].Value);
                    int firstPointY = int.Parse(match.Groups[3].Value);
                    int secondPointX = int.Parse(match.Groups[4].Value);
                    int secondPointY = int.Parse(match.Groups[5].Value);

                    Image img = new Image(filePath, imageName, new System.Drawing.Point(firstPointX, firstPointY), new System.Drawing.Point(secondPointX, secondPointY));
                    ListBoxImages.Items.Add(img);
                }
                else
                {
                    Console.WriteLine("The input string does not match the expected format.");
                }
            }
        }

        public void RefreshPixelListBox()
        {
            ListBoxPixels.ClearSelected();
            ListBoxPixels.Items.Clear();

            string pixelFolder = AppDomain.CurrentDomain.BaseDirectory + "pixels\\";

            string searchPattern = "FSB_*";

            // Get all files that match the search pattern in the Debug folder
            string[] files = Directory.GetFiles(pixelFolder, searchPattern);

            // Display the list of files
            foreach (var filePath in files)
            {
                string input = filePath.Replace(pixelFolder, "");

                string pattern = @"^FSB_(.+)_([^_]+)_(?:-?X(-?\d+)Y(-?\d+))(\.txt)?$";

                Regex regex = new Regex(pattern);
                Match match = regex.Match(input);

                if (match.Success)
                {
                    string iconName = match.Groups[1].Value;
                    string colorCode = match.Groups[2].Value;
                    int xCoordinate = int.Parse(match.Groups[3].Value);
                    int yCoordinate = int.Parse(match.Groups[4].Value);

                    Pixel p = new Pixel(iconName, new System.Drawing.Point(xCoordinate, yCoordinate), new MagickColor(colorCode));
                    ListBoxPixels.Items.Add(p);
                }
                else
                {
                    Console.WriteLine("The input string did not match the expected format.");
                }
            }
        }

        public void CompareImages(string imagePath1, string imagePath2)
        {
            using (var image1 = new MagickImage(imagePath1))
            using (var image2 = new MagickImage(imagePath2))
            {
                // This creates a new image showing the differences
                using (var diffImage = new MagickImage())
                {
                    // The return value is the error metric
                    double difference = image1.Compare(image2, ErrorMetric.NormalizedCrossCorrelation, diffImage);
                    string formattedDifference = (difference * 100).ToString("0.00");

                    // Optionally, save the diff image
                    //diffImage.Write("diff.png");

                    //Console.WriteLine("Difference: " + formattedDifference);
                    LabelImageMatch.Text = formattedDifference.ToString();
                }
            }
        }

        public double CompareImagesRt(string imagePath1, string imagePath2)
        {
            using (var image1 = new MagickImage(imagePath1))
            using (var image2 = new MagickImage(imagePath2))
            {
                // This creates a new image showing the differences
                using (var diffImage = new MagickImage())
                {
                    // The return value is the error metric
                    double difference = image1.Compare(image2, ErrorMetric.NormalizedCrossCorrelation, diffImage);
                    return difference;
                }
            }
        }

        private void GetText(Label outputLabel, string filepath)
        {
            using (var engine = new TesseractEngine(@"C:\OCR\tessdata", "eng", EngineMode.TesseractOnly))
            {
                using (var img = Pix.LoadFromFile(filepath))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        outputLabel.Text = text;
                    }
                }
            }
        }

        private string GetTextRt(string filepath)
        {
            using (var engine = new TesseractEngine(@"C:\OCR\tessdata", "eng", EngineMode.TesseractOnly))
            {
                using (var img = Pix.LoadFromFile(filepath))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText().ToLower().Trim();
                    }
                }
            }
        }

        public static Rectangle CreateRectangleFromCorners(System.Drawing.Point pointA, System.Drawing.Point pointB)
        {
            pointA.X += offsetX;

            pointB.X += offsetX;

            int x = Math.Min(pointA.X, pointB.X);
            int y = Math.Min(pointA.Y, pointB.Y);
            int width = Math.Abs(pointA.X - pointB.X);
            int height = Math.Abs(pointA.Y - pointB.Y);

            return new Rectangle(x, y, width, height);
        }

        public void ScreenshotAreaSelection(string screenshotName, bool replace = false, System.Drawing.Point posA = new System.Drawing.Point(), System.Drawing.Point posB = new System.Drawing.Point())
        {
            System.Drawing.Point pointA = new System.Drawing.Point();
            System.Drawing.Point pointB = new System.Drawing.Point();
            if (replace == false)
            {

                Thread.Sleep(200);

                bool running1 = true;
                bool running2 = false;

                while (running1)
                {
                    if ((GetAsyncKeyState(Keys.V) & 0x8000) != 0 && running1)
                    {
                        running1 = false;
                        pointA = Cursor.Position;
                        running2 = true;
                    }
                    if ((GetAsyncKeyState(Keys.Escape) & 0x8000) != 0)
                    {
                        running1 = false;
                    }
                }

                Thread.Sleep(200);

                while (running2)
                {
                    if ((GetAsyncKeyState(Keys.V) & 0x8000) != 0 && running2)
                    {
                        running2 = false;
                        pointB = Cursor.Position;
                    }
                    if ((GetAsyncKeyState(Keys.Escape) & 0x8000) != 0)
                    {
                        running2 = false;
                    }
                }
            }
            else
            {
                pointA = posA;
                pointB = posB;
            }

            Bitmap screenshot = TakeScreenshot(pointA, pointB);

            string fileName = DetermineFileName(screenshotName, replace, pointA, pointB);

            screenshot.Save("images/" + fileName, System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine($"New image was saved.");

            pbImageStored.Image = new Bitmap(screenshot);
            RefreshImageListBox();
            SelectImageByName(screenshotName);
        }

        private string DetermineFileName(string screenshotName, bool replace, System.Drawing.Point pointA, System.Drawing.Point pointB)
        {
            SetOffset();
            if (replace)
            {
                string imageFolder = AppDomain.CurrentDomain.BaseDirectory + "images\\";
                string searchPattern = "FSB_" + screenshotName;
                string firstFilePath = FindFirstFile(imageFolder, searchPattern + "*");
                firstFilePath = firstFilePath.Replace(imageFolder, "");

                if (!string.IsNullOrEmpty(firstFilePath))
                {
                    Console.WriteLine($"First file found: {firstFilePath}");
                    screenshotName = firstFilePath;
                }
                else
                {
                    Console.WriteLine("No file found matching the pattern.");
                }
            }
            else
            {
                screenshotName = $"FSB_{screenshotName}_X{pointA.X - offsetX}Y{pointA.Y}_X{pointB.X - offsetX}Y{pointB.Y}.png";
            }
            return screenshotName;
        }

        private Bitmap TakeScreenshot(System.Drawing.Point pointA, System.Drawing.Point pointB)
        {
            try
            {
                Rectangle rect = CreateRectangleFromCorners(pointA, pointB);

                Bitmap result;

                using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(rect.Location, System.Drawing.Point.Empty, rect.Size);
                    }
                    result = new Bitmap(bmp);
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error TakeScreenshot(): " + e.Message);
                return null;
            }
        }

        private void SelectImageByName(string name)
        {

            ListBoxImages.ClearSelected();
            for (int i = 0; i < ListBoxImages.Items.Count; i++)
            {
                if (ListBoxImages.Items[i] is Image img && img.Name == name)
                {
                    ListBoxImages.SelectedIndex = i;
                    break;
                }
            }
        }




        private void SelectPixelByName(string name)
        {
            ListBoxPixels.ClearSelected();
            for (int i = 0; i < ListBoxPixels.Items.Count; i++)
            {
                if (ListBoxPixels.Items[i] is Pixel p && p.Name == name)
                {
                    ListBoxPixels.SelectedIndex = i;
                    break;
                }
            }
        }


        private void SelectMonitorByDeviceName(string name)
        {
            for (int i = 0; i < ComboboxMonitor.Items.Count; i++)
            {
                if (ComboboxMonitor.Items[i] is Monitor monitor && monitor.Device == name)
                {
                    ComboboxMonitor.SelectedIndex = i;
                    break;
                }
            }
        }

        static string FindFirstFile(string folderPath, string searchPattern)
        {
            try
            {
                // Get all files in the directory that match the search pattern
                string[] files = Directory.GetFiles(folderPath, searchPattern);

                // Return the first file found, or null if no file is found
                if (files.Length > 0)
                {
                    return files[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return null; // Return null if no file is found or an error occurs
        }

        private System.Drawing.Point GetCenterScreenPoint(int offSetX, int offSetY)
        {
            Screen screen = Screen.FromControl(this);
            int centerX = (screen.WorkingArea.Left + screen.WorkingArea.Width / 2) + offSetX;
            int centerY = (screen.WorkingArea.Top + screen.WorkingArea.Height / 2) + offSetY;
            System.Drawing.Point centerPoint = new System.Drawing.Point(centerX, centerY);
            return centerPoint;
        }

        private void ListBoxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetOffset();
            try
            {
                if (ListBoxImages.SelectedItem != null)
                {
                    Image imageStored = (Image)ListBoxImages.SelectedItem;
                    LabelImageFileName.Text = imageStored.Name;
                    LabelImagePath.Text = imageStored.GetFilePath();

                    LabelImagePosAX.Text = imageStored.PointA.X.ToString();
                    LabelImagePosAY.Text = imageStored.PointA.Y.ToString();

                    LabelImagePosBX.Text = imageStored.PointB.X.ToString();
                    LabelImagePosBY.Text = imageStored.PointB.Y.ToString();

                    pbImageStored.ImageLocation = imageStored.GetFilePath();

                    Bitmap screenshot = TakeScreenshot(imageStored.PointA, imageStored.PointB);
                    pbImageActual.Image = new Bitmap(screenshot);

                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);

                    string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                    string imageStoredPath = imageStored.GetFilePath();

                    CompareImages(imageStoredPath, imageCurrentPath);

                    GetText(LabelImageTextStored, imageStoredPath);
                    GetText(LabelImageTextCurrent, imageCurrentPath);

                    ToolTip toolTip = new ToolTip();
                    toolTip.SetToolTip(LabelImagePath, LabelImagePath.Text);  // Show the full text on hover

                    toolTip.InitialDelay = 100;  // Delay before the tooltip shows (in milliseconds)
                    toolTip.ReshowDelay = 100;   // Delay between subsequent tooltips
                    toolTip.AutoPopDelay = 30000; // Tooltip stays for 30 seconds
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Error lImages_SelectedIndexChanged: " + err.Message);
            }
        }

        private void ClearImageLabels()
        {
            LabelImageFileName.Text = "";
            LabelImagePath.Text = "";

            LabelImagePosAX.Text = "";
            LabelImagePosAY.Text = "";

            LabelImagePosBX.Text = "";
            LabelImagePosBY.Text = "";

            LabelImageMatch.Text = "";
            LabelImageTextStored.Text = "";
            LabelImageTextCurrent.Text = "";

            pbImageStored.Image = null;
        }

        public static Screen GetScreenByAppName(string appName)
        {
            // Get all processes
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                // Check if main window title contains the app name
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle == appName)
                {
                    // Get the handle to the main window of the process
                    IntPtr handle = process.MainWindowHandle;

                    // Get the screen that contains the window
                    return Screen.FromHandle(handle);
                }
            }

            return null; // Return null if no process found
        }

        private void ComboBoxMonitor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if there is a selected item to avoid NullReferenceException
            if (ComboboxMonitor.SelectedItem != null)
            {
                // Cast the selected item back to an Image
                Monitor monitor = (Monitor)ComboboxMonitor.SelectedItem;
                LabelDeviceName.Text = monitor.Device;
                LabelBoundsWidth.Text = monitor.BoundsWidth.ToString();
                LabelBoundsHeight.Text = monitor.BoundsHeight.ToString();
            }
        }

        private void SetPictureBoxColor(PictureBox pb, MagickColor color)
        {
            // Create a new Bitmap
            Bitmap bmp = new Bitmap(pbPixelStored.Width, pbPixelStored.Height);
            // Get the color from hex
            Color c = ColorTranslator.FromHtml(color.ToHexString());
            // Use Graphics to fill the Bitmap
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(c);
            }
            // Set the PictureBox's Image
            pb.Image = bmp;
        }

        public static MagickColor GetColorAt(System.Drawing.Point point)
        {
            point.X += offsetX;

            // Capture the screen
            Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Copy the pixel's color into the bitmap
                g.CopyFromScreen(point.X, point.Y, 0, 0, new System.Drawing.Size(1, 1));
            }

            byte[] byteArray = ConvertBitmapToBytes(bmp);
            // Use Magick.NET to read the color
            using (MagickImage image = new MagickImage(byteArray))
            {
                // Get the color of the pixel at (0, 0) in the bitmap
                MagickColor color = (MagickColor)image.GetPixels().GetPixel(0, 0).ToColor();
                return color;
            }
        }

        public static byte[] ConvertBitmapToBytes(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the bitmap to the memory stream as a PNG (or any other format)
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                // Return the array of bytes
                return ms.ToArray();
            }
        }

        private void ButtonCreateImage_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Press 'V' when your mouse is at the top left corner of the desired location. Press 'V' again when your mouse is at the bottom right location. Press 'esc' to abort.");

            System.Drawing.Point center = GetCenterScreenPoint(-100, -50);
            string newImageName = Interaction.InputBox("Enter image name", "Create new image", "{ImageName}", center.X, center.Y);

            ScreenshotAreaSelection(newImageName);
        }

        private void ButtonRecreateImage_Click(object sender, EventArgs e)
        {
            int currentSelectedIndex;
            if (ListBoxImages.SelectedIndex == -1)
            {
                MessageBox.Show("Select an image from the list before trying to recreate.");
                return;
            }
            currentSelectedIndex = ListBoxImages.SelectedIndex;
            string imageName = ((Image)ListBoxImages.SelectedItem).Name;
            System.Drawing.Point pointA = ((Image)ListBoxImages.SelectedItem).PointA;
            System.Drawing.Point pointB = ((Image)ListBoxImages.SelectedItem).PointB;
            ScreenshotAreaSelection(imageName, true, pointA, pointB);
            ListBoxImages.SelectedIndex = currentSelectedIndex;
        }

        private void ButtonDeleteImage_Click(object sender, EventArgs e)
        {
            if (ListBoxImages.SelectedIndex == -1)
            {
                MessageBox.Show("Select an image from the list before trying to delete.");
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the selected image?", "Confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string path = ((Image)ListBoxImages.SelectedItem).GetFilePath();
                System.IO.File.Delete(path);
                RefreshImageListBox();
                ClearImageLabels();
            }
        }

        private void SavePixel(Pixel p)
        {
            // Define the directory path for 'pixels' inside the Debug folder
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pixels");

            // Define the file name
            string fileName = string.Format($"FSB_{p.Name}_{p.Color.ToHexString()}_X{p.Point.X}Y{p.Point.Y}.txt");

            // Combine the directory path and file name
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {
            }

            // Create an empty file
            using (File.Create(filePath))
            {
                // File has been created and immediately closed by using 'using'
            }

            //Console.WriteLine("File succesfully created.");
            RefreshPixelListBox();
        }

        private void ButtonRenamePixel_Click(object sender, EventArgs e)
        {
            if (ListBoxPixels.SelectedIndex == -1)
            {
                MessageBox.Show("Select a pixel from the list before trying to rename");
                return;
            }

            Pixel pixel = ((Pixel)ListBoxPixels.SelectedItem);

            System.Drawing.Point center = GetCenterScreenPoint(-100, -50);
            string newPixelName = Interaction.InputBox("Enter new pixel name", "Rename pixel", "{NewPixelName}", center.X, center.Y);

            // Original file path
            string originalFilePath = pixel.GetFilePath();

            // Replace the first occurrence of 'test' with 'modified'
            int firstTestIndex = originalFilePath.IndexOf(pixel.Name);
            if (firstTestIndex != -1) // Check if 'test' is found
            {
                string newFilePath = originalFilePath.Substring(0, firstTestIndex) + newPixelName +
                                     originalFilePath.Substring(firstTestIndex + pixel.Name.Length);

                // Rename the file
                File.Move(originalFilePath, newFilePath);

                Console.WriteLine("File has been renamed to: " + newFilePath);
            }
            else
            {
                Console.WriteLine($"The string {pixel.Name} was not found in the filename.");
            }

            RefreshPixelListBox();
            SelectPixelByName(newPixelName);
        }

        private void ButtonDeletePixel_Click(object sender, EventArgs e)
        {
            if (ListBoxPixels.SelectedIndex == -1)
            {
                MessageBox.Show("Select a pixel from the list before trying to delete");
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the selected pixel?", "Confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string path = ((Pixel)ListBoxPixels.SelectedItem).GetFilePath();
                System.IO.File.Delete(path);
                RefreshPixelListBox();
                ClearPixelLabels();
            }
        }

        private void ButtonRenameImage_Click(object sender, EventArgs e)
        {
            if (ListBoxImages.SelectedIndex == -1)
            {
                MessageBox.Show("Select an image from the list before trying to rename.");
                return;
            }

            Image img = ((Image)ListBoxImages.SelectedItem);

            System.Drawing.Point center = GetCenterScreenPoint(-100, -50);
            string newImageName = Interaction.InputBox("Enter new image name", "Rename image", "{NewImageName}", center.X, center.Y);

            // Original file path
            string originalFilePath = img.GetFilePath();

            // Replace the first occurrence of 'test' with 'modified'
            int firstTestIndex = originalFilePath.IndexOf(img.Name);
            if (firstTestIndex != -1) // Check if 'test' is found
            {
                string newFilePath = originalFilePath.Substring(0, firstTestIndex) + newImageName +
                                     originalFilePath.Substring(firstTestIndex + img.Name.Length);

                // Rename the file
                File.Move(originalFilePath, newFilePath);

                Console.WriteLine("File has been renamed to: " + newFilePath);
            }
            else
            {
                Console.WriteLine($"The string {img.Name} was not found in the filename.");
            }

            RefreshImageListBox();
            SelectImageByName(newImageName);
        }

        private void ListBoxPixels_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetOffset();
            // Check if there is a selected item to avoid NullReferenceException
            if (ListBoxPixels.SelectedItem != null)
            {
                // Cast the selected item back to an Image
                Pixel pixel = (Pixel)ListBoxPixels.SelectedItem;
                LabelPixelName.Text = pixel.Name;
                LabelPixelX.Text = pixel.Point.X.ToString();
                LabelPixelY.Text = pixel.Point.Y.ToString();
                LabelPixelHex.Text = pixel.Color.ToHexString();

                MagickColor color = GetColorAt(new System.Drawing.Point(pixel.Point.X, pixel.Point.Y));
                SetPictureBoxColor(pbPixelStored, pixel.Color);
                SetPictureBoxColor(pbPixelActual, color);

                AreColorsIdentical(pbPixelStored, pbPixelActual);
            }
        }

        private bool AreColorsIdentical(PictureBox pb1, PictureBox pb2)
        {
            if (pb1.Image == null || pb2.Image == null)
            {
                return false;
            }

            // Get the color of the pixel from the first PictureBox
            Color color1 = GetPixelColorFromImage(pb1.Image);

            // Get the color of the pixel from the second PictureBox
            Color color2 = GetPixelColorFromImage(pb2.Image);

            bool result = color1.Equals(color2);

            if (result)
            {
                LabelPixelMatch.Text = "Yes";
            }
            else
            {
                LabelPixelMatch.Text = "No";
            }

            // Compare the colors
            return result;
        }

        private Color GetPixelColorFromImage(System.Drawing.Image image)
        {
            using (Bitmap bmp = new Bitmap(image))
            {
                // Since the image is 1x1, get the color of the only pixel
                return bmp.GetPixel(0, 0);
            }
        }

        private void ButtonCreatePixel_Click(object sender, EventArgs e)
        {
            System.Drawing.Point center = GetCenterScreenPoint(-100, -50);

            string newPixelName = Interaction.InputBox("Enter pixel name", "Create new pixel", "{PixelName}", center.X, center.Y);

            int posX = Cursor.Position.X;
            int posY = Cursor.Position.Y;

            MagickColor color = GetColorAt(new System.Drawing.Point(posX, posY));

            System.Drawing.Point myPoint = new System.Drawing.Point(posX, posY);

            Pixel myPixel = new Pixel(newPixelName, myPoint, color);

            SavePixel(myPixel);
            RefreshPixelListBox();
            SelectPixelByName(newPixelName);
        }

        private void ShortSleep()
        {
            Thread.Sleep(150);
        }

        private void MediumSleep()
        {
            Thread.Sleep(500);
        }

        private void LongSleep()
        {
            Thread.Sleep(5000);
        }

        private void ButtonUpdatePixel_Click(object sender, EventArgs e)
        {
            SetOffset();
            if (ListBoxPixels.SelectedIndex == -1)
            {
                MessageBox.Show("Select a pixel from the list before trying to update.");
                return;
            }
            Pixel pixel = ((Pixel)ListBoxPixels.SelectedItem);
            string selectedName = pixel.Name;

            int posX = Cursor.Position.X - offsetX;

            int posY = Cursor.Position.Y;

            MagickColor newColor = GetColorAt(new System.Drawing.Point(posX, posY));
            Pixel newPixel = new Pixel(pixel.Name, new System.Drawing.Point(posX, posY), newColor);

            File.Delete(pixel.GetFilePath());

            SavePixel(newPixel);
            RefreshPixelListBox();
            SelectPixelByName(selectedName);
        }

        private void Logging(string message)
        {
            if (TextboxStatus.InvokeRequired)
            {
                TextboxStatus.Invoke(new Action(() => Logging(message)));
            }
            else
            {
                string msg = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " - " + message;
                TextboxStatus.AppendText(msg + Environment.NewLine);
                TextboxStatus.ScrollToCaret();
                //using (StreamWriter writer = new StreamWriter(@"C:/temp/logging.txt", true)) // true to append data to the file
                //{
                //    writer.WriteLine(msg);
                //}
            }
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        bool active = false;

        public static List<OpenCvSharp.Point> FindTargetInMap(string mapPath, string targetPath, double threshold = 0.79)
        {
            List<OpenCvSharp.Point> coordinates = new List<OpenCvSharp.Point>();

            // Load images
            using (Mat map = new Mat(mapPath, ImreadModes.Color))
            using (Mat target = new Mat(targetPath, ImreadModes.Color))
            using (Mat result = new Mat())
            {
                // Match template without a mask
                Cv2.MatchTemplate(map, target, result, TemplateMatchModes.CCoeffNormed);

                // Collect matches above the threshold
                while (true)
                {
                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
                    if (maxVal >= threshold)
                    {
                        coordinates.Add(maxLoc);
                        Cv2.Rectangle(result, new OpenCvSharp.Rect(maxLoc.X, maxLoc.Y, target.Width, target.Height), new Scalar(0), -1);
                    }
                    else
                    {
                        break;
                    }
                }

                // Apply non-maximum suppression to remove overlapping boxes
                coordinates = NonMaximumSuppression(coordinates, target.Size(), 0.01); // Stricter threshold
            }

            return coordinates;
        }

        private static List<OpenCvSharp.Point> NonMaximumSuppression(List<OpenCvSharp.Point> points, OpenCvSharp.Size targetSize, double iouThreshold)
        {
            List<OpenCvSharp.Rect> rects = new List<OpenCvSharp.Rect>();
            foreach (var point in points)
            {
                rects.Add(new OpenCvSharp.Rect(point.X, point.Y, targetSize.Width, targetSize.Height));
            }

            List<int> indices = Enumerable.Range(0, rects.Count).ToList();
            List<int> finalIndices = new List<int>();

            while (indices.Count > 0)
            {
                int bestIndex = indices[0];
                finalIndices.Add(bestIndex);

                OpenCvSharp.Rect bestRect = rects[bestIndex];
                indices.RemoveAt(0);

                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    OpenCvSharp.Rect rect = rects[indices[i]];
                    if (IntersectionOverUnion(bestRect, rect) > iouThreshold)
                    {
                        indices.RemoveAt(i);
                    }
                }
            }

            return finalIndices.Select(i => points[i]).ToList();
        }

        private static double IntersectionOverUnion(OpenCvSharp.Rect a, OpenCvSharp.Rect b)
        {
            int intersectionArea = (a & b).Width * (a & b).Height;
            int unionArea = (a.Width * a.Height) + (b.Width * b.Height) - intersectionArea;
            return (double)intersectionArea / unionArea;
        }

        private void CenterMap()
        {
            Pixel startDrag = GetPixelByName("StartDrag");
            Pixel endDrag = GetPixelByName("EndDrag");
            AutoClicker.DragMouse(startDrag.Point, endDrag.Point);
            ShortSleep();
            AutoClicker.DragMouse(startDrag.Point, endDrag.Point);
            ShortSleep();
            AutoClicker.DragMouse(startDrag.Point, endDrag.Point);
            ShortSleep();
            AutoClicker.DragMouse(startDrag.Point, endDrag.Point);
            ShortSleep();

            Pixel Drag1A = GetPixelByName("A_MapDragStart");
            Pixel Drag1B = GetPixelByName("A_MapDragEnd");
            AutoClicker.DragMouse(Drag1A.Point, Drag1B.Point);
            ShortSleep();
        }

        private void SetOffset()
        {
            offsetX = int.Parse(TextOffsetX.Text);
            AutoClicker.offsetX = offsetX;
        }

        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            if (DoValidations() == false)
            {
                return;
            }

            SetOffset();

            ButtonStop.Enabled = true;
            ButtonStart.Enabled = false;
            active = true;

            GetGameInfo();

            while (active)
            {
                UpdateCurrentStage();
                UpgradeHeroes();
                OracleRituals();
                GuildExpeditions();
                Alchemist();
                FirestoneResearch();
                MeteoriteResearch();
                TrainGuardian();
                EngineerReward();
                CampaignLoot();
                MapMissions();
                DailyMissionsLiberations();
                DailyMissionsDungeons();
                Quests();
                Pickaxes();
                DailyReward();
                UpdateCurrentStage();

                if (cbTaskAutoclick.Checked)
                {
                    GoToMainScreen();
                    await RunAutoclickerAsync(int.Parse(TextAutoclickDuration.Text));
                }
            }
        }

        private void DailyReward()
        {
            if (cbTaskDailyReward.Checked == false)
            {
                return;
            }

            Logging("Start daily reward.");

            Pixel GemIcon = GetPixelByName("GemIcon");
            AutoClicker.LeftClickAtPosition(GemIcon.Point);
            MediumSleep();

            Pixel GemTab1 = GetPixelByName("GemTab1");
            AutoClicker.LeftClickAtPosition(GemTab1.Point);
            MediumSleep();

            Image MysteryBoxFree = GetImageByName("MysteryBoxFree");
            Bitmap screenshot = TakeScreenshot(MysteryBoxFree.PointA, MysteryBoxFree.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            LongSleep();

            if (CurrentText.ToLower().Trim() == "free")
            {
                Pixel GemFreeMysteryBox = GetPixelByName("GemFreeMysteryBox");
                AutoClicker.LeftClickAtPosition(GemFreeMysteryBox.Point);
                MediumSleep();
                Logging("Claimed daily mystery box.");
            }
            else
            {
                Logging("Daily mystery box already claimed.");
            }

            Pixel GemTab7 = GetPixelByName("GemTab7");
            AutoClicker.LeftClickAtPosition(GemTab7.Point);
            MediumSleep();

            Pixel GemCalendarCheckin = GetPixelByName("GemCalendarCheckin");
            AutoClicker.LeftClickAtPosition(GemCalendarCheckin.Point);
            MediumSleep();

            Logging("Claimed daily reward");

            AutoClicker.SendKey("Firestone", "{ESC}");
            MediumSleep();

            AutoClicker.SendKey("Firestone", "{O}");
            MediumSleep();

            Pixel OracleShop = GetPixelByName("OracleShop");
            AutoClicker.LeftClickAtPosition(OracleShop.Point);
            MediumSleep();

            Pixel OracleExtremeValueBundles = GetPixelByName("OracleExtremeValueBundles");
            AutoClicker.LeftClickAtPosition(OracleExtremeValueBundles.Point);
            MediumSleep();

            Pixel FreeOracleGift = GetPixelByName("FreeOracleGift");
            AutoClicker.LeftClickAtPosition(FreeOracleGift.Point);
            MediumSleep();

            Logging("Claimed daily oracle gift.");
            
            MediumSleep();

            GoToMainScreen();

            Logging("Finished all daily rewards.");
        }

        private void Pickaxes()
        {
            if (cbTaskPickaxes.Checked == false)
            {
                return;
            }

            Logging("Start pickaxes.");

            AutoClicker.SendKey("Firestone", "{T}");
            MediumSleep();

            Pixel TownSelectGuild = GetPixelByName("TownSelectGuild");
            AutoClicker.LeftClickAtPosition(TownSelectGuild.Point);
            MediumSleep();

            Pixel GuildSelectArcaneCrystal = GetPixelByName("GuildSelectArcaneCrystal");
            AutoClicker.LeftClickAtPosition(GuildSelectArcaneCrystal.Point);
            MediumSleep();

            Pixel GuildSelectPickaxePlus = GetPixelByName("GuildSelectPickaxe+");
            AutoClicker.LeftClickAtPosition(GuildSelectPickaxePlus.Point);
            MediumSleep();

            Pixel GuildSelectFreePickaxes = GetPixelByName("GuildSelectFreePickaxes");
            AutoClicker.LeftClickAtPosition(GuildSelectFreePickaxes.Point);
            MediumSleep();

            GoToMainScreen();

            Logging("Finished pickaxes.");
        }

        private void Quests()
        {
            if (cbTaskQuests.Checked == false)
            {
                return;
            }

            Logging("Start quests");

            AutoClicker.SendKey("Firestone", "{Q}");
            MediumSleep();

            Pixel QuestsDaily = GetPixelByName("QuestsDaily");
            AutoClicker.LeftClickAtPosition(QuestsDaily.Point);

            MediumSleep();

            for (int i = 0; i < 6; i++)
            {
                Pixel QuestsClaim = GetPixelByName("QuestsClaim");
                AutoClicker.LeftClickAtPosition(QuestsClaim.Point);
                ShortSleep();
            }

            MediumSleep();

            Pixel QuestsWeekly = GetPixelByName("QuestsWeekly");
            AutoClicker.LeftClickAtPosition(QuestsWeekly.Point);

            MediumSleep();

            for (int i = 0; i < 6; i++)
            {
                Pixel QuestsClaim = GetPixelByName("QuestsClaim");
                AutoClicker.LeftClickAtPosition(QuestsClaim.Point);
                ShortSleep();
            }

            MediumSleep();

            GoToMainScreen();

            Logging("Finished quests");            
        }

        private void DailyMissionsDungeons()
        {
            if (cbTaskDailyMissionsDungeons.Checked == false)
            {
                return;
            }

            Logging("Start daily missions dungeons");

            AutoClicker.SendKey("Firestone", "{M}");
            MediumSleep();

            Pixel MapMissionsTankIcon = GetPixelByName("MapMissionsTankIcon");
            AutoClicker.LeftClickAtPosition(MapMissionsTankIcon.Point);

            MediumSleep();

            Pixel DailyMissionsButton = GetPixelByName("DailyMissionsButton");
            AutoClicker.LeftClickAtPosition(DailyMissionsButton.Point);

            MediumSleep();

            Pixel LiberationMissionsButton = GetPixelByName("DungeonMissionButton");
            AutoClicker.LeftClickAtPosition(LiberationMissionsButton.Point);

            MediumSleep();

            Pixel MissionsMenuCenter = GetPixelByName("DungeonsMenuCenter");
            AutoClicker.LeftClickAtPosition(MissionsMenuCenter.Point);

            MediumSleep();

            // Scroll to the first 2 daily mission dungeons
            int direction = 1;
            AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 70);

            MediumSleep();

            // Check if dungeon 1 is available
            Image Liberate = GetImageByName("Dungeon1");
            Bitmap screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "fight")
            {
                Console.WriteLine("Dungeon 1 needs to be done");

                Pixel Liberate1 = GetPixelByName("Dungeon1");
                AutoClicker.LeftClickAtPosition(Liberate1.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Dungeon 1 is ready.");
            }
            MediumSleep();

            // Check if mission 2 is available
            Liberate = GetImageByName("Dungeon2");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "fight")
            {
                Console.WriteLine("Dungeon 2 needs to be done");

                Pixel Liberate2 = GetPixelByName("Dungeon2");
                AutoClicker.LeftClickAtPosition(Liberate2.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Dungeon 2 is ready.");
            }
            MediumSleep();

            GoToMainScreen();

            Logging("Finished daily dungeons");
        }

        private void MeteoriteResearch()
        {
            if (cbTaskMeteoriteResearch.Checked == false)
            {
                return;
            }

            Logging("Start meteorite research.");
            AutoClicker.SendKey("Firestone", "{L}");
            MediumSleep();

            Pixel ResearchesMeteorite = GetPixelByName("ResearchesMeteorite");
            AutoClicker.LeftClickAtPosition(ResearchesMeteorite.Point);
            MediumSleep();

            Pixel MeteoriteSelectedUpgrade = GetPixelByName("A_MeteoriteUpgrade");
            AutoClicker.LeftClickAtPosition(MeteoriteSelectedUpgrade.Point);
            MediumSleep();

            Pixel MeteoriteResearchButton = GetPixelByName("MeteoriteResearchButton");
            AutoClicker.LeftClickAtPosition(MeteoriteResearchButton.Point);
            MediumSleep();           

            GoToMainScreen();

            Logging("Finished meteorite research.");
        }

        private bool DoValidations()
        {
            if
                (
                cbTaskUpgradeHeroes.Checked == false &&
                cbTaskOracleRituals.Checked == false &&
                cbTaskGuildExpeditions.Checked == false &&
                cbTaskAlchemist.Checked == false &&
                cbTaskFirestoneResearch.Checked == false &&
                cbTaskMeteoriteResearch.Checked == false &&
                cbTaskTrainGuardian.Checked == false &&
                cbTaskEngineerReward.Checked == false &&
                cbTaskCampaignLoot.Checked == false &&
                cbTaskMapMissions.Checked == false &&
                cbTaskDailyMissionsDungeons.Checked == false &&
                cbTaskDailyMissionsLiberations.Checked == false &&
                cbTaskQuests.Checked == false &&
                cbTaskPickaxes.Checked == false &&
                cbTaskDailyReward.Checked == false &&
                cbTaskAutoclick.Checked == false
                )
            {
                MessageBox.Show("Tasks: Select at least 1 task.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextSwitchToPrimaryHeroAfterStage.Text, out int xxx))
            {
                MessageBox.Show("SETTING: 'Switch to primary hero after stage' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextTotalSquads.Text, out xxx))
            {
                MessageBox.Show("SETTING: 'Total Squads' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextOffsetX.Text, out xxx))
            {
                MessageBox.Show("SETTING: 'offsetX' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextAutoclickDuration.Text, out xxx))
            {
                MessageBox.Show("SETTING: 'Autoclick duration' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextResearchScroll1.Text, out xxx))
            {
                MessageBox.Show("SETTING: 'Research Scroll 1' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!int.TryParse(TextResearchScroll2.Text, out xxx))
            {
                MessageBox.Show("SETTING: 'Research Scroll 2' requires a numeric value.", "Setting validation error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private void TrainGuardian()
        {
            if (cbTaskTrainGuardian.Checked == false)
            {
                return;
            }

            Logging("Start guardian training");
            AutoClicker.SendKey("Firestone", "{G}");
            ShortSleep();

            if (ComboboxGuardian.SelectedIndex == 0)
            {
                Pixel Guardian1 = GetPixelByName("Guardian1");
                AutoClicker.LeftClickAtPosition(Guardian1.Point);
                MediumSleep();
            }
            else if (ComboboxGuardian.SelectedIndex == 1)
            {
                Pixel Guardian2 = GetPixelByName("Guardian2");
                AutoClicker.LeftClickAtPosition(Guardian2.Point);
                MediumSleep();
            }
            if (ComboboxGuardian.SelectedIndex == 2)
            {
                Pixel Guardian3 = GetPixelByName("Guardian3");
                AutoClicker.LeftClickAtPosition(Guardian3.Point);
                MediumSleep();
            }
            if (ComboboxGuardian.SelectedIndex == 3)
            {
                Pixel Guardian3 = GetPixelByName("Guardian4");
                AutoClicker.LeftClickAtPosition(Guardian3.Point);
                MediumSleep();
            }

            Pixel GuardianTrain = GetPixelByName("GuardianTrain");
            AutoClicker.LeftClickAtPosition(GuardianTrain.Point);
            ShortSleep();

            if (CheckboxUseStrangeDust.Checked == true)
            {
                Pixel GuardianDust = GetPixelByName("GuardianDust");
                AutoClicker.LeftClickAtPosition(GuardianDust.Point);
                ShortSleep();
            }

            GoToMainScreen();

            Logging("Finished guardian training");
        }

        private void MapMissions(Boolean debug = false)
        {
            if (cbTaskMapMissions.Checked == false && debug == false)
            {
                return;
            }

            Logging("Start map missions");
            AutoClicker.SendKey("Firestone", "{M}");
            MediumSleep();

            int totalSquads = int.Parse(TextTotalSquads.Text);

            Image MissionClaim = GetImageByName("MissionClaim");
            Bitmap screenshotMissionClaim = TakeScreenshot(MissionClaim.PointA, MissionClaim.PointB);
            screenshotMissionClaim.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string screenshotImageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
            string screenshotText = GetTextRt(screenshotImageCurrentPath);

            Logging("Start claiming process.");
            int missionsClaimed = 0;
            while (screenshotText == "claim" && debug == false)
            {
                Pixel pixelMissionClaim = GetPixelByName("MissionClaim");
                AutoClicker.LeftClickAtPosition(pixelMissionClaim.Point);
                MediumSleep();

                Pixel MissionClaimRewards = GetPixelByName("MissionClaimRewards");
                AutoClicker.LeftClickAtPosition(MissionClaimRewards.Point);
                MediumSleep();

                missionsClaimed++;

                screenshotMissionClaim = TakeScreenshot(MissionClaim.PointA, MissionClaim.PointB);
                screenshotMissionClaim.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                screenshotImageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                screenshotText = GetTextRt(screenshotImageCurrentPath);

                // +2 Accounts for potential error/misclick during claiming phase
                if (missionsClaimed > (totalSquads + 2))
                {
                    Logging("The claim mission loop seems to be stuck, breaking out of while loop forcefully.");
                    break;
                }
            }
            if (missionsClaimed == 0 && debug == false)
            {
                Logging("There was nothing to claim.");
            }
            else
            {
                Logging("Claiming process finished.");
            }

            Logging("Check for idling squads..");
            Image squadIdling = GetImageByName("SquadIdling");
            Bitmap screenshot = TakeScreenshot(squadIdling.PointA, squadIdling.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
            string text = GetTextRt(imageCurrentPath);
            if (text != "your squads are idling! start a new mission" && debug == false)
            {
                Logging("Your squads are not idling.");
                Logging("Finished map missions.");
                AutoClicker.SendKey("Firestone", "{ESC}");
                return;
            }
            Logging("Idling squads found.");
            Logging("Centering map.");
            CenterMap();
            Logging("Map centered.");
            ShortSleep();

            List<OpenCvSharp.Point> AllCoordinates = new List<OpenCvSharp.Point>();

            Image myImg = GetImageByName("MissionMap");
            screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP.png");

            MediumSleep();

            string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP.png";
            string mapPathWithMarkers = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            File.Copy(mapPath, mapPathWithMarkers, true);

            int marginX = 1800;

            // NavalMissions
            int countNavalMissions = 0;
            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_NAVAL.png";
            List<OpenCvSharp.Point> coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            Mat mapImage = new Mat(mapPath, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countNavalMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countNavalMissions + " Naval missions found.");

            // MonsterMissions
            int countMonsterMissions = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_MONSTER.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countMonsterMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countMonsterMissions + " Monster missions found.");


            // MysteryGifts
            int countMysteryGifts = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_MYSTERYGIFT.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countMysteryGifts++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countMysteryGifts + " Mystery Gifts found.");


            // WarMissions
            int countWarMissions = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_WAR.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countWarMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);

            Logging(countWarMissions + " War missions found.");

            // ScoutMissions
            int countScoutMissions = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_SCOUT.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countScoutMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countScoutMissions + " Scout missions found.");


            // AdventureMissions
            int countAdventureMissions = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_ADVENTURE.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countAdventureMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countAdventureMissions + " Adventure missions found.");

            // DragonMissions
            int countDragonMissions = 0;
            targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/TARGET_DRAGON.png";
            coordinates = FindTargetInMap(mapPath, targetPath, 0.76);
            mapImage = new Mat(mapPathWithMarkers, ImreadModes.Color);
            foreach (OpenCvSharp.Point point in coordinates)
            {
                if (point.X < marginX) continue;
                countDragonMissions++;
                Cv2.Rectangle(mapImage, new OpenCvSharp.Rect(point.X, point.Y, 10, 10), new Scalar(0, 0, 0), -1); // Fill rectangle
                AllCoordinates.Add(point);
            }
            resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missions") + "/MAP_with_markers.png";
            mapImage.SaveImage(resultPath);
            Logging(countDragonMissions + " Dragon missions found.");


            int coordinateCount = 0;
            if (debug)
            {
                AllCoordinates.Clear();
            }
            foreach (OpenCvSharp.Point point in AllCoordinates)
            {
                AutoClicker.LeftClickAtPosition(new System.Drawing.Point(point.X, point.Y));
                MediumSleep();

                Image MissionStartButton = GetImageByName("MissionStartButton");
                screenshot = TakeScreenshot(MissionStartButton.PointA, MissionStartButton.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                text = GetTextRt(imageCurrentPath);
                if (text == "start mission")
                {
                    Pixel StartMissionButton = GetPixelByName("StartMissionButton");
                    AutoClicker.LeftClickAtPosition(StartMissionButton.Point);
                }
                else
                {
                    AutoClicker.SendKey("Firestone", "{ESC}");
                }
                MediumSleep();
                coordinateCount++;
                if (coordinateCount >= totalSquads)
                {
                    break;
                }
            }

            GoToMainScreen();

            Logging("Finished map missions");
        }

        private void FirestoneResearch()
        {
            if (cbTaskFirestoneResearch.Checked == false)
            {
                return;
            }

            Logging("Start firestone research.");
            AutoClicker.SendKey("Firestone", "{L}");
            ShortSleep();

            Pixel ResearchesFirestone = GetPixelByName("ResearchesFirestone");
            AutoClicker.LeftClickAtPosition(ResearchesFirestone.Point);
            ShortSleep();

            ResearchScroll();

            Image myImg = GetImageByName("FirestoneClaim2");
            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
            string text = GetTextRt(imageCurrentPath);

            if (text == "claim")
            {
                Pixel FirestoneClaim1 = GetPixelByName("FirestoneClaim2");
                AutoClicker.LeftClickAtPosition(FirestoneClaim1.Point);
                Logging("Claimed firestone research #2");
            }

            MediumSleep();

            myImg = GetImageByName("FirestoneClaim1");
            screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
            text = GetTextRt(imageCurrentPath);

            if (text == "claim")
            {
                Pixel FirestoneClaim1 = GetPixelByName("FirestoneClaim1");
                AutoClicker.LeftClickAtPosition(FirestoneClaim1.Point);
                Logging("Claimed firestone research #1");
            }

            MediumSleep();

            if (cbEnableResearch1.Checked)
            {
                Pixel FirestoneUpgrade1 = GetPixelByName("A_FirestoneUpgrade1");
                AutoClicker.LeftClickAtPosition(FirestoneUpgrade1.Point);
                MediumSleep();

                myImg = GetImageByName("FirestoneUpgradeResearchButton");
                screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                text = GetTextRt(imageCurrentPath);

                if (text == "research")
                {
                    Pixel FirestoneUpgradeResearchButton = GetPixelByName("FirestoneUpgradeResearchButton");
                    AutoClicker.LeftClickAtPosition(FirestoneUpgradeResearchButton.Point);
                    Logging("Started firestone research #1");
                }
                else
                {
                    Logging("research #1 still busy");
                    AutoClicker.SendKey("Firestone", "{ESC}");
                }
                MediumSleep();
            }

            if (cbEnableResearch2.Checked)
            {
                Pixel FirestoneUpgrade2 = GetPixelByName("A_FirestoneUpgrade2");
                AutoClicker.LeftClickAtPosition(FirestoneUpgrade2.Point);
                MediumSleep();

                myImg = GetImageByName("FirestoneUpgradeResearchButton");
                screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                text = GetTextRt(imageCurrentPath);

                if (text == "research")
                {
                    Pixel FirestoneUpgradeResearchButton = GetPixelByName("FirestoneUpgradeResearchButton");
                    AutoClicker.LeftClickAtPosition(FirestoneUpgradeResearchButton.Point);
                    Logging("Started firestone research #2");
                }
                else
                {
                    Logging("research #2 still busy");
                    AutoClicker.SendKey("Firestone", "{ESC}");
                }
                MediumSleep();
            }
            
            MediumSleep();

            GoToMainScreen();
            Logging("Finished firestone research.");
        }

        private void ResearchScroll()
        {
            Pixel FirestoneResearchCenter = GetPixelByName("FirestoneResearchCenter");
            AutoClicker.LeftClickAtPosition(FirestoneResearchCenter.Point);

            int direction = -1;
            if (cbScrollLeft1.Checked) { direction = 1; }

            AutoClicker.ScrollMouseWheel(FirestoneResearchCenter.Point, direction, int.Parse(TextResearchScroll1.Text));

            MediumSleep();

            direction = -1;
            if (cbScrollLeft2.Checked) { direction = 1; }

            AutoClicker.ScrollMouseWheel(FirestoneResearchCenter.Point, direction, int.Parse(TextResearchScroll2.Text));

            MediumSleep();
        }

        private void Alchemist()
        {
            if (cbTaskAlchemist.Checked == false)
            {
                return;
            }

            Logging("Start alchemist.");
            AutoClicker.SendKey("Firestone", "{A}");
            MediumSleep();

            if (cbAlchemistDragonBlood.Checked)
            {
                Image myImg = GetImageByName("AlchemistOptionA");
                Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                string text = GetTextRt(imageCurrentPath);

                if (text != "speed up")
                {
                    Pixel alchemistA = GetPixelByName("TaskAlchemistA");
                    AutoClicker.LeftClickAtPosition(alchemistA.Point);
                    Logging("Alchemist upgrade (Dragon Blood) selected.");
                    MediumSleep();
                }
                else
                {
                    Logging("Alchemist upgrade (Dragon Blood) is still running.");
                    MediumSleep();
                }
            }

            if (cbAlchemistStrangeDust.Checked)
            {
                Image myImg = GetImageByName("AlchemistOptionB");
                Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                string text = GetTextRt(imageCurrentPath);

                if (text != "speed up")
                {
                    Pixel alchemistA = GetPixelByName("TaskAlchemistB");
                    AutoClicker.LeftClickAtPosition(alchemistA.Point);
                    Logging("Alchemist upgrade (Strange Dust) selected.");
                    MediumSleep();
                }
                else
                {
                    Logging("Alchemist upgrade (Strange Dust) is still running.");
                    MediumSleep();
                }
            }

            if (cbAlchemistExoticCoin.Checked)
            {
                Image myImg = GetImageByName("AlchemistOptionC");
                Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
                screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");
                string text = GetTextRt(imageCurrentPath);

                if (text != "speed up")
                {
                    Pixel alchemistA = GetPixelByName("TaskAlchemistC");
                    AutoClicker.LeftClickAtPosition(alchemistA.Point);
                    Logging("Alchemist upgrade (Exotic Coin) selected.");
                    MediumSleep();
                }
                else
                {
                    Logging("Alchemist upgrade (Exotic Coin) is still running.");
                    MediumSleep();
                }
            }

            GoToMainScreen();

            Logging("Finished alchemist.");
        }

        private void GuildExpeditions()
        {
            if (cbTaskGuildExpeditions.Checked == false)
            {
                return;
            }

            Logging("Start guild expeditions.");
            AutoClicker.SendKey("Firestone", "{T}");
            MediumSleep();

            Pixel TownSelectGuild = GetPixelByName("TownSelectGuild");
            AutoClicker.LeftClickAtPosition(TownSelectGuild.Point);
            MediumSleep();

            Pixel GuildSelectEx = GetPixelByName("GuildSelectExpeditions");
            AutoClicker.LeftClickAtPosition(GuildSelectEx.Point);
            MediumSleep();

            Pixel ExCurrent = GetPixelByName("ExpeditionSelectCurrent");
            AutoClicker.LeftClickAtPosition(ExCurrent.Point);
            MediumSleep();

            Pixel exOne = GetPixelByName("ExpeditionSelect1");
            AutoClicker.LeftClickAtPosition(exOne.Point);
            MediumSleep();

            GoToMainScreen();

            Logging("Finished guild expeditions.");
        }

        private void OracleRituals()
        {
            if (cbTaskOracleRituals.Checked == false)
            {
                return;
            }

            Logging("Start oracle rituals.");
            AutoClicker.SendKey("Firestone", "{O}");

            Pixel myPixel = GetPixelByName("OracleSelectRituals");
            AutoClicker.LeftClickAtPosition(myPixel.Point);
            MediumSleep();

            Pixel OracleObedience = GetPixelByName("OracleObedience");
            Pixel OracleHarmony = GetPixelByName("OracleHarmony");
            Pixel OracleConcentration = GetPixelByName("OracleConcentration");
            Pixel OracleSerenity = GetPixelByName("OracleSerenity");

            AutoClicker.LeftClickAtPosition(OracleObedience.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleHarmony.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleSerenity.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleConcentration.Point);
            ShortSleep();

            AutoClicker.LeftClickAtPosition(OracleObedience.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleHarmony.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleSerenity.Point);
            ShortSleep();
            AutoClicker.LeftClickAtPosition(OracleConcentration.Point);
            ShortSleep();

            GoToMainScreen();

            Logging("Finished oracle rituals.");
            MediumSleep();
        }

        private void GetGameInfo()
        {
            Logging("Getting game info.");
            GoToMainScreen();
            UpdateMaxStageAndCharacterLevel();
            Logging("Game info retrieved.");
        }

        private void UpdateMaxStageAndCharacterLevel()
        {
            AutoClicker.SendKey("Firestone", "{C}");
            MediumSleep();

            Image myImg = GetImageByName("MaxStage");

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            string text = GetTextRt(imageCurrentPath);

            TextMaxStage.Text = text;
            TextMaxStage.Refresh();

            ////
            //myImg = GetImageByName("CharacterLevel");

            //screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            //screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            //imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            //string text2 = GetTextRt(imageCurrentPath);

            //LabelGICharacterLevel.Text = text2;
            //LabelGICharacterLevel.Refresh();

            AutoClicker.SendKey("Firestone", "{ESC}");
            MediumSleep();
        }

        private void UpdateCurrentStage()
        {
            Logging("Update current stage.");
            Image myImg = GetImageByName("CurrentStage");

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            string text = GetTextRt(imageCurrentPath);

            TextCurrentStage.Text = text;
            TextCurrentStage.Refresh();

            Logging("Updated current stage.");
        }

        private Task RunAutoclickerAsync(int seconds)
        {


            Pixel myPixel = GetPixelByName("IdleMouseLocation");
            AutoClicker.LeftClickAtPosition(myPixel.Point);
            myAutoClicker.StartClicking("Firestone");

            var tcs = new TaskCompletionSource<bool>();

            void tickHandler(object sender, EventArgs e)
            {
                clickTimer.Tick -= tickHandler; // Detach the event handler to avoid memory leaks
                clickTimer.Stop();
                myAutoClicker.StopClicking();
                tcs.SetResult(true);
            }

            clickTimer.Interval = seconds * 1000; // Set the timer for X seconds
            clickTimer.Tick += tickHandler; // Dynamically attach the event handler
            clickTimer.Start(); // Start the timer

            return tcs.Task; // This task completes when the timer ticks
        }

        private void UpgradeHeroes()
        {
            if (cbTaskUpgradeHeroes.Checked == false)
            {
                return;
            }

            int dontUpgradeAfterStage = int.Parse(TextSwitchToPrimaryHeroAfterStage.Text);
            OpenUpgradesMenu();
            int.TryParse(TextCurrentStage.Text, out int currentStage);
            if (IsUpgradeableGeneric())
            {
                Pixel myPixel = GetPixelByName("UpgradeGeneric");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading generic");
                MediumSleep();
            }
            if (IsUpgradeableHero5())
            {
                Pixel myPixel = GetPixelByName("UpgradeHero5");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading hero5");
                ShortSleep();
            }
            if (IsUpgradeableGuardian() && currentStage < dontUpgradeAfterStage)
            {
                Pixel myPixel = GetPixelByName("UpgradeGuardian");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading guardian");
                ShortSleep();
            }
            if (IsUpgradeableHero4() && currentStage < dontUpgradeAfterStage)
            {
                Pixel myPixel = GetPixelByName("UpgradeHero4");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading hero4");
                ShortSleep();
            }
            if (IsUpgradeableHero3() && currentStage < dontUpgradeAfterStage)
            {
                Pixel myPixel = GetPixelByName("UpgradeHero3");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading hero3");
                ShortSleep();
            }
            if (IsUpgradeableHero2() && currentStage < dontUpgradeAfterStage)
            {
                Pixel myPixel = GetPixelByName("UpgradeHero2");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading hero2");
                ShortSleep();
            }
            if (IsUpgradeableHero1() && currentStage < dontUpgradeAfterStage)
            {
                Pixel myPixel = GetPixelByName("UpgradeHero1");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                Logging("Upgrading hero1");
                ShortSleep();
            }
            MediumSleep();
        }

        private bool IsUpgradeableGeneric()
        {
            Pixel myPixel = GetPixelByName("UpgradeGeneric");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableGuardian()
        {
            Pixel myPixel = GetPixelByName("UpgradeGuardian");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableHero1()
        {
            Pixel myPixel = GetPixelByName("UpgradeHero1");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableHero2()
        {
            Pixel myPixel = GetPixelByName("UpgradeHero2");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableHero3()
        {
            Pixel myPixel = GetPixelByName("UpgradeHero3");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableHero4()
        {
            Pixel myPixel = GetPixelByName("UpgradeHero4");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUpgradeableHero5()
        {
            Pixel myPixel = GetPixelByName("UpgradeHero5");
            MagickColor colorCurrent = GetColorAt(myPixel.Point);
            MagickColor colorStored = myPixel.Color;

            if (colorCurrent == colorStored)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        private void SetUpgradeMax()
        {
            while (!IsUpgradeMode("upgrade max"))
            {
                Logging("Upgrade mode is not set to 'upgrade max'");
                Pixel myPixel = GetPixelByName("UpgradeMax");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                ShortSleep();
            }
            Logging("Upgrade mode set to 'upgrade max'");
            MediumSleep();
        }

        private void SetUpgradeNextMilestone()
        {
            while (!IsUpgradeMode("next milestone"))
            {
                Logging("Upgrade mode is not set to 'next milestone'");
                Pixel myPixel = GetPixelByName("UpgradeMax");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                ShortSleep();
            }
            Logging("Upgrade mode set to 'next milestone'");
            MediumSleep();
        }

        private bool IsUpgradeMode(string upgradeMode)
        {
            Image myImg = GetImageByName("UpgradeMax");

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            string text = GetTextRt(imageCurrentPath);

            if (text == upgradeMode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OpenUpgradesMenu()
        {
            if (!InsideUpgradesScreen())
            {
                GoToMainScreen();

                Pixel myPixel = GetPixelByName("Upgrades");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
            }
            MediumSleep();
            Logging("Clicked 'Upgrades'");

            SetDetailedView();
            if (ComboboxUpgradeMode.SelectedItem.ToString() == "Upgrade Max")
            {
                SetUpgradeMax();
            }
            else
            {
                SetUpgradeNextMilestone();
            }
        }

        private bool InsideUpgradesScreen()
        {
            Image myImg = GetImageByName("UpgradesX");
            string imageStoredPath = myImg.GetFilePath();

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            double match = CompareImagesRt(imageStoredPath, imageCurrentPath);

            if (match > 0.99)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetDetailedView()
        {
            int safetyCounter = 0;
            while (!InDetailedView())
            {
                Logging("Upgrades screen is not set to 'detailed view'");
                Pixel myPixel = GetPixelByName("BasicView");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
                MediumSleep();
                safetyCounter++;
                if (safetyCounter > 12)
                {
                    OpenUpgradesMenu();
                    break;
                }
            }
            Logging("Upgrades screen set to 'detailed view'");
        }

        private void SetBasicView()
        {
            while (!InBasicView())
            {
                Pixel myPixel = GetPixelByName("DetailedView");
                AutoClicker.LeftClickAtPosition(myPixel.Point);
            }
        }

        private bool InDetailedView()
        {
            Image myImg = GetImageByName("UpgradesDetailedView");

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            string text = GetTextRt(imageCurrentPath);

            MediumSleep();

            if (text.Contains("detailed view"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InBasicView()
        {
            Image myImg = GetImageByName("UpgradesBasicView");
            string imageStoredPath = myImg.GetFilePath();

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            double match = CompareImagesRt(imageStoredPath, imageCurrentPath);
            string text = GetTextRt(imageCurrentPath);

            if (match > 0.99 && text.Contains("basic view"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GoToMainScreen()
        {
            while (!InMainScreen())
            {
                Logging("Not in mainscreen");
                AutoClicker.SendKey("Firestone", "{ESC}");
                MediumSleep();
            }
            Logging("Arrived at mainscreen");
        }

        private bool InMainScreen()
        {
            Image myImg = GetImageByName("mainscreenBag");
            string imageStoredPath = myImg.GetFilePath();

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            double match = CompareImagesRt(imageStoredPath, imageCurrentPath);

            //string text1 = GetTextRt(imageStoredPath);
            //string text2 = GetTextRt(imageCurrentPath);

            if (match > 0.74)
            {
                if (InSaveAndExitScreen())
                {
                    AutoClicker.SendKey("Firestone", "{ESC}");
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InSaveAndExitScreen()
        {
            Image myImg = GetImageByName("SaveAndExit");
            string imageStoredPath = myImg.GetFilePath();

            Bitmap screenshot = TakeScreenshot(myImg.PointA, myImg.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string imageCurrentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png");

            double match = CompareImagesRt(imageStoredPath, imageCurrentPath);

            //string text1 = GetTextRt(imageStoredPath);
            //string text2 = GetTextRt(imageCurrentPath);

            if (match > 0.99)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Define a function to find an image and extract coordinates
        public Image GetImageByName(string imageName)
        {
            // Define the path to the images folder
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

            // Check if the directory exists
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("The specified directory does not exist.");
            }

            // Use regex to find files that match the pattern
            Regex filePattern = new Regex($"FSB_{imageName}_X([\\-0-9]+)Y([\\-0-9]+)_X([\\-0-9]+)Y([\\-0-9]+)\\.png");

            // Iterate through files in the directory
            foreach (string file in Directory.GetFiles(path))
            {
                // Check if the file name matches our pattern
                Match match = filePattern.Match(Path.GetFileName(file));
                if (match.Success)
                {
                    // Extract coordinates from the file name
                    int x1 = int.Parse(match.Groups[1].Value);
                    int y1 = int.Parse(match.Groups[2].Value);
                    int x2 = int.Parse(match.Groups[3].Value);
                    int y2 = int.Parse(match.Groups[4].Value);

                    // You can modify this part depending on how you want to use the coordinates
                    // This example returns the first set of coordinates                   
                    return new Image(file, imageName, new System.Drawing.Point(x1, y1), new System.Drawing.Point(x2, y2));
                }
            }

            // If no file is found, throw an exception or handle it as needed
            throw new FileNotFoundException($"No image matching the name '{imageName}' was found.");
        }

        public Pixel GetPixelByName(string pixelName)
        {
            // Define the path to the Pixels folder
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pixels");

            // Check if the directory exists
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("The specified directory does not exist.");
            }

            // Use regex to find files that match the pattern
            Regex filePattern = new Regex($"FSB_{Regex.Escape(pixelName)}_(#[0-9A-Fa-f]+)_X([\\-0-9]+)Y([\\-0-9]+)");

            // Iterate through files in the directory
            foreach (string file in Directory.GetFiles(path))
            {
                // Check if the file name matches our pattern
                Match match = filePattern.Match(Path.GetFileName(file));
                if (match.Success)
                {
                    // Extract hex color and coordinates from the file name
                    string hexColor = match.Groups[1].Value;
                    int x = int.Parse(match.Groups[2].Value);
                    int y = int.Parse(match.Groups[3].Value);

                    // Convert hex string to a Color object
                    MagickColor color = new MagickColor(hexColor);

                    // Create a System.Drawing.Point object with the extracted coordinates
                    System.Drawing.Point myPoint = new System.Drawing.Point(x, y);

                    // Return the extracted color and point
                    return new Pixel(pixelName, myPoint, color);
                }
            }

            // If no file is found, throw an exception or handle it as needed
            throw new FileNotFoundException($"No pixel matching the name '{pixelName}' was found.");
        }

        public static void CaptureScreen()
        {
            try
            {
                // Identify the leftmost monitor
                Screen leftmost = Screen.AllScreens.OrderBy(screen => screen.Bounds.X).First();

                // Create a new bitmap with the size of the entire screen
                Rectangle bounds = leftmost.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    // Use graphics to copy the screen into the bitmap
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
                    }

                    // Convert the Bitmap to a MagickImage
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Save to memory stream in PNG format
                        bitmap.Save(ms, ImageFormat.Png); // Make sure it's System.Drawing.Imaging.ImageFormat
                        ms.Position = 0; // Rewind the stream to start

                        // Load the image from the stream
                        using (MagickImage image = new MagickImage(ms))
                        {
                            // Optionally process the image (e.g., resize, filter)
                            // image.Resize(800, 600); // Example processing

                            // Save the image to a file
                            image.Write("screenshot.jpg");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error capturing screen: " + ex.Message);
            }
        }

        private void ButtonTestScroll_Click(object sender, EventArgs e)
        {
            try
            {
                SetOffset();
                ResearchScroll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonTestMapScroll_Click(object sender, EventArgs e)
        {
            try
            {
                SetOffset();
                CenterMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonTestMission_Click(object sender, EventArgs e)
        {
            try
            {
                SetOffset();
                DailyMissionsLiberations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DailyMissionsLiberations()
        {
            if (cbTaskDailyMissionsLiberations.Checked == false)
            {
                return;
            }

            Logging("Start daily missions liberation");

            AutoClicker.SendKey("Firestone", "{M}");
            MediumSleep();

            Pixel MapMissionsTankIcon = GetPixelByName("MapMissionsTankIcon");
            AutoClicker.LeftClickAtPosition(MapMissionsTankIcon.Point);

            MediumSleep();

            Pixel DailyMissionsButton = GetPixelByName("DailyMissionsButton");
            AutoClicker.LeftClickAtPosition(DailyMissionsButton.Point);

            MediumSleep();

            Pixel LiberationMissionsButton = GetPixelByName("LiberationMissionsButton");
            AutoClicker.LeftClickAtPosition(LiberationMissionsButton.Point);

            MediumSleep();

            Pixel MissionsMenuCenter = GetPixelByName("MissionsMenuCenter");
            AutoClicker.LeftClickAtPosition(MissionsMenuCenter.Point);

            MediumSleep();

            // Scroll to the first 4 daily missions
            int direction = 1;
            AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 70);

            MediumSleep();

            // Check if mission 1 is available
            Image Liberate = GetImageByName("Liberate1");
            Bitmap screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            string CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission1 needs to be done");

                Pixel Liberate1 = GetPixelByName("Liberate1");
                AutoClicker.LeftClickAtPosition(Liberate1.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 1 is ready.");
            }
            MediumSleep();

            // Check if mission 2 is available
            Liberate = GetImageByName("Liberate2");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission2 needs to be done");

                Pixel Liberate2 = GetPixelByName("Liberate2");
                AutoClicker.LeftClickAtPosition(Liberate2.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 2 is ready.");
            }
            MediumSleep();

            // Check if mission 3 is available
            Liberate = GetImageByName("Liberate3");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission3 needs to be done");

                Pixel Liberate3 = GetPixelByName("Liberate3");
                AutoClicker.LeftClickAtPosition(Liberate3.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 3 is ready.");
            }
            MediumSleep();

            // Check if mission 4 is available
            Liberate = GetImageByName("Liberate4");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission4 needs to be done");

                Pixel Liberate4 = GetPixelByName("Liberate4");
                AutoClicker.LeftClickAtPosition(Liberate4.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 4 is ready.");
            }
            MediumSleep();

            // Show next 4 daily missions
            direction = -1;
            AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 48);

            MediumSleep();

            // Check if mission 5 is available
            Liberate = GetImageByName("Liberate5");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission5 needs to be done");

                Pixel Liberate5 = GetPixelByName("Liberate5");
                AutoClicker.LeftClickAtPosition(Liberate5.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 5 is ready.");
            }
            MediumSleep();

            // Check if mission 6 is available
            Liberate = GetImageByName("Liberate6");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission6 needs to be done");

                Pixel Liberate6 = GetPixelByName("Liberate6");
                AutoClicker.LeftClickAtPosition(Liberate6.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 6 is ready.");
            }
            MediumSleep();

            // Check if mission 7 is available
            Liberate = GetImageByName("Liberate7");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission7 needs to be done");

                Pixel Liberate7 = GetPixelByName("Liberate7");
                AutoClicker.LeftClickAtPosition(Liberate7.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 7 is ready.");
            }
            MediumSleep();

            // Check if mission 8 is available
            Liberate = GetImageByName("Liberate8");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission8 needs to be done");

                Pixel Liberate8 = GetPixelByName("Liberate8");
                AutoClicker.LeftClickAtPosition(Liberate8.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 8 is ready.");
            }
            MediumSleep();

            // Show next 2 daily missions
            direction = -1;
            AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 22);

            MediumSleep();

            // Check if mission 9 is available
            Liberate = GetImageByName("Liberate9");
            screenshot = TakeScreenshot(Liberate.PointA, Liberate.PointB);
            screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
            CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
            if (CurrentText.ToLower().Trim() == "liberate")
            {
                Console.WriteLine("Mission9 needs to be done");

                Pixel Liberate9 = GetPixelByName("Liberate9");
                AutoClicker.LeftClickAtPosition(Liberate9.Point);
                MediumSleep();

                CurrentText = "";
                while (CurrentText.Trim().ToUpper() != "OK")
                {
                    Image LiberateReward = GetImageByName("LiberateReward");
                    screenshot = TakeScreenshot(LiberateReward.PointA, LiberateReward.PointB);
                    screenshot.Save("images/temp.png", System.Drawing.Imaging.ImageFormat.Png);
                    CurrentText = GetTextRt(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\temp.png"));
                    LongSleep();
                }
                Pixel LiberateRewardButton = GetPixelByName("LiberateRewardButton");
                AutoClicker.LeftClickAtPosition(LiberateRewardButton.Point);
                MediumSleep();
                Logging("Daily mission 9 is ready.");
            }
            MediumSleep();

            GoToMainScreen();

            Logging("Finished daily missions");
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.DoResearch1 = cbEnableResearch1.Checked;
            Properties.Settings.Default.DoResearch2 = cbEnableResearch2.Checked;

            Properties.Settings.Default.ResearchScrollAmount1 = TextResearchScroll1.Text;
            Properties.Settings.Default.ResearchScrollAmount2 = TextResearchScroll2.Text;

            Properties.Settings.Default.ResearchScrollLeft1 = cbScrollLeft1.Checked;
            Properties.Settings.Default.ResearchScrollLeft2 = cbScrollLeft2.Checked;

            Properties.Settings.Default.UseDustForGuardian = CheckboxUseStrangeDust.Checked;

            Properties.Settings.Default.TrainedGuardian = ComboboxGuardian.SelectedIndex;
            Properties.Settings.Default.UpgradeMode = ComboboxUpgradeMode.SelectedIndex;
            Properties.Settings.Default.StopSubUpgradesAfterStage = TextSwitchToPrimaryHeroAfterStage.Text;

            Properties.Settings.Default.TotalSquads = TextTotalSquads.Text;
            Properties.Settings.Default.offsetX = TextOffsetX.Text;

            Properties.Settings.Default.cycleDurationSeconds = TextAutoclickDuration.Text;

            Properties.Settings.Default.TaskUpgradeHeroes = cbTaskUpgradeHeroes.Checked;
            Properties.Settings.Default.TaskOracleRituals = cbTaskOracleRituals.Checked;
            Properties.Settings.Default.TaskGuildExpeditions = cbTaskGuildExpeditions.Checked;
            Properties.Settings.Default.TaskAlchemist = cbTaskAlchemist.Checked;
            Properties.Settings.Default.TaskFirestoneResearch = cbTaskFirestoneResearch.Checked;
            Properties.Settings.Default.TaskMeteoriteResearch = cbTaskMeteoriteResearch.Checked;
            Properties.Settings.Default.TaskTrainGuardian = cbTaskTrainGuardian.Checked;
            Properties.Settings.Default.TaskMapMissions = cbTaskMapMissions.Checked;
            Properties.Settings.Default.TaskDailyMissionsLiberations = cbTaskDailyMissionsLiberations.Checked;
            Properties.Settings.Default.TaskDailyMissionsDungeons = cbTaskDailyMissionsDungeons.Checked;
            Properties.Settings.Default.TaskCampaignLoot = cbTaskCampaignLoot.Checked;
            Properties.Settings.Default.TaskEngineerReward = cbTaskEngineerReward.Checked;
            Properties.Settings.Default.TaskPickaxes = cbTaskPickaxes.Checked;
            Properties.Settings.Default.TaskQuests = cbTaskQuests.Checked;
            Properties.Settings.Default.TaskDailyReward = cbTaskDailyReward.Checked;
            Properties.Settings.Default.TaskAutoclick = cbTaskAutoclick.Checked;

            Properties.Settings.Default.AlchemistDragonBlood = cbAlchemistDragonBlood.Checked;
            Properties.Settings.Default.AlchemistStrangeDust = cbAlchemistStrangeDust.Checked;
            Properties.Settings.Default.AlchemistExoticCoin = cbAlchemistExoticCoin.Checked;

            Properties.Settings.Default.Save();
        }

        private void EngineerReward()
        {
            if (cbTaskEngineerReward.Checked == false)
            {
                return;
            }

            Logging("Start engineer reward");

            AutoClicker.SendKey("Firestone", "{T}");
            MediumSleep();

            Pixel TownEngineer = GetPixelByName("TownEngineer");
            AutoClicker.LeftClickAtPosition(TownEngineer.Point);

            MediumSleep();

            Pixel TownEngineerEngineer = GetPixelByName("TownEngineerEngineer");
            AutoClicker.LeftClickAtPosition(TownEngineerEngineer.Point);

            MediumSleep();

            Pixel TownEngineerTab1 = GetPixelByName("TownEngineerTab1");
            AutoClicker.LeftClickAtPosition(TownEngineerTab1.Point);

            MediumSleep();

            Pixel TownEngineerClaim = GetPixelByName("TownEngineerClaim");
            AutoClicker.LeftClickAtPosition(TownEngineerClaim.Point);

            MediumSleep();

            GoToMainScreen();

            Logging("Finished engineer reward");
        }

        private void CampaignLoot()
        {
            if (cbTaskCampaignLoot.Checked == false)
            {
                return;
            }

            Logging("Start campaign loot");

            AutoClicker.SendKey("Firestone", "{M}");
            MediumSleep();
            MediumSleep();

            Pixel MapMissionsTankIcon = GetPixelByName("MapMissionsTankIcon");
            AutoClicker.LeftClickAtPosition(MapMissionsTankIcon.Point);

            MediumSleep();
            MediumSleep();

            Pixel DailyMissionsButton = GetPixelByName("CampaignLootClaim");
            AutoClicker.LeftClickAtPosition(DailyMissionsButton.Point);

            MediumSleep();
            MediumSleep();

            GoToMainScreen();

            Logging("Finished campaign loot");
        }

        private void btnTestSearchMission_Click(object sender, EventArgs e)
        {
            try
            {
                SetOffset();
                MapMissions(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SetOffset();
                Pixel MissionsMenuCenter = GetPixelByName("DungeonsMenuCenter");
                AutoClicker.LeftClickAtPosition(MissionsMenuCenter.Point);
                MediumSleep();
                int direction = 1;
                AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 70);
                MediumSleep();
                direction = -1;
                AutoClicker.ScrollMouseWheel(MissionsMenuCenter.Point, direction, 48);
                MediumSleep();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
          
        }
    }
}