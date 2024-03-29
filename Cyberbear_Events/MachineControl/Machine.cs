﻿using log4net;
using Cyberbear_Events.MachineControl.CameraControl;
using Cyberbear_Events.MachineControl.LightingControl;
using Cyberbear_Events.MachineControl.GrblArdunio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static Cyberbear_Events.MachineControl.LightingControl.LightsArdunio;
using System.Threading;
using Cyberbear_View.Consts;

namespace Cyberbear_Events.MachineControl
{
    /// <summary>
    /// Machine will be the class that contains everything needed for the
    /// machine to run and be connected
    /// </summary>
    public class Machine
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private GRBLArdunio grblArdunio;
        private LightsArdunio litArdunio;
        private Camera cameraControl;
        private TimelapseConst timelapseConst;
        private GRBLArdunio_Constants grblArdunio_Constants;
        private string name; //name of machine, may be user added or by front end automatically, or window
        private int numPlants;
        private CameraConst cameraConst;
        private bool growlightsOn;
        private bool dayNightCycleEnable = false; //day night cycle is not enabled unless checked box checked
        private int numOfPositions; //number of positions of machine, determined by grbl file chosen in GUI

        public GRBLArdunio GrblArdunio { get => grblArdunio; set => grblArdunio = value; }
        public LightsArdunio LitArdunio { get => litArdunio; set => litArdunio = value; }
        public Camera CameraControl { get => cameraControl; set => cameraControl = value; }
        public string Name { get => name; set => name = value; }
        public TimelapseConst TimelapseConst { get => timelapseConst; set => timelapseConst = value; }
        public int NumPlants { get => numPlants; set => numPlants = value; }
        public GRBLArdunio_Constants GrblArdunio_Constants { get => grblArdunio_Constants; set => grblArdunio_Constants = value; }
        public CameraConst CameraConst { get => cameraConst; set => cameraConst = value; }
        public bool GrowlightsOn { get => growlightsOn; set => growlightsOn = value; }
        public bool DayNightCycleEnable { get => dayNightCycleEnable; set => dayNightCycleEnable = value; }
        public int NumOfPositions { get => numOfPositions; set => numOfPositions = value; }
        public bool LongerWaitCheck = false; //for single axis machines like the Gassman machine
        public int Max_pos;



        //constructor
        /// <summary>
        /// Will initalize a new machine object and subsaquent other things
        /// </summary>
        public Machine()
        {
            log.Info("New machine start named: " + name);

            GrblArdunio = new GRBLArdunio();
            log.Info("New Grbl Ardunio added");
            litArdunio = new LightsArdunio();
            log.Info("New Lights Ardunuio added");
            cameraControl = new Camera();
            log.Info("New Camera Control added");

            TimelapseConst = new TimelapseConst(); //for timelapses
            GrblArdunio_Constants = new GRBLArdunio_Constants();
            cameraConst = new CameraConst();
        }

