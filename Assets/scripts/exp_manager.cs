using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Diagnostics;
using System.IO;

using Random = System.Random;
using Debug = UnityEngine.Debug;

//TODO:
    // Create a introduction to set up the experiment
    // Real question audio
    // researcher input aka wait for enter fix screen ???

//Nice to haves
    // Real time
    // complex questions

public class exp_manager : MonoBehaviour
{
    private List<VideoPlayer> videos;
    public List<VideoClip> clips;

    public List<AudioClip> audioSources;
    public List<AudioClip> practiceAudioSources;
    public AudioSource audioSource;
    public AudioSource vidaudioSource;
    private List<AudioClip> selected;
    private List<AudioClip> practiceLies;

    public RenderTexture screen;

    public float answerTime;
    public int desiredLies;
    public int practiceLiesCount;
    public string questionStatus;
    private VideoPlayer playingVideo;
    private AudioClip currentQuestion;
    private int questionNumber;
    private EyeTrackingLogger logger;

    public static class clip
    {
        public const int begin = 0;
        public const int baseline = 1;
        public const int truth = 2;
        public const int lie = 3;
        public const int redoPractice = 4;
        public const int redoBeginning = 5;
    }

    void Start()
    {
        questionNumber = 0;
        initVidsList();
        selected = generateLies(desiredLies, audioSources);
        practiceLies = pgenerateLies(practiceLiesCount, practiceAudioSources);
        run_exp();
    }

    void Update(){
        if(logger != null){
            logger.FullLog(ViveSR.anipal.Eye.ciEyeTracking.eyeData, questionStatus);
            logger.PupilLog(ViveSR.anipal.Eye.ciEyeTracking.eyeData, questionStatus);
        }
    }

    public void run_exp(){
        Debug.Log("run exp");
        // setup clip
        StartCoroutine(beginning());
    }

    private IEnumerator beginning(){
        videos[clip.begin].SetTargetAudioSource(0, vidaudioSource);
        vidaudioSource.mute = false;
        videos[clip.begin].Prepare();

        //Wait until this video is prepared
        while (!videos[clip.begin].isPrepared)
        {
            yield return null;
        }

        //Play first video
        playingVideo = videos[clip.begin];
        playingVideo.targetTexture = screen;
        playingVideo.isLooping = false;
        playingVideo.Play();
        bool prepped = false;
        while(playingVideo.isPlaying)
        {
            if(!prepped){
                prepped = true;
                videos[clip.baseline].Prepare();
            }
            yield return null;
        }

        playingVideo.targetTexture = null;
        playingVideo = videos[clip.redoBeginning];
        StartCoroutine(startPractice());
    }

    private IEnumerator startPractice(){
        Debug.Log("Start practice? (n to continue r to repeat)");
        playingVideo.SetTargetAudioSource(0, vidaudioSource);
        playingVideo.isLooping = true;
        playingVideo.Prepare();
        while(!playingVideo.isPrepared){
            yield return null;
        }
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        while(!Input.GetKeyDown("n") && !Input.GetKeyDown("r")){
            yield return null;
        }
        if(Input.GetKeyDown("n")){
            playingVideo.Stop();
            playingVideo = videos[clip.baseline];
            playingVideo.Prepare();
            questionNumber = 0;
            StartCoroutine(practice());
        }
        if(Input.GetKeyDown("r")){
            playingVideo.Stop();
            StartCoroutine(beginning());
        }
    }

    private IEnumerator researcherInput(){
        questionNumber = 0;
        playingVideo = videos[clip.redoPractice];
        playingVideo.SetTargetAudioSource(0, vidaudioSource);
        playingVideo.isLooping = true;
        playingVideo.Prepare();
        while(!playingVideo.isPrepared){
            yield return null;
        }
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        Debug.Log("Repeat practice (n to continue r to repeat)");
        while(!Input.GetKeyDown("n") && !Input.GetKeyDown("r")){
            yield return null;
        }
        if(Input.GetKeyDown("n")){
            playingVideo.Stop();
            playingVideo = videos[clip.baseline];
            playingVideo.Prepare();
            StartCoroutine(baseline());
        }
        if(Input.GetKeyDown("r")){
            playingVideo.Stop();
            playingVideo = videos[clip.baseline];
            playingVideo.Prepare();
            StartCoroutine(practice());
        }
    }

