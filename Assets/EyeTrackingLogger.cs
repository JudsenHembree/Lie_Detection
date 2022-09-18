using System.Collections;
using System.Collections.Generic;
using System.IO;
using ViveSR.anipal.Eye;
using UnityEngine;

public class EyeTrackingLogger 
{
public string FullLogFile;
public string PupilLogFile;

public FileStream full;
public FileStream pupil;
private StreamWriter sw;
private StreamWriter psw;
 
    public EyeTrackingLogger(string ful, string pup){
        FullLogFile = ful;
        PupilLogFile = pup;
        GenerateLogs();
        LabelLogs();
    }
    ~EyeTrackingLogger(){
        sw.Close();
        psw.Close();
        full.Close();
        pupil.Close();
    }

    public void GenerateLogs(){
        full = File.Create(FullLogFile);
        pupil = File.Create(PupilLogFile);
        sw = new StreamWriter(full);
        psw = new StreamWriter(pupil);
    }
    
    public void LabelLogs(){
        Debug.Log("FullLogFile: "+ FullLogFile);
        Debug.Log("PupilLogFile: "+ PupilLogFile);

        sw.WriteLine("Timestamp, Frame Sequence, Left Eye gaze origin x, Left Eye gaze origin y, Left Eye gaze origin z, Left eye gaze direction normalized x, Left eye gaze direction normalized y, Left eye gaze direction normalized z, left pupil diameter mm, left eye openness, Right Eye gaze origin x, Right Eye gaze origin y, Right Eye gaze origin z, Right eye gaze direction normalized x, Right eye gaze direction normalized y, Right eye gaze direction normalized z, right pupil diameter mm, right eye openness");

        psw.WriteLine("Timestamp, Left pupil_Diameter_mm, Right pupil_diameter_mm");

    }

    public void FullLog(EyeData eyeData){
        sw.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}", eyeData.timestamp, eyeData.frame_sequence, eyeData.verbose_data.left.gaze_origin_mm[0], eyeData.verbose_data.left.gaze_origin_mm[1], eyeData.verbose_data.left.gaze_origin_mm[2], eyeData.verbose_data.left.gaze_direction_normalized[0], eyeData.verbose_data.left.gaze_direction_normalized[1], eyeData.verbose_data.left.gaze_direction_normalized[2], eyeData.verbose_data.left.pupil_diameter_mm,  eyeData.verbose_data.left.eye_openness,        eyeData.verbose_data.right.gaze_origin_mm[0], eyeData.verbose_data.right.gaze_origin_mm[1], eyeData.verbose_data.right.gaze_origin_mm[2], eyeData.verbose_data.right.gaze_direction_normalized[0], eyeData.verbose_data.right.gaze_direction_normalized[1], eyeData.verbose_data.right.gaze_direction_normalized[2], eyeData.verbose_data.right.pupil_diameter_mm,  eyeData.verbose_data.right.eye_openness);
    }
    public void PupilLog(EyeData eyeData){
        psw.WriteLine("{0}, {1}, {2}", eyeData.timestamp, eyeData.verbose_data.left.pupil_diameter_mm, eyeData.verbose_data.right.pupil_diameter_mm);
    }
}