        /// <summary>
        /// Connects Machine object by connecting GRBL ardunio, lights ardunio, and camera control. Returns boolean for success or failure
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            try
            {
                //testing machine object
                if (GrblArdunio.Connected == false)
                {
                    GrblArdunio.Connect();
                    GrblArdunio.SoftReset();
                    log.Info("GRBL Ardunio Connected");
                    Thread.Sleep(1000);
                }
                //if (litArdunio.Connected == false)
                //{
                //    litArdunio.Connect();
                //    log.Info("Lights Ardunio Connected");

                //    //TODO set lights to white when turned on
                //    //setLightWhite();
                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Disconnects Machine object from window when called
        /// </summary>
        public void Disconnect()
        {
            try
            {
                //if grbl connected
                if (GrblArdunio.Connected == true)
                {
                    GrblArdunio.Disconnect();
                    log.Info("Grbl Ardunio Disconnected");
                }
                ////if lit aruindio connected
                //if (litArdunio.Connected == true)
                //{
                //    LightOff(); //turn lights off before disconnecting

                //    litArdunio.Disconnect();
                //    log.Info("Lights Ardunio Disconnected");
                //}

                cameraControl.ShutdownVimba(); //shutdown vimba
                log.Info("Camera Control Shutdown");

                log.Info("Machine Disconnected");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }


        /// <summary>
        /// Single Cycle of machine
        /// </summary>
        public void SingleCycle()
        {
            //resetting grbl to avoid any issues
            GrblArdunio.SoftReset();
///<<<<<<< HEAD
            ///Thread.Sleep(1000);
///=======
            Thread.Sleep(2000);
///>>>>>>> New_Lights_with_relays

            //creating folders and subfolders for the experiment
            //Only have to do once
            log.Info("Creating folders for positions");

            string expFolder = CameraControl.CameraConst.SaveFolderPath;
            DirectoryInfo dir = new DirectoryInfo(expFolder);

            //single cycle

            cameraControl.ImageAcquiredEvent += CameraControl_ImageAcquiredEvent;

            log.Info("Starting a Manual Cycle");

            string filePath = GrblArdunio_Constants.GRBLFilePath; //for second workstation testing

            log.Debug("Using the file: " + filePath);

            List<string> lines = File.ReadAllLines(filePath).ToList(); //putting all the lines in a list
            bool firstHome = true; //first time homing in cycle

            int length = lines.Count;

            //List<string> sublist = lines.GetRange(1, length - 1)

            for(int i = lines.Count; i > NumOfPositions + 2; i--)
            {
                lines.RemoveAt(i - 2);
            }

            for (int i = 1; i <= NumOfPositions; i++)
            {
                Name = "Position" + i;

                string subPath = Path.Combine(expFolder, Name);

                //DirectoryInfo pos = dir.CreateSubdirectory(subPath);
                DirectoryInfo pos = Directory.CreateDirectory(subPath);

            }

            LightOn();
            setLightWhiteMachine();

            //log.Debug("Backlights set to white");

            if (lines.Count == 0 || lines[0] != "$H")
            {
                MessageBox.Show("Please check that correct GRBLCommand file is selected.");
                return; // exit function
            }

            foreach (string line in lines)
            {
                //X commands will take photos
                GrblArdunio.SendLine(line); //sending line to ardunio
                log.Info("G Command Sent: " + line);

                if (line == "$H" && firstHome)
                {
                    Thread.Sleep(6000); //6 secs t0 home and not miss positions
                    firstHome = false;
                }

                if (line.Contains('Y'))
                {
                   System.Threading.Thread.Sleep(4000);
                }

                if (line == "$HY")
                {
                    Thread.Sleep(12000);
                }

                //if line not homing command then take pics
                if (!line.Contains('H'))
                {
                    if (!line.Contains('Y')) //if not moving y axis then take pics
                    {
                        Thread.Sleep(5000); //presleep for capture to adjust

                        cameraControl.CameraConst.positionNum++;

                        cameraControl.CapSaveImage(); //capture image

                        Thread.Sleep(2000); //sleep for 1 seconds

                    }
                }

                
            }

            //turn lights off (and growlights if night)
            LightOff();

            cameraControl.CameraConst.positionNum = 0; //reseting position after single cycle

            GC.Collect();
        }

        /// <summary>
        /// Camera Control registers when the Image Acquired Event is raised
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraControl_ImageAcquiredEvent(object sender, EventArgs e)
        {
            //log.Debug("Photo taken by program");
        }

        public void loadCameraSettingsMachine()
        {
            try
            {
                cameraControl.loadCameraSettings();
            }
            catch(Exception ex)
            {
                log.Error("Camera Settings failed to load because: " + ex.Message);
                MessageBox.Show("Camera settings failed to load because: " + ex.Message);
            }
        }

        #region Timelapse
        /// <summary>
        /// returns next timelapse starting time
        /// </summary>
        /// <returns></returns>
        public string UpdateNextTimelapse()
        {
            return TimelapseConst.TlStartDate.ToString();
        }
        #endregion

        #region Exception Handler
        /// <summary>
        /// Exception Handler when expections happens
        /// </summary>
        /// <param name="task">The task were the expection occurred is passed to the method
        /// to log the error</param>
        static void ExceptionHandler(Task task)
        {
            var exception = task.Exception;
            log.Error(exception);
        }

        /// <summary>
        /// To accesss the machine's timelapse start date and make it into a string for a textbox
        /// </summary>
        /// <returns>A string version of the timelapse starting date</returns>
        public string TimelapseStartDate()
        {
            return TimelapseConst.TlStartDate.ToString();
        }

        /// <summary>
        /// Accesses the machine's end date for the timelapse
        /// </summary>
        /// <returns>Returns string version of ending time for timelapse</returns>
        public string TimelapseEndDate()
        {
            return timelapseConst.TlEnd;
        }
        #endregion

        #region Lighting Control
        /// <summary>
        /// When called, turns Growlights on in machine object
        /// </summary>
        public void GrowLightOn()
        {
            try
            {
                Task task = new Task(() => LitArdunio.SetLight(Peripheral.GrowLight, true));
                task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task.Start();

                growlightsOn = true;
            }
            catch(Exception ex)
            {
                log.Error("Growlights failed to turn on because: " + ex.Message);
                MessageBox.Show("Growlights failed to turn on because: " + ex.Message);
            }
            
        }
        /// <summary>
        /// When called, turns growlights off
        /// </summary>
        public void GrowLightOff()
        {
            try
            {
                Task task = new Task(() => LitArdunio.SetLight(Peripheral.GrowLight, false));
               task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
               task.Start();

               growlightsOn = false;
            }
            catch(Exception ex)
            {
                log.Error("Growlights failed to turn off because: " + ex.Message);
                MessageBox.Show("Growlights failed to turn off because: " + ex.Message);
            }
        }

        /// <summary>
        /// When called, turns camera light on in machine object
        /// </summary>
        public void CameraLightOn()
        {
            try
            {
                Task task = new Task(() => LitArdunio.SetLight(Peripheral.Backlight, true));
                task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task.Start();

                growlightsOn = true;
            }
            catch (Exception ex)
            {
                log.Error("Growlights failed to turn on because: " + ex.Message);
                MessageBox.Show("Growlights failed to turn on because: " + ex.Message);
            }

        }

        /// <summary>
        /// When called, turns camera light off
        /// </summary>
        public void CameraLightOff()
        {
            try
            {
                Task task = new Task(() => LitArdunio.SetLight(Peripheral.Backlight, false));
                task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task.Start();

                growlightsOn = false;
            }
            catch (Exception ex)
            {
                log.Error("Growlights failed to turn off because: " + ex.Message);
                MessageBox.Show("Growlights failed to turn off because: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets lights to white on lights ardunio
        /// </summary>
        public void setLightWhiteMachine()
        {
            try
            {
                Task task = new Task(() => LitArdunio.SetBacklightColorWhite());
                task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task.Start();
            }
            catch(Exception ex)
            {
                log.Error("Lights failed to change to white because: " + ex.Message);
                MessageBox.Show("Lights failed to change to white because: " + ex.Message);
            }
        }

        public void LightOn()
        {
            if(litArdunio.LightStatus == false)
            {
                //turn off growlights if on
                Task task = new Task(() => LitArdunio.SetLight(Peripheral.GrowLight, false));
                task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task.Start();

                //turn on backlights
                Task task1 = new Task(() => LitArdunio.SetLight(Peripheral.Backlight, true));
                task1.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                task1.Start();

                litArdunio.LightStatus = true; //lights are on

            }
            else
            {
                return;
            }
        }

        public void LightOff()
        {
            try
            {
                if(litArdunio.LightStatus == true)
                {
                    //turn off backlights
                    Task task = new Task(() => LitArdunio.SetLight(Peripheral.Backlight, false));
                    task.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                    task.Start();



                    litArdunio.LightStatus = false; //lights are off 

                    if(dayNightCycleEnable == false)
                    {
                        //turn on growlights
                        Task task2 = new Task(() => LitArdunio.SetLight(Peripheral.GrowLight, true));
                        task2.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                        task2.Start();
                    }
                    else //if night day cycle enabled then check if night before turning lights back on
                    {
                        if (litArdunio.IsNightTime() != true) //if not night time
                        {
                            //turn on growlights
                            Task task2 = new Task(() => LitArdunio.SetLight(Peripheral.GrowLight, true));
                            task2.ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
                            task2.Start();
                        } 
                    }
                }
                else
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                log.Error("Lights failed to turn off because: " + ex.Message);
                MessageBox.Show("Lights failed to turn off because: " + ex.Message);
            }
            
        }

        /// <summary>
        /// Checks if nightime then turns growlights off
        /// </summary>
        public void NightTimeLightOff()
        {
            try
            {
                if (litArdunio.IsNightTime() == true) //if night time
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(500);
                        GrowLightOff();
                    });
                }
                else
                {
                    if(growlightsOn == false) //if lights not on, like after coming off of night, turn on
                    {
                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(500);
                            GrowLightOn();
                        });

                    }
                    
                }
            }
            catch(Exception ex)
            {
                log.Error("Failed to turn growlights off during nighttime check because: " + ex.Message);
                MessageBox.Show("Failed to turn off during nighttime check because: " + ex.Message);
            }
        }