    private IEnumerator practice(){
        Debug.Log("practice baseline");
        if(questionNumber > practiceAudioSources.Count-1){
            StartCoroutine(researcherInput());
            yield break;
        }
        while(!playingVideo.isPrepared){
            yield return null;
        }
        questionStatus = "practice baseline";
        int hmm = practicetruthorlie(questionNumber);
        if(logger == null){
            string one = "Assets/Data/Q" + questionNumber +"full.csv";
            string two = "Assets/Data/Q" + questionNumber + "partial.csv";
            logger = new EyeTrackingLogger(hmm, one, two);
        }else{
            logger.clean();
            string one = "Assets/Data/Q" + questionNumber +"full.csv";
            string two = "Assets/Data/Q" + questionNumber + "partial.csv";
            logger.rebuild(hmm, one, two);
        }
        if(hmm == 3){
            Debug.Log("Prompting for lie");
        } else{
            Debug.Log("Prompting for truth");
        }
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool prepped = false;
        float fiveSeconds = 5.0f;
        while(fiveSeconds>0){
            if(!prepped){
                prepped = true;
                videos[hmm].Prepare();
            }
            fiveSeconds-=Time.deltaTime;
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[hmm];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        StartCoroutine(practicequestioning());       
    }

    private IEnumerator baseline(){
        Debug.Log("baseline");
        if(questionNumber > audioSources.Count-1){
            Debug.Log("Its over");
            Application.Quit();            
            yield break;
        }
        while(!playingVideo.isPrepared){
            yield return null;
        }
        questionStatus = "baseline";
        int hmm = truthorlie(questionNumber);
        if(logger == null){
            string one = "Assets/Data/Q" + questionNumber +"full.csv";
            string two = "Assets/Data/Q" + questionNumber + "partial.csv";
            logger = new EyeTrackingLogger(hmm, one, two);
        }else{
            logger.clean();
            string one = "Assets/Data/Q" + questionNumber +"full.csv";
            string two = "Assets/Data/Q" + questionNumber + "partial.csv";
            logger.rebuild(hmm, one, two);
        }
        if(hmm == 3){
            Debug.Log("Prompting for lie");
        } else{
            Debug.Log("Prompting for truth");
        }
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool prepped = false;
        float fiveSeconds = 5.0f;
        while(fiveSeconds>0){
            if(!prepped){
                prepped = true;
                videos[hmm].Prepare();
            }
            fiveSeconds-=Time.deltaTime;
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[hmm];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        StartCoroutine(questioning());
    }

    private IEnumerator questioning(){
        Debug.Log("questioning");
        questionStatus = "questioning";
        // load screen with right indication
        currentQuestion = audioSources[questionNumber];
        audioSource.clip = currentQuestion;
        audioSource.Play();
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool prepped = false;
        while(audioSource.isPlaying){
            if(!prepped){
                prepped = true;
                videos[clip.baseline].Prepare();
            }
            yield return null;
        }
        questionStatus = "response";
        playingVideo.targetTexture = null;
        playingVideo = videos[clip.baseline];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        Debug.Log("returning to baseline");
        questionNumber++;
        StartCoroutine(response());
    }

    private IEnumerator practicequestioning(){
        Debug.Log("practice questioning");
        questionStatus = "questioning";
        // load screen with right indication
        currentQuestion = practiceAudioSources[questionNumber];
        audioSource.clip = currentQuestion;
        audioSource.Play();
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool prepped = false;
        while(audioSource.isPlaying){
            if(!prepped){
                prepped = true;
                videos[clip.baseline].Prepare();
            }
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[clip.baseline];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        Debug.Log("returning to baseline");
        questionNumber++;
        StartCoroutine(practiceresponse());
    }

    private IEnumerator response(){
        Debug.Log("Getting participant response");
        float timeleft = answerTime;
        while(timeleft >= 0.0f){
            timeleft -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(baseline());
    }

    private IEnumerator practiceresponse(){
        Debug.Log("Getting participant response");
        float timeleft = answerTime;
        while(timeleft >= 0.0f){
            timeleft -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(practice());
    }

    private List<AudioClip> generateLies(int lieCount, List<AudioClip> audioSources){
        Debug.Log("generateLies");
        int k = lieCount; // items to select
        var selected = new List<AudioClip>();
        double needed = k;
        double available = audioSources.Count;
        var rand = new Random();
        while (selected.Count < k) {
        if( rand.NextDouble() < needed / available ) {
            selected.Add(audioSources[(int)available-1]);
            needed--;
        }
        available--;
        }
        return selected;
    }

    private List<AudioClip> pgenerateLies(int lieCount, List<AudioClip> practiceAudioSources){
        Debug.Log("generateLies");
        int k = lieCount; // items to select
        var selected = new List<AudioClip>();
        double needed = k;
        double available = practiceAudioSources.Count;
        var rand = new Random();
        while (selected.Count < k) {
        if( rand.NextDouble() < needed / available ) {
            selected.Add(practiceAudioSources[(int)available-1]);
            needed--;
        }
        available--;
        }
        return selected;
    }

    private int truthorlie(int clip){
        Debug.Log("truthorlie");
        return selected.Contains(audioSources[clip]) ? 3 : 2;
    }

    private int practicetruthorlie(int clip){
        Debug.Log("truthorlie");
        return practiceLies.Contains(practiceAudioSources[clip]) ? 3 : 2;
    }

    private void initVidsList(){
        Debug.Log("initvidslist");
        videos = new List<VideoPlayer>();
        for (int i = 0; i < clips.Count; i++)
        {
            //Create new Object to hold the Video and the sound then make it a child of this object
            GameObject vidHolder = new GameObject("VP" + i);
            vidHolder.transform.SetParent(transform);

            //Add VideoPlayer to the GameObject
            VideoPlayer videoPlayer = vidHolder.AddComponent<VideoPlayer>();
            videos.Add(videoPlayer);
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = screen;

            //Add AudioSource to  the GameObject
            AudioSource audioSource = vidHolder.AddComponent<AudioSource>();
            audioSource.mute = true;

            //Disable Play on Awake for both Video and Audio
            videoPlayer.playOnAwake = false;
            audioSource.playOnAwake = false;

            //loop for time 
            videoPlayer.isLooping = true;

            //We want to play from video clip not from url
            videoPlayer.source = VideoSource.VideoClip;

            //Set Audio Output to AudioSource
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            videoPlayer.clip = clips[i];
        }
    }

    private static void ExecProcess(string name)
    {
        Process p = new Process();
        p.StartInfo.FileName = name;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = true;
        p.Start();

        p.WaitForExit();
        p.Close();
    }
}

