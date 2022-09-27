using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

using Random = System.Random;


public class exp_manager : MonoBehaviour
{
    public List<VideoPlayer> videos;
    public List<VideoClip> clips;

    public List<AudioClip> audioSources;
    private List<AudioClip> selected;

    public GameObject tv;
    public RenderTexture screen;

    public int desiredLies;
    public string questionStatus;
    private VideoPlayer playingVideo;
    private int questionNumber;
    private EyeTrackingLogger logger;

    public static class clip
    {
        public const int begin = 0;
        public const int baseline = 1;
        public const int truth = 2;
        public const int lie = 3;
    }

    void Start()
    {
        questionNumber = 0;
        initVidsList();
        StartCoroutine(run_exp());
        selected = generateLies();
        string lies = "";
        foreach(AudioClip item in selected){
            lies += item + " ";
        }
        Debug.Log(lies);
    }

    void Update(){
        if(logger != null){
            logger.FullLog(ViveSR.anipal.Eye.ciEyeTracking.eyeData, questionStatus);
            logger.PupilLog(ViveSR.anipal.Eye.ciEyeTracking.eyeData, questionStatus);
        }
    }

    public IEnumerator run_exp(){
        Debug.Log("run exp");
        // setup clip
        videos[clip.begin].Prepare();
        //Wait until this video is prepared
        while (!videos[clip.begin].isPrepared)
        {
            yield return null;
        }

        //Play first video
        playingVideo = videos[clip.begin];
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool halfway = false;
        while(playingVideo.isPlaying)
        {
            if(!halfway && playingVideo.time >= (playingVideo.clip.length / 2)){
                halfway = true;
                videos[clip.baseline].Prepare();
            }
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[clip.baseline];
        StartCoroutine(baseline());
    }

    private IEnumerator baseline(){
        Debug.Log("baseline");
        Debug.Log("Playing video is " + playingVideo);
        if(questionNumber > audioSources.Count-1){
            Debug.Log("Its over");
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
        bool halfway = false;
        Debug.Log("hmm =" + hmm);
        while(playingVideo.isPlaying){
            if(!halfway && playingVideo.time >= (playingVideo.clip.length / 2)){
                halfway = true;
                videos[hmm].Prepare();
            }
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[hmm];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        StartCoroutine(response());
    }

    private IEnumerator response(){
        Debug.Log("response");
        Debug.Log("Playing video is " + playingVideo);
        questionStatus = "response";
        // load screen with right indication
        playingVideo.targetTexture = screen;
        playingVideo.Play();
        bool halfway = false;
        while(playingVideo.isPlaying){
            if(!halfway && playingVideo.time >= (playingVideo.clip.length / 2)){
                halfway = true;
                videos[clip.baseline].Prepare();
            }
            yield return null;
        }
        playingVideo.targetTexture = null;
        playingVideo = videos[clip.baseline];
        while(!playingVideo.isPrepared){
            yield return null;
        }
        // play question

        // wait for response
        //yield return new WaitForSeconds(5);
        Debug.Log("returning to baseline");
        questionNumber++;
        StartCoroutine(baseline());
    }

    private List<AudioClip> generateLies(){
        Debug.Log("generateLies");
        int k = desiredLies; // items to select
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

    private int truthorlie(int clip){
        Debug.Log("truthorlie");
        return selected.Contains(audioSources[clip]) ? 3 : 2;
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

            //Disable Play on Awake for both Video and Audio
            videoPlayer.playOnAwake = false;
            audioSource.playOnAwake = false;

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

//    private IEnumerator playVideo(bool firstRun = true)
    //{
        //if (videos == null || videos.Count <= 0)
        //{
            //Debug.LogError("Assign VideoClips from the Editor");
            //yield break;
        //}

        ////Make sure that the NEXT VideoPlayer index is valid
        //if (videoIndex >= videos.Count)
            //yield break;

        ////Prepare video
        //videos[videoIndex].Prepare();

        ////Wait until this video is prepared
        //while (!videos[videoIndex].isPrepared)
        //{
            //Debug.Log("Preparing Index: " + videoIndex);
            //yield return null;
        //}
        //Debug.LogWarning("Done Preparing current Video Index: " + videoIndex);

        ////Assign the Texture from Video to RawImage to be displayed
        ////image.texture = videos[videoIndex].texture;

        ////Play first video
        //videos[videoIndex].Play();

        ////Wait while the current video is playing
        //bool reachedHalfWay = false;
        //int nextIndex = (videoIndex + 1);
        //while (videos[videoIndex].isPlaying)
        //{
            //Debug.Log("Playing time: " + videos[videoIndex].time + " INDEX: " + videoIndex);

            ////(Check if we have reached half way)
            //if (!reachedHalfWay && videos[videoIndex].time >= (videos[videoIndex].clip.length / 2))
            //{
                //reachedHalfWay = true; //Set to true so that we don't evaluate this again

                ////Make sure that the NEXT VideoPlayer index is valid. Othereise Exit since this is the end
                //if (nextIndex >= videos.Count)
                //{
                    //Debug.LogWarning("End of All Videos: " + videoIndex);
                    //yield break;
                //}

                ////Prepare the NEXT video
                //Debug.LogWarning("Ready to Prepare NEXT Video Index: " + nextIndex);
                //videos[nextIndex].Prepare();
            //}
            //yield return null;
        //}
        //Debug.Log("Done Playing current Video Index: " + videoIndex);

        ////Wait until NEXT video is prepared
        //while (!videos[nextIndex].isPrepared)
        //{
            //Debug.Log("Preparing NEXT Video Index: " + nextIndex);
            //yield return null;
        //}

        //Debug.LogWarning("Done Preparing NEXT Video Index: " + videoIndex);

        ////Increment Video index
        //videoIndex++;

        ////Play next prepared video. Pass false to it so that some codes are not executed at-all
        //StartCoroutine(playVideo(false));
    //}
}
