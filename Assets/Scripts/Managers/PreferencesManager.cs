using Systems.Persistence;

namespace Managers {
    public record AudioPreferencesChanged() : IEvent;

    public class PreferencesManager : PersistentSingleton<PreferencesManager>
    {
        private UserPreferences userPreferences = null!;

        protected override void Awake()
        {
            base.Awake();
            Bind(SaveLoadSystem.Instance.GetUserPreferences());
        }

        void Bind(UserPreferences preferences)
        {
            userPreferences = preferences;
            CardComparator.autoRoll = preferences.AutoRollEnabled;
            ScreenShakeHandler.IsScreenShakeEnabled = preferences.screenShakePreference.IsScreenShakeEnabled;
        }

        public void SetAutoRoll(bool value) {
            CardComparator.autoRoll = value;
            userPreferences.AutoRollEnabled = value;
            
            SaveLoadSystem.Instance.SavePreferences(); 
        }

        public void SetScreenShakeEnabled(bool value) {
            ScreenShakeHandler.IsScreenShakeEnabled = value;
            userPreferences.screenShakePreference.IsScreenShakeEnabled = value;
        }

        public void SetSFXVolume(float volume) {
            userPreferences.audioPreferences.SFXVolume = volume;
            new AudioPreferencesChanged().Invoke();
        }

        public void SetMusicVolume(float volume) {
            userPreferences.audioPreferences.BackgroundMusicVolume = volume;
            new AudioPreferencesChanged().Invoke();
        }

        public void SetSFXMuted(bool muted) {
            userPreferences.audioPreferences.SFXMuted = muted;
            new AudioPreferencesChanged().Invoke();
        }

        public void SetMusicMuted(bool muted) {
            userPreferences.audioPreferences.MusicMuted = muted;
            new AudioPreferencesChanged().Invoke();
        }

        public float GetMusicVolume() {
            return userPreferences.audioPreferences.BackgroundMusicVolume;
        }

        public float GetSFXVolume() {
            return userPreferences.audioPreferences.SFXVolume;
        }

        public bool GetMusicMuted() {
            return userPreferences.audioPreferences.MusicMuted;
        }

        public bool GetSFXMuted() {
            return userPreferences.audioPreferences.SFXMuted;
        }
    }
}