        /// <summary>
        /// soft reset Grbl Ardunio for machine
        /// </summary>
        public void SoftReset()
        {
            try
            {
                GrblArdunio.SoftReset();
            }
            catch(Exception ex)
            {
                log.Error("Failed to soft reset Grbl Arduino because: " + ex.Message);
                MessageBox.Show("Failed to soft reset Grbl Arduino because: " + ex.Message);
            }
        }
        #endregion

        //add camera const changes
        public void CameraSettingsPathChange(string filepath)
        {
            try
            {
                CameraControl.CameraConst.CameraSettingsPath = filepath;
            }
            catch(Exception ex)
            {
                log.Error("Failed to change camera settings path because: " + ex.Message);
                MessageBox.Show("Failed to change camera settings path because: " + ex.Message);
            }
        }

        public void CameraSaveFolderPathChange(string folderResult)
        {
            try
            {
                CameraControl.CameraConst.SaveFolderPath = folderResult;
            }
            catch (Exception ex)
            {
                log.Error("Failed to change camera save folder path because: " + ex.Message);
                MessageBox.Show("Failed to change camera save folder path because: " + ex.Message);
            }
        }

        public void GRBLCommandFileChange(string fileResult)
        {
            try
            {
                GrblArdunio_Constants.GRBLFilePath = fileResult;
            }
            catch (Exception ex)
            {
                log.Error("Failed to change GRBL command file path because: " + ex.Message);
                MessageBox.Show("Failed to change GRBL command file path because: " + ex.Message);
            }
        }
    }
}
