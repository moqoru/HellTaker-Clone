using UnityEngine;

public enum BGMType
{
    None = -1,
    Opening,
    Game,
    Ending,
}

public enum SFXType
{
    None = -1,
    TransitionOn,
    TransitionOff,
    PlayerMove,
    PlayerDamage,
    PlayerDeath,
    BlockKick,
    BlockMove,
    MonsterMove,
    MonsterDestroy,
    KeyGet,
    LockBoxOpen,
    DialogueOpen,
    DialogueAdvance,
    DialogueSelect,
    DialogueConfirm,
    DialogueDeath,
    DialogueSuccess,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip openingBGM; // Apropos
    public AudioClip gameBGM; // Vitality
    public AudioClip endingBGM; // Luminescent

    [Header("Sound Effects")]
    public AudioClip transitionOn; // transition_on
    public AudioClip transitionOff; // transition_off
    public AudioClip playerMove; // player_move
    public AudioClip playerDamage; // player_damage
    public AudioClip playerDeath; // player_death
    public AudioClip blockKick; // stone_kick_001
    public AudioClip blockMove; // stone_move_001
    public AudioClip monsterMove; // enemy_kick_001
    public AudioClip monsterDestroy; // enemy_die_001
    public AudioClip keyGet; // key_pick_up
    public AudioClip lockBoxOpen; // lockbox_open
    public AudioClip dialogueOpen; // dialogue_start
    public AudioClip dialogueAdvance; // booper_click
    public AudioClip dialogueSelect; // dialogue_button_focus
    public AudioClip dialogueConfirm; // dialogue_button_confirm
    public AudioClip dialogueDeath; // bad_end_screen
    public AudioClip dialogueSuccess; // dialogue_success

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        // 싱글톤 중복 방지
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    #region BGM Methods
    public void PlayBGM(BGMType type)
    {
        AudioClip clip = GetBGMClip(type);
        if (clip != null)
        {
            PlayBGM(clip);
        }
    }
    
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    private AudioClip GetBGMClip(BGMType type)
    {
        switch (type)
        {
            case BGMType.Opening: return openingBGM;
            case BGMType.Game: return gameBGM;
            case BGMType.Ending: return endingBGM;
            case BGMType.None: return null;
            default:
                Debug.LogError($"[AudioManager] 이 BGM Type을 찾지 못했습니다: {type}");
                return null;
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }
    #endregion

    #region SFX Methods

    public void PlaySFX(SFXType type, float volumeScale = 1.0f)
    {
        AudioClip clip = GetSFXClip(type);
        if (clip != null)
        {
            PlaySFX(clip, volumeScale);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }
    }

    private AudioClip GetSFXClip(SFXType type)
    {
        switch (type)
        {
            case SFXType.TransitionOn: return transitionOn;
            case SFXType.TransitionOff: return transitionOff;
            case SFXType.PlayerMove: return playerMove;
            case SFXType.PlayerDamage: return playerDamage;
            case SFXType.PlayerDeath: return playerDeath;
            case SFXType.BlockKick: return blockKick;
            case SFXType.BlockMove: return blockMove;
            case SFXType.MonsterMove: return monsterMove;
            case SFXType.MonsterDestroy: return monsterDestroy;
            case SFXType.KeyGet: return keyGet;
            case SFXType.LockBoxOpen: return lockBoxOpen;
            case SFXType.DialogueOpen: return dialogueOpen;
            case SFXType.DialogueAdvance: return dialogueAdvance;
            case SFXType.DialogueSelect: return dialogueSelect;
            case SFXType.DialogueConfirm: return dialogueConfirm;
            case SFXType.DialogueDeath: return dialogueDeath;
            case SFXType.DialogueSuccess: return dialogueSuccess;
            case SFXType.None: return null;
            default:
                Debug.LogError($"[AudioManager] 이 SFX Type을 찾지 못했습니다: {type}");
                return null;
        }
    }
    #endregion

    #region VolumeControl

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    #endregion
}
