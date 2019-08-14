﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandrakeEvents.Machine.CameraControl
{
    /// <summary>
    /// Consts for Camera Control Class
    /// </summary>
    /// May add methods to change the consts
    public static class CameraConst
    {
        public static string CameraState = "Not Ready"; //state of camera 
        public static string CameraSettingsPath = @"C:\"; //where are camera settings saved
        public static string SaveFolderPath; //save folder for photo capturing
        public static string FileName; //name of saved images

    }
}