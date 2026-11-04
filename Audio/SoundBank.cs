using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace game_1.Audio;

// Every noise the game makes, in one place, so the rest of the code asks for a
// sound by what just happened rather than by what file to play.
//
// The audio is still being written, so anything the content pipeline does not
// have yet loads as nothing and plays as silence. The game runs either way,
// and each sound starts working the moment its asset is added.
public class SoundBank
{
    // Content pipeline asset names, listed together so what the game expects to
    // find is obvious without reading the rest of the class.
    private const string ShotAsset = "ShotSound";

    private const string MeleeAsset = "MeleeSound";

    private const string DamageAsset = "DamageSound";

    private const string EnemyDeathAsset = "EnemyDeathSound";

    private const string MusicAsset = "BackgroundMusic";

    // Music sits under the effects so a shot is still heard over it.
    private const float MusicVolume = 0.5f;

    private SoundEffect _shot;

    private SoundEffect _melee;

    private SoundEffect _damage;

    private SoundEffect _enemyDeath;

    private Song _music;

    public void LoadContent(ContentManager content)
    {
        _shot = TryLoad<SoundEffect>(content, ShotAsset);
        _melee = TryLoad<SoundEffect>(content, MeleeAsset);
        _damage = TryLoad<SoundEffect>(content, DamageAsset);
        _enemyDeath = TryLoad<SoundEffect>(content, EnemyDeathAsset);
        _music = TryLoad<Song>(content, MusicAsset);
    }

    public void PlayShot() => _shot?.Play();

    public void PlayMelee() => _melee?.Play();

    // Shared by the player and the mobs, so a hit sounds like a hit whoever
    // took it.
    public void PlayDamage() => _damage?.Play();

    public void PlayEnemyDeath() => _enemyDeath?.Play();

    // Loops under the run until it is stopped.
    public void StartMusic()
    {
        if (_music is null) return;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = MusicVolume;
        MediaPlayer.Play(_music);
    }

    public void StopMusic()
    {
        if (_music is null) return;

        MediaPlayer.Stop();
    }

    // An asset that has not been made yet is not an error worth stopping for:
    // it comes back as null and every call to play it is a no-op.
    private static T TryLoad<T>(ContentManager content, string asset) where T : class
    {
        try
        {
            return content.Load<T>(asset);
        }
        catch (ContentLoadException)
        {
            return null;
        }
    }
}
